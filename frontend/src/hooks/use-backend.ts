import { setThemeGlobal } from "@/components/ThemeProvider";
import { useToast } from "@/hooks/use-toast";
import {
  getAppArgs,
  getAppId,
  getIvyHost,
  getMachineId,
  getParentId,
} from "@/lib/utils";
import { WidgetEventHandlerType, WidgetNode } from "@/types/widgets";
import * as signalR from "@microsoft/signalr";
import { applyPatch, Operation } from "fast-json-patch";
import { useState, useEffect, useCallback } from "react";

type UpdateMessage = Array<{
  viewId: string;
  indices: number[];
  patch: Operation[];
}>;

type RefreshMessage = {
  widgets: WidgetNode;
  removeIvyBranding: boolean;
};

type AuthToken = {
  jwt: string;
  refreshToken?: string;
  expiresAt?: string;
};

const widgetTreeToXml = (node: WidgetNode) => {
  const tagName = node.type.replace("Ivy.", "");
  const attributes: string[] = [`Id="${escapeXml(node.id)}"`];
  if (node.props) {
    for (const [key, value] of Object.entries(node.props)) {
      // Convert key to PascalCase
      const pascalCaseKey = key.charAt(0).toUpperCase() + key.slice(1);
      attributes.push(`${pascalCaseKey}="${escapeXml(String(value))}"`);
    }
  }
  let childrenXml = "";
  if (node.children && node.children.length > 0) {
    childrenXml = node.children.map((child) => widgetTreeToXml(child)).join("");
    return `<${tagName} ${attributes.join(" ")}>${childrenXml}</${tagName}>`;
  } else {
    return `<${tagName} ${attributes.join(" ")} />`;
  }
};

const escapeXml = (str: string) => {
  return str
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&apos;");
};

export const useBackend = () => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(
    null
  );
  const [widgetTree, setWidgetTree] = useState<WidgetNode | null>(null);
  const [disconnected, setDisconnected] = useState(false);
  const [removeIvyBranding, setRemoveIvyBranding] = useState(false);
  const { toast } = useToast();
  const appId = getAppId();
  const appArgs = getAppArgs();
  const parentId = getParentId();
  const machineId = getMachineId();

  useEffect(() => {
    if (widgetTree) {
      const parser = new DOMParser();
      let xml;
      try {
        const xmlString = widgetTreeToXml(widgetTree);
        if (!xmlString) {
          return;
        }
        xml = parser.parseFromString(xmlString, "application/xml");
        const parserError = xml.querySelector("parsererror");
        if (parserError) {
          return;
        }
      } catch (error) {
        //ignore
        return;
      }
      console.log(xml);
    }
  }, [widgetTree]);

  const handleRefreshMessage = useCallback((message: RefreshMessage) => {
    console.log("Refresh", message);
    setRemoveIvyBranding(message.removeIvyBranding);
    setWidgetTree(message.widgets);
  }, []);

  const handleUpdateMessage = useCallback((message: UpdateMessage) => {
    console.log("Update", message);
    setWidgetTree((currentTree) => {
      if (!currentTree) return null;
      const newWidgetTree = { ...currentTree };
      message.forEach((update) => {
        let parent = newWidgetTree;
        if (update.indices.length === 0) {
          applyPatch(newWidgetTree, update.patch);
        } else {
          update.indices.forEach((index, i) => {
            if (i === update.indices.length - 1) {
              applyPatch(parent.children![index], update.patch);
            } else {
              parent = parent.children![index];
            }
          });
        }
      });
      return newWidgetTree;
    });
  }, []);

  const handleHotReloadMessage = useCallback(() => {
    console.log("HotReload");
    connection
      ?.invoke("HotReload")
      .catch((err) => console.error("SignalR Error:", err));
  }, [connection]);

  const handleSetJwt = useCallback(async (jwt: AuthToken | null) => {
    const response = await fetch(`${getIvyHost()}/auth/set-jwt`, {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(jwt),
      credentials: "include",
    });
    if (response.ok) {
      window.location.reload();
    }
  }, []);

  const handleSetTheme = useCallback((theme: string) => {
    const normalizedTheme = theme.toLowerCase();
    if (["dark", "light", "system"].includes(normalizedTheme)) {
      setThemeGlobal(normalizedTheme as "dark" | "light" | "system");
    } else {
      console.error(`Invalid theme value received: ${theme}`);
    }
  }, []);

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(
        `${getIvyHost()}/messages?appId=${appId ?? ""}&appArgs=${
          appArgs ?? ""
        }&machineId=${machineId}&parentId=${parentId ?? ""}`
      )
      .withAutomaticReconnect()
      .build();
    setConnection(newConnection);
  }, [appId]);

  useEffect(() => {
    if (connection) {
      connection
        .start()
        .then(() => {
          console.log("Connected!");
          connection.on("Refresh", handleRefreshMessage);
          connection.on("Update", handleUpdateMessage);
          connection.on("Toast", (message) => {
            console.log("Toast", message);
            toast(message);
          });
          connection.on("$SetChatPanelUrl", (chatPanelUrl: string | null) => {
            window.parent.postMessage(
              { type: "$SetChatPanelUrl", url: chatPanelUrl },
              "*"
            );
          });
          connection.on("SetJwt", handleSetJwt);
          connection.on("SetTheme", handleSetTheme);
          connection.on("CopyToClipboard", (text: string) => {
            navigator.clipboard.writeText(text);
          });
          connection.on("OpenUrl", (url: string) => {
            window.open(url, "_blank");
          });
          connection.on("HotReload", handleHotReloadMessage);
          connection.onreconnecting(() => {
            setDisconnected(true);
          });
          connection.onreconnected(() => {
            setDisconnected(false);
          });
          connection.onclose(() => {
            setDisconnected(true);
          });
        })
        .catch((e) => console.log("Connection failed: ", e));

      return () => {
        connection.off("Refresh");
        connection.off("Update");
        connection.off("Toast");
        connection.off("CopyToClipboard");
        connection.off("HotReload");
        connection.off("reconnecting");
        connection.off("reconnected");
        connection.off("close");
        connection.off("$SetChatPanelUrl");
        connection.off("SetJwt");
        connection.off("SetTheme");
        connection.off("OpenUrl");
      };
    }
  }, [
    connection,
    handleRefreshMessage,
    handleUpdateMessage,
    handleHotReloadMessage,
    toast,
  ]);

  const eventHandler: WidgetEventHandlerType = useCallback(
    (eventName, widgetId, args) => {
      console.log("Event", eventName, widgetId, args);
      connection
        ?.invoke("Event", eventName, widgetId, args)
        .catch((err) => console.error("SignalR Error:", err));
    },
    [connection]
  );

  return {
    connection,
    widgetTree,
    eventHandler,
    disconnected,
    removeIvyBranding,
  };
};
