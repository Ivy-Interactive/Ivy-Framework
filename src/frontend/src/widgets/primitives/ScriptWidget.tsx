import { useEffect, useRef } from 'react';

interface ScriptWidgetProps {
  id: string;
  src?: string;
  inlineCode?: string;
  async?: boolean;
  defer?: boolean;
  crossOrigin?: string;
  integrity?: string;
  referrerPolicy?: string;
}

const ScriptWidget: React.FC<ScriptWidgetProps> = ({
  id,
  src,
  inlineCode,
  async: isAsync,
  defer: isDefer,
  crossOrigin,
  integrity,
  referrerPolicy,
}) => {
  const scriptRef = useRef<HTMLScriptElement | null>(null);

  useEffect(() => {
    if (scriptRef.current) {
      scriptRef.current.remove();
      scriptRef.current = null;
    }

    const script = document.createElement('script');

    if (src) {
      script.src = src;
    }
    if (inlineCode) {
      script.textContent = inlineCode;
    }
    if (isAsync) script.async = true;
    if (isDefer) script.defer = true;
    if (crossOrigin) script.crossOrigin = crossOrigin;
    if (integrity) script.integrity = integrity;
    if (referrerPolicy) script.setAttribute('referrerpolicy', referrerPolicy);

    script.dataset.ivyWidgetId = id;
    document.head.appendChild(script);
    scriptRef.current = script;

    return () => {
      if (scriptRef.current) {
        scriptRef.current.remove();
        scriptRef.current = null;
      }
    };
  }, [
    id,
    src,
    inlineCode,
    isAsync,
    isDefer,
    crossOrigin,
    integrity,
    referrerPolicy,
  ]);

  return null;
};

export default ScriptWidget;
