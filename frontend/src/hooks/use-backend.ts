import { useState, useEffect, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import { WidgetEventHandlerType, WidgetNode } from '@/types/widgets';
import { useToast } from '@/hooks/use-toast';
import {
  getAppArgs,
  getAppId,
  getIvyHost,
  getMachineId,
  getParentId,
} from '@/lib/utils';
import { logger } from '@/lib/logger';
import { applyPatch, Operation } from 'fast-json-patch';
import { setThemeGlobal } from '@/components/ThemeProvider';

type UpdateMessage = Array<{
  viewId: string;
  indices: number[];
  patch: Operation[];
}>;

type RefreshMessage = {
  widgets: WidgetNode;
  removeIvyBranding: boolean;
  lazyLoadWholeTree?: boolean;
};

type AuthToken = {
  jwt: string;
  refreshToken?: string;
  expiresAt?: string;
};

const widgetTreeToXml = (node: WidgetNode) => {
  const tagName = node.type.replace('Ivy.', '');
  const attributes: string[] = [`Id="${escapeXml(node.id)}"`];
  if (node.props) {
    for (const [key, value] of Object.entries(node.props)) {
      // Convert key to PascalCase
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

export const useBackend = () => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(
    null
  );
  const [widgetTree, setWidgetTree] = useState<WidgetNode | null>(null);
  const [disconnected, setDisconnected] = useState(false);
  const [removeIvyBranding, setRemoveIvyBranding] = useState(false);
  const [lazyLoadWholeTree, setLazyLoadWholeTree] = useState<boolean>(false);
  const { toast } = useToast();
  const appId = getAppId();
  const appArgs = getAppArgs();
  const parentId = getParentId();
  const machineId = getMachineId();

  useEffect(() => {
    if (widgetTree) {
      logger.debug('Converting widget tree to XML', {
        widgetTreeId: widgetTree.id,
      });
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
        logger.debug('Widget tree successfully converted to XML');
      } catch (error) {
        logger.error('Error converting widget tree to XML', { error });
        return;
      }
      logger.debug(xml);
    }
  }, [widgetTree]);

  const handleRefreshMessage = useCallback((message: RefreshMessage) => {
    logger.debug('Processing Refresh message', { message });
    setRemoveIvyBranding(message.removeIvyBranding);
    setWidgetTree(message.widgets);
    setLazyLoadWholeTree(!!message.lazyLoadWholeTree);
  }, []);

  const handleUpdateMessage = useCallback((message: UpdateMessage) => {
    logger.debug('Processing Update message', { message });
    setWidgetTree(currentTree => {
      if (!currentTree) {
        logger.warn('No current widget tree available for update');
        return null;
      }
      logger.debug('Applying update to widget tree', {
        updateCount: message.length,
        currentTreeId: currentTree.id,
      });
      const newWidgetTree = { ...currentTree };
      message.forEach((update, index) => {
        logger.debug(`Processing update ${index + 1}/${message.length}`, {
          viewId: update.viewId,
          indices: update.indices,
          patchOperations: update.patch.length,
        });
        let parent = newWidgetTree;
        if (update.indices.length === 0) {
          logger.debug('Applying patch to root widget tree');
          applyPatch(newWidgetTree, update.patch);
        } else {
          update.indices.forEach((index, i) => {
            if (i === update.indices.length - 1) {
              logger.debug('Applying patch to child widget', {
                childIndex: index,
                parentId: parent.id,
              });
              applyPatch(parent.children![index], update.patch);
            } else {
              parent = parent.children![index];
            }
          });
        }
      });
      logger.debug('Widget tree update completed');
      return newWidgetTree;
    });
  }, []);

  const handleHotReloadMessage = useCallback(() => {
    logger.debug('Sending HotReload message');
    connection?.invoke('HotReload').catch(err => {
      logger.error('SignalR Error when sending HotReload:', err);
    });
  }, [connection]);

  const handleSetJwt = useCallback(async (jwt: AuthToken | null) => {
    logger.debug('Processing SetJwt request', { hasJwt: !!jwt });
    const response = await fetch(`${getIvyHost()}/auth/set-jwt`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(jwt),
      credentials: 'include',
    });
    if (response.ok) {
      logger.info('JWT set successfully, reloading page');
      window.location.reload();
    } else {
      logger.error('Failed to set JWT', {
        status: response.status,
        statusText: response.statusText,
      });
    }
  }, []);

  const handleSetTheme = useCallback((theme: string) => {
    logger.debug('Processing SetTheme request', { theme });
    const normalizedTheme = theme.toLowerCase();
    if (['dark', 'light', 'system'].includes(normalizedTheme)) {
      logger.info('Setting theme globally', { theme: normalizedTheme });
      setThemeGlobal(normalizedTheme as 'dark' | 'light' | 'system');
    } else {
      logger.error('Invalid theme value received', { theme });
    }
  }, []);

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(
        `${getIvyHost()}/messages?appId=${appId ?? ''}&appArgs=${appArgs ?? ''}&machineId=${machineId}&parentId=${parentId ?? ''}`
      )
      .withAutomaticReconnect()
      .build();
    setConnection(newConnection);
  }, [appArgs, appId, machineId, parentId]);

  useEffect(() => {
    if (connection) {
      connection
        .start()
        .then(() => {
          logger.info('SignalR connection established');

          connection.on('Refresh', message => {
            logger.debug('Received Refresh message', message);
            handleRefreshMessage(message);
          });

          connection.on('Update', message => {
            logger.debug('Received Update message', message);
            handleUpdateMessage(message);
          });

          connection.on('Toast', message => {
            logger.debug('Received Toast message', message);
            toast(message);
          });

          connection.on('SetJwt', jwt => {
            logger.debug('Received SetJwt message');
            handleSetJwt(jwt);
          });

          connection.on('SetTheme', theme => {
            logger.debug('Received SetTheme message', { theme });
            handleSetTheme(theme);
          });

          connection.on('CopyToClipboard', (text: string) => {
            logger.debug('Received CopyToClipboard message');
            navigator.clipboard.writeText(text);
          });

          connection.on('OpenUrl', (url: string) => {
            logger.debug('Received OpenUrl message', { url });
            window.open(url, '_blank');
          });

          connection.on('HotReload', () => {
            logger.debug('Received HotReload message');
            handleHotReloadMessage();
          });
          connection.onreconnecting(() => {
            logger.warn('SignalR connection reconnecting');
            setDisconnected(true);
          });
          connection.onreconnected(() => {
            logger.info('SignalR connection reconnected');
            setDisconnected(false);
          });
          connection.onclose(() => {
            logger.warn('SignalR connection closed');
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
  ]);

  const eventHandler: WidgetEventHandlerType = useCallback(
    (eventName, widgetId, args) => {
      logger.debug(`Processing event: ${eventName}`, { widgetId, args });
      if (!connection) {
        logger.warn('No SignalR connection available for event', {
          eventName,
          widgetId,
        });
        return;
      }
      logger.debug(`Invoking SignalR event: ${eventName}`, { widgetId, args });
      connection.invoke('Event', eventName, widgetId, args).catch(err => {
        logger.error('SignalR Error when sending event:', err);
      });
    },
    [connection]
  );

  return {
    connection,
    widgetTree,
    eventHandler,
    disconnected,
    removeIvyBranding,
    lazyLoadWholeTree,
  };
};
