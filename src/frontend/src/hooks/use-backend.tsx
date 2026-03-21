import { useState, useEffect, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { WidgetEventHandlerType, WidgetNode } from '@/types/widgets';
import { useToast } from '@/hooks/use-toast';
import { showError } from '@/hooks/use-error-sheet';
import { getIvyHost, getMachineId } from '@/lib/utils';
import { validateRedirectUrl, validateLinkUrl } from '@/lib/url';
import { logger } from '@/lib/logger';
import { applyPatch, Operation } from 'fast-json-patch';
import { ToastAction } from '@/components/ui/toast';
import { setThemeGlobal } from '@/components/theme-provider';
import {
  setExternalWidgetRegistry,
  ExternalWidgetInfo,
} from '@/widgets/externalWidgetLoader';

type UpdateMessage = Array<{
  iteration: number;
  viewId: string;
  indices: number[];
  patch: Operation[];
  treeHash?: string;
}>;

type RefreshMessage = {
  widgets: WidgetNode;
  externalWidgets?: ExternalWidgetInfo[] | null;
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
  triggerMachineBrokeredRefresh: boolean;
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

type StreamDataMessage = {
  streamId: string;
  data: unknown;
};

type StreamHandler = (data: unknown) => void;
type StreamSubscriber = (streamId: string, onData: StreamHandler) => () => void;

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

/**
 * Shallow clones a WidgetNode, preserving children array reference unless specified.
 */
function shallowCloneNode(node: WidgetNode, cloneChildren = false): WidgetNode {
  const cloned = { ...node };
  if (cloneChildren && node.children) {
    cloned.children = [...node.children];
  }
  return cloned;
}

/**
 * Deep clones a WidgetNode and all its descendants.
 * Uses structuredClone for proper deep cloning of props (which may have nested objects/arrays).
 * Used only when applying patches that may modify nested structures.
 */
function deepCloneNode(node: WidgetNode): WidgetNode {
  // Use structuredClone for a true deep clone - handles nested props correctly
  // and is faster than JSON.parse(JSON.stringify()) for complex objects
  return structuredClone(node);
}

/**
 * Creates a new tree with structural sharing, cloning only the path to the target node.
 * All unchanged subtrees maintain their reference identity for React reconciliation optimization.
 *
 * @param tree - The original tree
 * @param indices - Path to the target node (array of child indices)
 * @returns Object containing the new tree root and the parent of the target node, or null on error
 */
function clonePathToTarget(
  tree: WidgetNode,
  indices: number[]
): { newTree: WidgetNode; parent: WidgetNode; targetIndex: number } | null {
  if (indices.length === 0) {
    // No path - return a shallow clone of root with cloned children array
    const newTree = shallowCloneNode(tree, true);
    return { newTree, parent: newTree, targetIndex: -1 };
  }

  // Shallow clone root with its children array
  const newTree = shallowCloneNode(tree, true);
  let current = newTree;

  // Walk down the path, shallow cloning each node along the way
  for (let i = 0; i < indices.length - 1; i++) {
    const index = indices[i];

    if (!current.children) {
      logger.error('No children found while cloning path', {
        nodeId: current.id,
        nodeType: current.type,
        pathIndex: i,
        indices,
      });
      return null;
    }

    if (index >= current.children.length) {
      logger.error('Index out of bounds while cloning path', {
        index,
        childrenLength: current.children.length,
        pathIndex: i,
        indices,
      });
      return null;
    }

    const child = current.children[index];
    if (!child) {
      logger.error('Child at index is null/undefined', {
        index,
        childrenLength: current.children.length,
        parentType: current.type,
        parentId: current.id,
      });
      return null;
    }

    // Shallow clone this child with its children array
    current.children[index] = shallowCloneNode(child, true);
    current = current.children[index];
  }

  // Validate final parent has children
  const targetIndex = indices[indices.length - 1];
  if (!current.children) {
    logger.error('No children found at final parent', {
      nodeId: current.id,
      nodeType: current.type,
      targetIndex,
    });
    return null;
  }

  if (targetIndex >= current.children.length) {
    logger.error('Target index out of bounds', {
      targetIndex,
      childrenLength: current.children.length,
      parentId: current.id,
    });
    return null;
  }

  return { newTree, parent: current, targetIndex };
}

/**
 * Pure function that applies updates to the widget tree using structural sharing.
 * Only the path to changed nodes is cloned - unchanged subtrees keep their references.
 * This allows React to skip re-rendering unchanged components via reference equality checks.
 *
 * Safe for React Strict Mode double-invocation.
 */
function applyUpdateMessage(
  tree: WidgetNode,
  updates: UpdateMessage
): WidgetNode | null {
  let newTree = tree;

  for (const update of updates) {
    const firstPatch = update.patch[0];
    const isFullReplacement =
      update.patch.length === 1 &&
      firstPatch.op === 'replace' &&
      firstPatch.path === '';

    if (isFullReplacement) {
      // Full node replacement - just swap in the new value
      const newValue = (firstPatch as { value: unknown }).value as WidgetNode;

      if (update.indices.length === 0) {
        // Replacing entire tree
        newTree = newValue;
      } else {
        // Clone path to target and replace
        const result = clonePathToTarget(newTree, update.indices);
        if (!result) continue;
        newTree = result.newTree;
        result.parent.children![result.targetIndex] = newValue;
      }
    } else {
      // Patch operation - need to clone target before mutating
      if (update.indices.length === 0) {
        // Patching root - deep clone since patch may affect nested props
        newTree = deepCloneNode(newTree);
        applyPatch(newTree, update.patch);
      } else {
        // Clone path to target
        const result = clonePathToTarget(newTree, update.indices);
        if (!result) continue;
        newTree = result.newTree;

        // Deep clone the target node before applying patch (patch mutates in place)
        const targetClone = deepCloneNode(
          result.parent.children![result.targetIndex]
        );
        result.parent.children![result.targetIndex] = targetClone;
        applyPatch(targetClone, update.patch);
      }
    }

    if (update.treeHash) {
      // Verify tree integrity in DEBUG mode
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

async function refreshAuthFromCookies(
  connectionId: string | null
): Promise<void> {
  try {
    const response = await fetch(`${getIvyHost()}/ivy/auth/refresh-session`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Connection-Id': connectionId ?? '',
        'X-Machine-Id': getMachineId(),
      },
      credentials: 'include',
    });
    if (!response.ok) {
      logger.error('Failed to refresh auth from cookies, reloading page', {
        status: response.status,
        statusText: response.statusText,
      });
      window.location.reload();
    }
  } catch (error) {
    logger.error('Error refreshing auth from cookies, reloading page', error);
    window.location.reload();
  }
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
  const isStoppingRef = useRef(false);
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

  // Stream registry for server-to-client streaming
  const streamRegistryRef = useRef<Map<string, StreamHandler>>(new Map());
  const streamBufferRef = useRef<Map<string, unknown[]>>(new Map());

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

    // Update external widget registry if provided
    if (message.externalWidgets) {
      setExternalWidgetRegistry(message.externalWidgets);
    }

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
            triggerMachineBrokeredRefresh:
              message.triggerMachineBrokeredRefresh,
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

    // Check if this is an OAuth login redirect
    const pageParams = new URLSearchParams(window.location.search);
    const oauthLogin = pageParams.get('oauthLogin');

    // Build SignalR connection URL
    let signalRUrl = `${getIvyHost()}/ivy/messages?appId=${latestAppIdRef.current ?? ''}&appArgs=${appArgs ?? ''}&machineId=${machineId}&parentId=${parentId ?? ''}&chrome=${latestChromeRef.current}`;
    if (oauthLogin) {
      signalRUrl += `&oauthLogin=${oauthLogin}`;
      // Clean up the URL by removing the oauthLogin parameter
      pageParams.delete('oauthLogin');
      const newUrl = pageParams.toString()
        ? `${window.location.pathname}?${pageParams.toString()}`
        : window.location.pathname;
      window.history.replaceState({}, '', newUrl);
    }

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(signalRUrl)
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
      isStoppingRef.current = false;
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

          connection.on(
            'Toast',
            (message: {
              title?: string;
              description?: string;
              variant?: string | number;
            }) => {
              logger.debug(`[${connection.connectionId}] Toast`, message);

              const { variant, ...rest } = message;
              let variantStr = 'default';

              if (typeof variant === 'string') {
                variantStr = variant.toLowerCase();
              } else if (typeof variant === 'number') {
                const variantMap = [
                  'default',
                  'destructive',
                  'success',
                  'warning',
                  'info',
                ];
                variantStr = variantMap[variant] || 'default';
              }

              toast({
                ...rest,
                variant: variantStr as
                  | 'default'
                  | 'destructive'
                  | 'success'
                  | 'warning'
                  | 'info',
              });
            }
          );

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

          connection.on('RefreshAuthFromCookies', () => {
            logger.debug(`[${connection.connectionId}] RefreshAuthFromCookies`);
            refreshAuthFromCookies(connection.connectionId);
          });

          connection.on('HttpRequest', message => {
            logger.debug(`[${connection.connectionId}] HttpRequest`, {
              requestId: message.requestId,
              method: message.method,
              url: message.url,
            });
            handleHttpRequest(message);
          });

          connection.on('StreamData', (message: StreamDataMessage) => {
            const handler = streamRegistryRef.current.get(message.streamId);
            if (handler) {
              handler(message.data);
            } else {
              // Buffer data until handler registers (mirrors backend WriteStream buffering)
              let buffer = streamBufferRef.current.get(message.streamId);
              if (!buffer) {
                buffer = [];
                streamBufferRef.current.set(message.streamId, buffer);
              }
              buffer.push(message.data);
            }
          });

          connection.onreconnecting(() => {
            if (isStoppingRef.current) return;
            logger.warn(`[${connection.connectionId}] Reconnecting`);
            setDisconnected(true);
          });

          connection.onreconnected(() => {
            logger.info(`[${connection.connectionId}] Reconnected`);
            setDisconnected(false);
          });

          connection.onclose(() => {
            if (isStoppingRef.current) return;
            logger.warn(`[${connection.connectionId}] Closed`);
            setDisconnected(true);
          });
        })
        .catch(e => {
          logger.error('SignalR connection failed:', e);
        });

      return () => {
        isStoppingRef.current = true;

        connection.off('Refresh');
        connection.off('Update');
        connection.off('Toast');
        connection.off('Error');
        connection.off('CopyToClipboard');
        connection.off('HotReload');
        connection.off('ReloadPage');
        connection.off('HttpRequest');
        connection.off('StreamData');
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
      logger.info('[Event] Sending:', { eventName, widgetId, args });
      logger.debug(`[${connectionId}] Event: ${eventName}`, { widgetId, args });
      if (!connection) {
        logger.warn('[Event] No SignalR connection available for event:', {
          eventName,
          widgetId,
        });
        return;
      }
      connection
        .invoke('Event', eventName, widgetId, args)
        .then(() => {
          logger.debug('[Event] Invoke succeeded:', { eventName, widgetId });
        })
        .catch(err => {
          logger.error('[Event] Invoke failed:', {
            eventName,
            widgetId,
            error: err,
          });
        });
    },
    [connection, connectionId]
  );

  const subscribeToStream: StreamSubscriber = useCallback(
    (streamId: string, onData: StreamHandler) => {
      streamRegistryRef.current.set(streamId, onData);

      // Flush any data that arrived before the handler was registered
      const buffered = streamBufferRef.current.get(streamId);
      if (buffered) {
        streamBufferRef.current.delete(streamId);
        for (const data of buffered) {
          onData(data);
        }
      }

      // Notify backend that we're subscribed so it can flush any buffered data
      latestConnectionRef.current
        ?.invoke('StreamSubscribe', streamId)
        .catch(err => {
          logger.error('Failed to notify stream subscription:', err);
        });
      return () => {
        streamRegistryRef.current.delete(streamId);
      };
    },
    []
  );

  return {
    connection,
    widgetTree,
    eventHandler,
    subscribeToStream,
    disconnected,
  };
};
