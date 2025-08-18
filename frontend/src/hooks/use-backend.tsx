import { useState, useEffect, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { WidgetEventHandlerType, WidgetNode } from '@/types/widgets';
import { useToast } from '@/hooks/use-toast';
import { showError } from '@/hooks/use-error-sheet';
import { getIvyHost, getMachineId } from '@/lib/utils';
import { logger } from '@/lib/logger';
import { applyPatch, Operation } from 'fast-json-patch';
import { cloneDeep } from 'lodash';
import { ToastAction } from '@/components/ui/toast';
import { setThemeGlobal } from '@/components/theme-provider';

type UpdateMessage = Array<{
  viewId: string;
  indices: number[];
  patch: Operation[];
}>;

type RefreshMessage = {
  widgets: WidgetNode;
};

type ErrorMessage = {
  title: string;
  description: string;
  stackTrace?: string;
};

type AuthToken = {
  jwt: string;
  refreshToken?: string;
  expiresAt?: string;
  tag?: unknown;
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

const escapeXml = (str: string) => {
  return str
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&apos;');
};

function applyUpdateMessage(
  tree: WidgetNode,
  message: UpdateMessage
): WidgetNode {
  const newTree = cloneDeep(tree);

  message.forEach(update => {
    let parent = newTree;

    if (!parent) {
      logger.error('No parent found in applyUpdateMessage', { message });
      return;
    }

    if (update.indices.length === 0) {
      applyPatch(parent, update.patch);
    } else {
      update.indices.forEach((index, i) => {
        if (i === update.indices.length - 1) {
          if (!parent.children) {
            logger.error('No children found in parent', { parent });
            return;
          }
          applyPatch(parent.children[index], update.patch);
        } else {
          if (!parent) {
            logger.error('No parent found in applyUpdateMessage', { message });
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
  });

  return newTree;
}

export const useBackend = (
  appId: string | null,
  appArgs: string | null,
  parentId: string | null
) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(
    null
  );
  const [widgetTree, setWidgetTree] = useState<WidgetNode | null>(null);
  const [disconnected, setDisconnected] = useState(false);
  const { toast } = useToast();
  const machineId = getMachineId();
  const connectionId = connection?.connectionId;
  const connectionRef = useRef<signalR.HubConnection | null>(null);

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
    setWidgetTree(message.widgets);
  }, []);

  const handleUpdateMessage = useCallback((message: UpdateMessage) => {
    setWidgetTree(currentTree => {
      if (!currentTree) {
        logger.warn('No current widget tree available for update');
        return null;
      }
      return applyUpdateMessage(currentTree, message);
    });
  }, []);

  const handleHotReloadMessage = useCallback(() => {
    logger.debug('Sending HotReload message');
    connection?.invoke('HotReload').catch(err => {
      logger.error('SignalR Error when sending HotReload:', err);
    });
  }, [connection]);

  const handleSetJwt = useCallback(
    (token: AuthToken) => {
      const storageKey = `ivy-token-${appId}`;
      if (token.jwt) {
        localStorage.setItem(storageKey, JSON.stringify(token));
        logger.debug('JWT token stored');
      } else {
        localStorage.removeItem(storageKey);
        logger.debug('JWT token removed');
      }
    },
    [appId]
  );

  const handleSetTheme = useCallback((theme: string) => {
    const normalizedTheme = theme?.toLowerCase();
    if (['dark', 'light', 'system'].includes(normalizedTheme)) {
      logger.info('Setting theme globally', { theme: normalizedTheme });
      setThemeGlobal(normalizedTheme as 'dark' | 'light' | 'system');
    } else {
      logger.error('Invalid theme value received', { theme });
    }
  }, []);

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
    let cancelled = false;

    const setupConnection = async () => {
      // Stop previous connection if it exists and wait for it to finish
      if (connectionRef.current) {
        try {
          await connectionRef.current.stop();
          logger.debug('Previous connection stopped successfully');
        } catch (err) {
          logger.warn('Error stopping previous SignalR connection:', err);
        }
      }

      // Check if effect was cancelled during cleanup
      if (cancelled) return;

      const newConnection = new signalR.HubConnectionBuilder()
        .withUrl(
          `${getIvyHost()}/messages?appId=${appId ?? ''}&appArgs=${appArgs ?? ''}&machineId=${machineId}&parentId=${parentId ?? ''}`
        )
        .withAutomaticReconnect()
        .build();

      connectionRef.current = newConnection;

      // Check again if effect was cancelled
      if (cancelled) {
        newConnection.stop().catch(() => {});
        return;
      }

      setConnection(newConnection);
    };

    setupConnection();

    // Cleanup function for component unmount
    return () => {
      cancelled = true;
      if (connectionRef.current) {
        connectionRef.current.stop().catch(err => {
          logger.warn('Error stopping SignalR connection during cleanup:', err);
        });
        connectionRef.current = null;
      }
    };
  }, [appArgs, appId, machineId, parentId]);

  useEffect(() => {
    if (
      connection &&
      connection.state === signalR.HubConnectionState.Disconnected
    ) {
      connection
        .start()
        .then(() => {
          logger.info('✅ WebSocket connection established for:', {
            appId,
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

          connection.on('SetJwt', jwt => {
            logger.debug(`[${connection.connectionId}] SetJwt`);
            handleSetJwt(jwt);
          });

          connection.on('SetTheme', theme => {
            logger.debug(`[${connection.connectionId}] SetTheme`, { theme });
            handleSetTheme(theme);
          });

          connection.on('CopyToClipboard', (text: string) => {
            logger.debug(`[${connection.connectionId}] CopyToClipboard`);
            navigator.clipboard.writeText(text);
          });

          connection.on('OpenUrl', (url: string) => {
            logger.debug(`[${connection.connectionId}] OpenUrl`, { url });
            window.open(url, '_blank');
          });

          connection.on('HotReload', () => {
            logger.debug(`[${connection.connectionId}] HotReload`);
            handleHotReloadMessage();
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
        connection.off('SetJwt');
        connection.off('SetTheme');
        connection.off('OpenUrl');
        connection.off('reconnecting');
        connection.off('reconnected');
        connection.off('close');
      };
    }
  }, [
    connection,
    handleRefreshMessage,
    handleUpdateMessage,
    handleHotReloadMessage,
    toast,
    handleSetJwt,
    handleSetTheme,
    handleError,
    appId,
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
