import { getHeight, getWidth } from "@/lib/styles";
import React, { useCallback, useEffect, useRef } from "react";
import { useEventHandler } from "@/components/event-handler";

const EMPTY_EVENTS: string[] = [];

interface IframeWidgetProps {
  id: string;
  src: string;
  width?: string;
  height?: string;
  refreshToken?: number;
  events?: string[];
  outboundMessageType?: string;
  outboundMessageToken?: string;
}

export const IframeWidget: React.FC<IframeWidgetProps> = ({
  id,
  src = "",
  width = "Full",
  height = "Full",
  refreshToken,
  events = EMPTY_EVENTS,
  outboundMessageType,
  outboundMessageToken,
}) => {
  const iframeKey = `${id}-${refreshToken}`;
  const iframeRef = useRef<HTMLIFrameElement>(null);
  const eventHandler = useEventHandler();

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
    maxWidth: "100%",
  };

  const handleMessage = useCallback(
    (event: MessageEvent) => {
      if (!events.includes("OnMessageReceived")) return;
      if (iframeRef.current?.contentWindow !== event.source) return;

      const { type, payload } = event.data ?? {};
      if (typeof type === "string") {
        eventHandler("OnMessageReceived", id, [type, payload]);
      }
    },
    [id, events, eventHandler],
  );

  useEffect(() => {
    window.addEventListener("message", handleMessage);
    return () => window.removeEventListener("message", handleMessage);
  }, [handleMessage]);

  const iframeLoadedRef = useRef(false);
  const pendingMessageRef = useRef<{ type: string; token: string } | null>(null);

  const sendOutboundMessage = useCallback(() => {
    if (!pendingMessageRef.current) return;
    iframeRef.current?.contentWindow?.postMessage(
      {
        type: pendingMessageRef.current.type,
        token: pendingMessageRef.current.token,
      },
      "*",
    );
  }, []);

  useEffect(() => {
    if (!outboundMessageType || outboundMessageToken == null) return;
    pendingMessageRef.current = {
      type: outboundMessageType,
      token: outboundMessageToken,
    };
    if (iframeLoadedRef.current) {
      sendOutboundMessage();
    }
  }, [outboundMessageType, outboundMessageToken, sendOutboundMessage]);

  // Reset loaded state when iframe key changes
  useEffect(() => {
    iframeLoadedRef.current = false;
  }, [iframeKey]);

  const handleIframeLoad = useCallback(() => {
    iframeLoadedRef.current = true;
    sendOutboundMessage();
  }, [sendOutboundMessage]);

  return (
    <iframe
      ref={iframeRef}
      src={src}
      key={iframeKey}
      style={styles}
      title={id}
      onLoad={handleIframeLoad}
    />
  );
};
