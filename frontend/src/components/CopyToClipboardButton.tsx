import { Copy, Check } from "lucide-react";
import { useState } from "react";

const CopyToClipboardButton = ({ textToCopy = "", label = "" }) => {
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(textToCopy);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      //ignore
    }
  };

  return (
    <button
      onClick={handleCopy}
      className={`
        flex 
        items-center 
        gap-1 
        px-3 
        py-2 
        rounded-lg
        transition-all duration-200 ease-in-out
        ${
          copied
            ? "bg-green-100 text-green-700"
            : "bg-transparent text-gray-700"
        }
      `}
    >
      <span className="relative w-4 h-4">
        <span
          className={`
            absolute inset-0 transform transition-transform duration-200
            ${copied ? "scale-0" : "scale-100"}
          `}
        >
          <Copy size={16} />
        </span>
        <span
          className={`
            absolute inset-0 transform transition-transform duration-200
            ${copied ? "scale-100" : "scale-0"}
          `}
        >
          <Check size={16} />
        </span>
      </span>
      {label && <span className="text-xs">{copied ? "Copied!" : label}</span>}
    </button>
  );
};

export default CopyToClipboardButton;
