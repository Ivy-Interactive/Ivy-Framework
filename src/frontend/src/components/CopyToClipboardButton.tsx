import { useState } from "react";
import { Copy, Check } from "lucide-react";
import { cn } from "@/lib/utils";
import { Densities } from "@/types/density";
import { cva } from "class-variance-authority";

const copyIconVariant = cva("", {
  variants: {
    density: {
      Small: "h-3 w-3",
      Medium: "h-4 w-4",
      Large: "h-5 w-5",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});

const copyButtonSizeVariant = cva(
  "p-2 rounded hover:bg-accent focus:outline-none cursor-pointer flex items-center",
  {
    variants: {
      density: {
        Small: "h-6",
        Medium: "h-8",
        Large: "h-9",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

interface CopyToClipboardButtonProps {
  textToCopy?: string;
  label?: string;
  "aria-label"?: string;
  density?: Densities;
  className?: string;
}

const CopyToClipboardButton: React.FC<CopyToClipboardButtonProps> = ({
  textToCopy = "",
  label = "",
  "aria-label": ariaLabel,
  density = Densities.Medium,
  className,
}) => {
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(textToCopy);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (err: unknown) {
      console.error(err);
    }
  };

  const isIconOnly = !label;

  return (
    <button
      onClick={handleCopy}
      aria-label={ariaLabel || "Copy to clipboard"}
      className={cn(
        isIconOnly
          ? cn(copyButtonSizeVariant({ density }), copied && "bg-primary text-primary-foreground")
          : "flex items-center gap-1 px-3 py-2 rounded-lg transition-all duration-200 ease-in-out cursor-pointer hover:bg-accent hover:shadow-sm border-0",
        !isIconOnly &&
          (copied
            ? "bg-primary text-primary-foreground"
            : "bg-transparent text-muted-foreground hover:text-foreground"),
        isIconOnly && !copied && "bg-background hover:bg-accent",
        copied &&
          isIconOnly &&
          "hover:bg-primary hover:text-primary-foreground focus-visible:ring-primary",
        className,
      )}
    >
      <span className={cn("relative", copyIconVariant({ density }))}>
        <span
          className={cn(
            "absolute inset-0 transform transition-transform duration-200",
            copied ? "scale-0" : "scale-100",
          )}
        >
          <Copy className={copyIconVariant({ density })} />
        </span>
        <span
          className={cn(
            "absolute inset-0 transform transition-transform duration-200",
            copied ? "scale-100" : "scale-0",
          )}
        >
          <Check className={copyIconVariant({ density })} />
        </span>
      </span>
      {label && <span className="text-small-label">{copied ? "Copied!" : label}</span>}
    </button>
  );
};

export default CopyToClipboardButton;
