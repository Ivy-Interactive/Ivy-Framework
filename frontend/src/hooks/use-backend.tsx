import { useState, useEffect, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { WidgetEventHandlerType, WidgetNode } from '@/types/widgets';
import { useToast } from '@/hooks/use-toast';
import { showError } from '@/hooks/use-error-sheet';
import { getIvyHost, getMachineId } from '@/lib/utils';
import { validateRedirectUrl, validateLinkUrl } from '@/lib/url';
import { logger } from '@/lib/logger';
import { applyPatch, Operation } from 'fast-json-patch';
import { cloneDeep } from 'lodash';
import { ToastAction } from '@/components/ui/toast';
import { setThemeGlobal } from '@/components/theme-provider';

type UpdateMessage = Array<{
  iteration: number;
  viewId: string;
  indices: number[];
  patch: Operation[];
  treeHash?: string;
}>;

type RefreshMessage = {
  widgets: WidgetNode;
};

type ErrorMessage = {
  title: string;
  type: string;
  description: string;
  stackTrace?: string;
};

type HistoryState = {
  tabId?: string;
};

type RedirectMessage = {
  url: string;
  replaceHistory: boolean;
  state: HistoryState;
};

type SetAuthCookiesMessage = {
  cookieJarId: string;
  reloadPage: boolean;
  triggerMachineReload: boolean;
};

type HttpTunnelRequestMessage = {
  requestId: string;
  method: string;
  url: string;
  headers?: Record<string, string[]>;
  body?: string; // Base64 encoded
  contentType?: string;
};

type HttpTunnelResponseMessage = {
  requestId: string;
  statusCode: number;
  headers?: Record<string, string[]>;
  body?: string; // Base64 encoded
  contentType?: string;
  errorMessage?: string;
};

const widgetTreeToXml = (node: WidgetNode) => {
  const tagName = node.type.replace('Ivy.', '');
  const attributes: string[] = [`Id="${escapeXml(node.id)}"`];
  if (node.props) {
    for (const [key, value] of Object.entries(node.props)) {
      const pascalCaseKey = key.charAt(0).toUpperCase() + key.slice(1);
      attributes.push(`${pascalCaseKey}="${escapeXml(String(value))}"`);
    }
  }
  let childrenXml = '';
  if (node.children && node.children.length > 0) {
    childrenXml = node.children.map(child => widgetTreeToXml(child)).join('');
    return `<${tagName} ${attributes.join(' ')}>${childrenXml}</${tagName}>`;
  } else {
    return `<${tagName} ${attributes.join(' ')} />`;
  }
};

const calculateTreeSignature = (node: WidgetNode): string => {
  const children =
    node.children?.map(c => calculateTreeSignature(c)).join(',') ?? '';
  return `${node.id}:${node.type}[${children}]`;
};

const hashString = (str: string): string => {
  // djb2 hash - simple and fast
  let hash = 5381;
  for (let i = 0; i < str.length; i++) {
    hash = (hash * 33) ^ str.charCodeAt(i);
  }
  return (hash >>> 0).toString(16).padStart(8, '0');
};

const calculateTreeHash = (node: WidgetNode): string => {
  const signature = calculateTreeSignature(node);
  return hashString(signature);
};

const escapeXml = (str: string) => {
  return str
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&apos;');
};

const getFullReplacementNode = (patch: Operation[]): WidgetNode | null => {
  if (patch.length === 1 && patch[0].op === 'replace' && patch[0].path === '') {
    return (patch[0] as { value: WidgetNode }).value;
  }
  return null;
};

// Pure function - no side effects, safe for React Strict Mode double-invocation
function applyUpdateMessage(
  tree: WidgetNode,
  updates: UpdateMessage
): WidgetNode | null {
  let newTree = cloneDeep(tree);

  for (const update of updates) {
    let parent = newTree;

    if (!parent) {
      logger.error('No parent found in applyUpdateMessage', { update });
      continue;
    }

    if (update.indices.length === 0) {
      const replacement = getFullReplacementNode(update.patch);
      if (replacement) {
        // Special case: full replacement of the root node
        newTree = replacement;
      } else {
        applyPatch(parent, update.patch);
      }
    } else {
      update.indices.forEach((index, i) => {
        if (i === update.indices.length - 1) {
          if (!parent.children) {
            logger.error('No children found in parent', { parent });
            return;
          }
          const target = parent.children[index];
          const replacement = getFullReplacementNode(update.patch);
          if (replacement) {
            // Special case: full replacement of the target node
            parent.children[index] = replacement;
          } else {
            applyPatch(target, update.patch);
          }
        } else {
          if (!parent) {
            logger.error('No parent found in applyUpdateMessage', { update });
            return;
          }
          if (!parent.children) {
            logger.error('No children found in parent', { parent });
            return;
          }
          if (index >= parent.children.length) {
            logger.error('Index out of bounds', {
              index,
              childrenLength: parent.children.length,
              parent,
            });
            return;
          }
          const nextParent = parent.children[index];
          if (!nextParent) {
            logger.error('Child at index is null/undefined', {
              index,
              childrenLength: parent.children.length,
              parentType: parent.type,
              parentId: parent.id,
            });
            return;
          }
          parent = nextParent;
        }
      });
    }

    if (update.treeHash) {
      //if we have the treeHash then we're in DEBUG
      const frontendHash = calculateTreeHash(newTree);
      if (frontendHash !== update.treeHash) {
        logger.error('Tree hash mismatch after update', {
          iteration: update.iteration,
          viewId: update.viewId,
          backendHash: update.treeHash,
          frontendHash,
        });
      }
    }
  }

  return newTree;
}

export const useBackend = (
  appId: string | null,
  appArgs: string | null,
  parentId: string | null,
  chrome: boolean
) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(
    null
  );
  const [widgetTree, setWidgetTree] = useState<WidgetNode | null>(null);
  const [disconnected, setDisconnected] = useState(false);
  const { toast } = useToast();
  const machineId = getMachineId();
  const connectionId = connection?.connectionId;
  const currentConnectionRef = useRef<signalR.HubConnection | null>(null);
  const lastIterationRef = useRef<number>(-1);

  // Use a ref that gets updated with the latest connection so we always have it in the callback
  const latestConnectionRef = useRef(connection);
  useEffect(() => {
    latestConnectionRef.current = connection;
  }, [connection]);

  // Stable values used in dependency arrays - only updated when we want to reconnect
  const [stableAppId, setStableAppId] = useState(appId);
  const [stableChrome, setStableChrome] = useState(chrome);

  // Refs to always have latest values in callbacks, without needing to add them to dependency arrays
  const latestAppIdRef = useRef(appId);
  const latestChromeRef = useRef(chrome);

  useEffect(() => {
    latestAppIdRef.current = appId;
  }, [appId]);

  useEffect(() => {
    latestChromeRef.current = chrome;
  }, [chrome]);

  const rootAppIdRef = useRef<string | undefined>(undefined);

  const isRootConnection = parentId === null;

  useEffect(() => {
    if (!isRootConnection) {
      // eslint-disable-next-line react-hooks/set-state-in-effect
      setStableAppId(appId);
      setStableChrome(chrome);
      return;
    }

    const rootAppId = rootAppIdRef.current;
    const shouldReconnect =
      !rootAppId ||
      (rootAppId === '$chrome' ? chrome === false : appId !== rootAppId);

    if (shouldReconnect) {
      setStableAppId(appId);
      setStableChrome(chrome);
    }
  }, [appId, chrome, isRootConnection]);

  useEffect(() => {
    if (import.meta.env.DEV && widgetTree) {
      const parser = new DOMParser();
      let xml;
      try {
        const xmlString = widgetTreeToXml(widgetTree);
        if (!xmlString) {
          logger.warn('Empty XML string generated from widget tree');
          return;
        }
        xml = parser.parseFromString(xmlString, 'application/xml');
        const parserError = xml.querySelector('parsererror');
        if (parserError) {
          logger.error('XML parsing error', { error: parserError.textContent });
          return;
        }
      } catch (error) {
        logger.error('Error converting widget tree to XML', { error });
        return;
      }
      logger.debug(`[${connectionId}]`, xml);
    }
  }, [widgetTree, connectionId]);

  const handleRefreshMessage = useCallback((message: RefreshMessage) => {
    // Reset iteration tracking on full refresh
    lastIterationRef.current = -1;
    setWidgetTree(message.widgets);
  }, []);

  const handleUpdateMessage = useCallback((message: UpdateMessage) => {
    const lastSeen = lastIterationRef.current;
    const newUpdates = message.filter(u => u.iteration > lastSeen);

    if (newUpdates.length === 0) {
      return;
    }

    // Check for lost iterations
    const expectedNext = lastSeen + 1;
    const firstNew = Math.min(...newUpdates.map(u => u.iteration));
    if (lastSeen >= 0 && firstNew > expectedNext) {
      logger.error('Lost iteration(s) detected', {
        lastIteration: lastSeen,
        receivedIteration: firstNew,
      });
    }
    const maxIteration = Math.max(...newUpdates.map(u => u.iteration));
    lastIterationRef.current = maxIteration;

    setWidgetTree(currentTree => {
      if (!currentTree) {
        logger.warn('No current widget tree available for update');
        return null;
      }
      return applyUpdateMessage(currentTree, newUpdates);
    });
  }, []);

  const handleHotReloadMessage = useCallback(() => {
    logger.debug('Sending HotReload message');
    connection?.invoke('HotReload').catch(err => {
      logger.error('SignalR Error when sending HotReload:', err);
    });
  }, [connection]);

  const handleSetAuthCookies = useCallback(
    async (message: SetAuthCookiesMessage) => {
      const currentConnectionId = latestConnectionRef.current?.connectionId;
      // logger.debug('Processing SetAuthCookies request', {
      //   hasAuthToken: !!message.cookieJarId,
      //   connectionId: currentConnectionId,
      // });
      const response = await fetch(
        `${getIvyHost()}/ivy/auth/set-auth-cookies`,
        {
          method: 'PATCH',
          headers: {
            'Content-Type': 'application/json',
            'X-Machine-Id': getMachineId(),
          },
          body: JSON.stringify({
            cookieJarId: message.cookieJarId,
            connectionId: currentConnectionId ?? null,
            triggerMachineReload: message.triggerMachineReload,
          }),
          credentials: 'include',
        }
      );
      if (!response.ok) {
        logger.error('Failed to set auth cookies', {
          status: response.status,
          statusText: response.statusText,
        });
      }

      if (message.reloadPage) {
        window.location.reload();
      }
    },
    []
  );

  const handleRedirect = useCallback((message: RedirectMessage) => {
    //logger.debug('Processing Redirect request', message);
    const { url, replaceHistory } = message;

    // Validate URL to prevent open redirect vulnerabilities
    // For redirects, only allow relative paths or same-origin URLs
    const validatedUrl = validateRedirectUrl(url, false);
    if (!validatedUrl) {
      logger.warn('Invalid redirect URL rejected', { url });
      return;
    }

    if (validatedUrl.startsWith('/')) {
      // For path-based redirects, update the pathname
      if (replaceHistory) {
        window.history.replaceState(message.state, '', validatedUrl);
      } else {
        window.history.pushState(message.state, '', validatedUrl);
      }
    } else {
      // For full URL redirects (same-origin only)
      window.location.href = validatedUrl;
    }
  }, []);

  const handleSetTheme = useCallback((theme: string) => {
    // logger.debug('Processing SetTheme request', { theme });
    const normalizedTheme = theme.toLowerCase();
    if (['dark', 'light', 'system'].includes(normalizedTheme)) {
      logger.info('Setting theme globally', { theme: normalizedTheme });
      setThemeGlobal(normalizedTheme as 'dark' | 'light' | 'system');
    } else {
      logger.error('Invalid theme value received', { theme });
    }
  }, []);

  const handleHttpRequest = useCallback(
    async (request: HttpTunnelRequestMessage) => {
      // logger.debug('Processing HttpRequest', {
      //   requestId: request.requestId,
      //   method: request.method,
      //   url: request.url,
      // });

      try {
        const headers = new Headers();
        if (request.headers) {
          Object.entries(request.headers).forEach(([key, values]) => {
            values.forEach(value => headers.append(key, value));
          });
        }
        if (request.contentType) {
          headers.set('Content-Type', request.contentType);
        }

        const fetchOptions: RequestInit = {
          method: request.method,
          headers,
          credentials: 'include',
        };

        if (request.body) {
          // Decode base64 body
          const binaryString = atob(request.body);
          const bytes = new Uint8Array(binaryString.length);
          for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
          }
          fetchOptions.body = bytes;
        }

        const response = await fetch(request.url, fetchOptions);

        // Read response body
        const responseBytes = new Uint8Array(await response.arrayBuffer());
        let responseBody = '';
        if (responseBytes.length > 0) {
          const binaryString = Array.from(responseBytes)
            .map(b => String.fromCharCode(b))
            .join('');
          responseBody = btoa(binaryString);
        }

        // Extract headers
        const responseHeaders: Record<string, string[]> = {};
        response.headers.forEach((value, key) => {
          if (!responseHeaders[key]) {
            responseHeaders[key] = [];
          }
          responseHeaders[key].push(value);
        });

        const tunnelResponse: HttpTunnelResponseMessage = {
          requestId: request.requestId,
          statusCode: response.status,
          headers: responseHeaders,
          body: responseBody || undefined,
          contentType: response.headers.get('Content-Type') || undefined,
        };

        const httpResponse = await fetch(
          `${getIvyHost()}/ivy/http-tunnel/response`,
          {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
              'X-Connection-Id': connection?.connectionId ?? '',
            },
            body: JSON.stringify(tunnelResponse),
          }
        );

        if (!httpResponse.ok) {
          logger.error('Failed to send HttpResponse via HTTP', {
            status: httpResponse.status,
            statusText: httpResponse.statusText,
          });
        } else {
          logger.debug('Sent HttpResponse back to backend via HTTP', {
            requestId: request.requestId,
          });
        }
      } catch (error) {
        logger.error('Error processing HttpRequest:', error);

        const errorResponse: HttpTunnelResponseMessage = {
          requestId: request.requestId,
          statusCode: 0,
          errorMessage:
            error instanceof Error ? error.message : 'Unknown error',
        };

        const httpResponse = await fetch(
          `${getIvyHost()}/ivy/http-tunnel/response`,
          {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
              'X-Connection-Id': connection?.connectionId ?? '',
            },
            body: JSON.stringify(errorResponse),
          }
        ).catch(err => {
          logger.error('Failed to send error HttpResponse via HTTP:', err);
          return null;
        });

        if (httpResponse) {
          if (!httpResponse.ok) {
            logger.error('Failed to send error HttpResponse via HTTP', {
              status: httpResponse.status,
              statusText: httpResponse.statusText,
            });
          } else {
            logger.debug('Sent error HttpResponse back to backend via HTTP', {
              requestId: request.requestId,
            });
          }
        }
      }
    },
    [connection]
  );

  const handleError = useCallback(
    (error: ErrorMessage) => {
      toast({
        title: error.title,
        description: error.description,
        variant: 'destructive',
        action: (
          <ToastAction
            altText="View error details"
            onClick={() => {
              showError({
                title: error.title,
                message: error.description,
                stackTrace: error.stackTrace,
              });
            }}
          >
            Details
          </ToastAction>
        ),
      });
    },
    [toast]
  );

  useEffect(() => {
    if (currentConnectionRef.current) {
      currentConnectionRef.current.stop().catch(err => {
        logger.warn('Error stopping previous SignalR connection:', err);
      });
    }

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(
        `${getIvyHost()}/ivy/messages?appId=${latestAppIdRef.current ?? ''}&appArgs=${appArgs ?? ''}&machineId=${machineId}&parentId=${parentId ?? ''}&chrome=${latestChromeRef.current}`
      )
      .withAutomaticReconnect()
      .build();

    currentConnectionRef.current = newConnection;
    queueMicrotask(() => setConnection(newConnection));

    return () => {
      if (currentConnectionRef.current === newConnection) {
        newConnection.stop().catch(err => {
          logger.warn('Error stopping SignalR connection during unmount:', err);
        });
        currentConnectionRef.current = null;
      }

      if (isRootConnection) {
        rootAppIdRef.current = undefined;
      }
    };
  }, [
    appArgs,
    stableAppId,
    machineId,
    parentId,
    stableChrome,
    isRootConnection,
  ]);

  useEffect(() => {
    if (
      connection &&
      connection.state === signalR.HubConnectionState.Disconnected
    ) {
      connection
        .start()
        .then(() => {
          logger.info('✅ WebSocket connection established for:', {
            appId: latestAppIdRef.current,
            parentId,
            connectionId: connection.connectionId,
          });

          connection.on('Refresh', message => {
            logger.debug(`[${connection.connectionId}] Refresh`, message);
            handleRefreshMessage(message);
          });

          connection.on('Update', message => {
            logger.debug(`[${connection.connectionId}] Update`, message);
            handleUpdateMessage(message);
          });

          connection.on('Toast', message => {
            logger.debug(`[${connection.connectionId}] Toast`, message);
            toast(message);
          });

          connection.on('Error', message => {
            logger.debug(`[${connection.connectionId}] Error`, message);
            handleError(message);
          });

          connection.on('SetAuthCookies', message => {
            logger.debug(`[${connection.connectionId}] SetAuthCookies`);
            handleSetAuthCookies(message);
          });

          connection.on('SetRootAppId', (message: { rootAppId: string }) => {
            logger.debug(`[${connection.connectionId}] SetRootAppId`, {
              rootAppId: message.rootAppId,
            });
            rootAppIdRef.current = message.rootAppId;
          });

          connection.on('SetTheme', theme => {
            logger.debug(`[${connection.connectionId}] SetTheme`, { theme });
            handleSetTheme(theme);
          });

          connection.on('SetTitle', (title: string) => {
            logger.debug(`[${connection.connectionId}] SetTitle`, { title });
            document.title = title;
          });

          connection.on('CopyToClipboard', (text: string) => {
            logger.debug(`[${connection.connectionId}] CopyToClipboard`);
            navigator.clipboard.writeText(text);
          });

          connection.on('OpenUrl', (url: string) => {
            logger.debug(`[${connection.connectionId}] OpenUrl`, { url });
            // Validate URL to prevent open redirect vulnerabilities
            const validatedUrl = validateLinkUrl(url);
            if (validatedUrl !== '#') {
              window.open(validatedUrl, '_blank', 'noopener,noreferrer');
            } else {
              logger.warn('Invalid OpenUrl request rejected', { url });
            }
          });

          connection.on('Redirect', message => {
            logger.debug(`[${connection.connectionId}] Redirect`, message);
            handleRedirect(message);
          });

          connection.on('ApplyTheme', (css: string) => {
            logger.debug(`[${connection.connectionId}] ApplyTheme`);

            // Remove existing custom theme style if any
            const existingStyle = document.getElementById('ivy-custom-theme');
            if (existingStyle) {
              existingStyle.remove();
            }

            // Create and inject the new style element
            const styleElement = document.createElement('style');
            styleElement.id = 'ivy-custom-theme';
            styleElement.innerHTML = css
              .replace('<style id="ivy-custom-theme">', '')
              .replace('</style>', '');
            document.head.appendChild(styleElement);
          });

          connection.on('HotReload', () => {
            logger.debug(`[${connection.connectionId}] HotReload`);
            handleHotReloadMessage();
          });

          connection.on('ReloadPage', () => {
            logger.debug(`[${connection.connectionId}] ReloadPage`);
            window.location.reload();
          });

          connection.on('HttpRequest', message => {
            logger.debug(`[${connection.connectionId}] HttpRequest`, {
              requestId: message.requestId,
              method: message.method,
              url: message.url,
            });
            handleHttpRequest(message);
          });

          connection.onreconnecting(() => {
            logger.warn(`[${connection.connectionId}] Reconnecting`);
            setDisconnected(true);
          });

          connection.onreconnected(() => {
            logger.info(`[${connection.connectionId}] Reconnected`);
            setDisconnected(false);
          });

          connection.onclose(() => {
            logger.warn(`[${connection.connectionId}] Closed`);
            setDisconnected(true);
          });
        })
        .catch(e => {
          logger.error('SignalR connection failed:', e);
        });

      return () => {
        connection.off('Refresh');
        connection.off('Update');
        connection.off('Toast');
        connection.off('Error');
        connection.off('CopyToClipboard');
        connection.off('HotReload');
        connection.off('ReloadPage');
        connection.off('HttpRequest');
        connection.off('SetAuthCookies');
        connection.off('SetRootAppId');
        connection.off('SetTheme');
        connection.off('SetTitle');
        connection.off('OpenUrl');
        connection.off('Redirect');
        connection.off('ApplyTheme');
        connection.off('reconnecting');
        connection.off('reconnected');
        connection.off('close');

        // Stop and dispose the connection when the component unmounts or connection changes
        if (connection.state !== signalR.HubConnectionState.Disconnected) {
          connection.stop().catch(err => {
            logger.warn(
              'Error stopping SignalR connection during cleanup:',
              err
            );
          });
        }
      };
    }
  }, [
    connection,
    handleRefreshMessage,
    handleUpdateMessage,
    handleHotReloadMessage,
    toast,
    handleSetAuthCookies,
    handleRedirect,
    handleSetTheme,
    handleHttpRequest,
    handleError,
    stableAppId,
    parentId,
  ]);

  const eventHandler: WidgetEventHandlerType = useCallback(
    (eventName, widgetId, args) => {
      logger.debug(`[${connectionId}] Event: ${eventName}`, { widgetId, args });
      if (!connection) {
        logger.warn('No SignalR connection available for event', {
          eventName,
          widgetId,
        });
        return;
      }
      connection.invoke('Event', eventName, widgetId, args).catch(err => {
        logger.error('SignalR Error when sending event:', err);
      });
    },
    [connection, connectionId]
  );

  return {
    connection,
    widgetTree,
    eventHandler,
    disconnected,
  };
};
