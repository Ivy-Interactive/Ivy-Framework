import React, { useMemo, useRef, useState, useEffect } from "react";
import { cn } from "@/lib/utils";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectSeparator,
  SelectTrigger,
  SelectTriggerEndActions,
} from "@/components/ui/select";
import { Tooltip, TooltipProvider, TooltipTrigger, TooltipContent } from "@/components/ui/tooltip";
import { Input } from "@/components/ui/input";
import { Search, Loader2, X } from "lucide-react";
import Icon from "@/components/Icon";
import { InvalidIcon } from "@/components/InvalidIcon";
import {
  textInputAffixPrefixCellClasses,
  textInputAffixSuffixCellClasses,
  textInputEmbeddedContentPaddingClasses,
  textInputSuffixGlyphSlotClasses,
  textInputTrailingIconButtonClasses,
  textInputTrailingIconSizeVariant,
  textInputTrailingInvalidSlotClasses,
  xIconVariant,
} from "@/components/ui/input/text-input-variant";
import { getWidth, inputStyles } from "@/lib/styles";
import { SelectInputWidgetProps } from "./select-types";
import { useSelectValueHandler } from "./select-utils";
import { EMPTY_ARRAY } from "@/lib/constants";

export const SelectSingleVariant: React.FC<SelectInputWidgetProps> = ({
  id,
  placeholder = "",
  value,
  disabled = false,
  invalid,
  options = EMPTY_ARRAY,
  eventHandler,
  nullable = false,
  searchable,
  searchMode = "CaseInsensitive",
  emptyMessage,
  loading = false,
  ghost = false,
  density,
  "data-testid": dataTestId,
  width,
  events = EMPTY_ARRAY,
  autoFocus,
  slots,
}) => {
  const triggerRef = useRef<HTMLButtonElement>(null);
  const [isOpen, setIsOpen] = useState(false);

  const [prevDisabled, setPrevDisabled] = useState(disabled);
  const [prevAutoFocus, setPrevAutoFocus] = useState(autoFocus);
  const hasAutoFocusedRef = useRef(false);

  if (disabled !== prevDisabled || autoFocus !== prevAutoFocus) {
    setPrevDisabled(disabled);
    setPrevAutoFocus(autoFocus);
    if (autoFocus && !disabled && !hasAutoFocusedRef.current) {
      setIsOpen(true);
    }
  }

  useEffect(() => {
    if (autoFocus && !disabled && !hasAutoFocusedRef.current) {
      hasAutoFocusedRef.current = true;
      triggerRef.current?.focus();
    }
  }, [autoFocus, disabled]);
  const validOptions = options.filter(
    (option) => option.value != null && option.value.toString().trim() !== "",
  );

  const handleValueChange = useSelectValueHandler(
    id,
    value,
    validOptions,
    eventHandler,
    false,
    nullable,
    events,
  );

  const stringValue =
    value != null && value.toString().trim() !== "" ? value.toString() : undefined;

  const selectedOption = useMemo(() => {
    if (!stringValue) return undefined;
    return validOptions.find((opt) => opt.value.toString() === stringValue);
  }, [stringValue, validOptions]);

  const selectedLabel = selectedOption?.label;
  const searchInputRef = useRef<HTMLInputElement>(null);
  /** True after the user edits the panel search field; reset when the dropdown closes. */
  const userFilteringRef = useRef(false);
  const [isEllipsed, setIsEllipsed] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");

  useEffect(() => {
    if (!selectedLabel) return;

    const checkEllipsis = () => {
      const firstSpan = triggerRef.current?.querySelector("span:first-child") as HTMLSpanElement;
      if (firstSpan) {
        setIsEllipsed(firstSpan.scrollWidth > firstSpan.clientWidth);
      }
    };

    requestAnimationFrame(checkEllipsis);
    const handleResize = () => setTimeout(checkEllipsis, 150);
    window.addEventListener("resize", handleResize);
    return () => window.removeEventListener("resize", handleResize);
  }, [selectedLabel]);

  const SEARCH_THRESHOLD = 7;
  // null/undefined = auto (show search when options >= threshold)
  // true  = always show search
  // false = never show search
  const isSearchEnabled =
    searchable === true || (searchable !== false && validOptions.length >= SEARCH_THRESHOLD);

  const filteredOptions = useMemo(() => {
    if (!isSearchEnabled || !searchTerm) return validOptions;
    return validOptions.filter((option) => {
      const term = searchMode === "CaseInsensitive" ? searchTerm.toLowerCase() : searchTerm;
      const label = (option.label || "").toLowerCase();
      if (searchMode === "Fuzzy") {
        let i = 0,
          j = 0;
        while (i < term.length && j < label.length) {
          if (term[i] === label[j]) i++;
          j++;
        }
        return i === term.length;
      }
      return label.includes(term);
    });
  }, [validOptions, isSearchEnabled, searchTerm, searchMode]);

  // Radix Select runs focusSelectedItem in a child useEffect: it focuses the selected item, or the
  // listbox when there are no items — which steals focus from the header search field whenever the
  // filtered item set changes (0 matches, 1 match, after clearing text, etc.). Parent useEffect runs
  // after the child's; a macrotask catches deferred focus work inside Radix.
  useEffect(() => {
    if (!isOpen || !isSearchEnabled || !userFilteringRef.current) return;
    const input = searchInputRef.current;
    if (!input) return;

    const restore = () => {
      const active = document.activeElement;
      if (active === input || active === triggerRef.current) return;
      input.focus({ preventScroll: true });
    };

    restore();
    const t = window.setTimeout(restore, 0);
    return () => clearTimeout(t);
  }, [filteredOptions.length, isOpen, isSearchEnabled, searchTerm]);

  useEffect(() => {
    if (!isOpen || !isSearchEnabled) return;
    requestAnimationFrame(() => {
      const viewport = document.querySelector<HTMLElement>("[data-radix-select-viewport]");
      if (viewport) {
        viewport.scrollTop = 0;
        viewport.dispatchEvent(new Event("scroll"));
      }
    });
  }, [searchTerm, isOpen, isSearchEnabled]);

  const groupedOptions = filteredOptions.reduce<Record<string, typeof validOptions>>(
    (acc, option) => {
      const key = option.group || "default";
      if (!acc[key]) acc[key] = [];
      acc[key].push(option);
      return acc;
    },
    {},
  );

  const hasValue = stringValue !== undefined;
  const styles = getWidth(width);

  const prefixContent = slots?.Prefix;
  const suffixContent = slots?.Suffix;
  const hasPrefix = (prefixContent?.length ?? 0) > 0;
  const hasSuffix = (suffixContent?.length ?? 0) > 0;
  const hasAffixes = hasPrefix || hasSuffix;
  const trailingBesideSuffix = hasSuffix;
  const showClear = nullable && hasValue && !disabled;
  const showTrailing = showClear || Boolean(invalid);

  const handleClear = (e: React.SyntheticEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (events.includes("OnChange")) eventHandler("OnChange", id, [null]);
  };

  const handleOpenChange = (newOpen: boolean) => {
    setIsOpen(newOpen);
    if (!newOpen) {
      userFilteringRef.current = false;
    }
    if (newOpen) {
      if (events.includes("OnFocus")) eventHandler("OnFocus", id, []);
    } else {
      if (events.includes("OnBlur")) eventHandler("OnBlur", id, []);
    }
  };

  const handleTriggerFocus = () => {
    if (!isOpen && events.includes("OnFocus")) eventHandler("OnFocus", id, []);
  };

  const handleTriggerBlur = () => {
    if (!isOpen && events.includes("OnBlur")) eventHandler("OnBlur", id, []);
  };

  const selectTriggerElement = (
    <SelectTrigger
      ref={triggerRef}
      className={cn(
        "relative",
        invalid &&
          (hasAffixes
            ? inputStyles.invalidInput
            : "text-destructive-foreground placeholder-destructive-foreground"),
        !hasValue && "text-muted-foreground",
        ghost &&
          "border-transparent shadow-none bg-transparent hover:bg-accent hover:text-accent-foreground dark:border-transparent dark:bg-transparent dark:hover:bg-accent dark:hover:text-accent-foreground",
        "border-0 bg-transparent shadow-none focus:ring-0 focus-visible:ring-0 focus-visible:ring-offset-0 dark:bg-transparent",
        hasAffixes && textInputEmbeddedContentPaddingClasses(density),
        hasPrefix && "rounded-l-none",
        hasSuffix && "rounded-r-none",
      )}
      density={density}
      onBlur={handleTriggerBlur}
      onFocus={handleTriggerFocus}
    >
      <span
        className={cn(
          "flex-1 truncate text-left pointer-events-none",
          !hasValue && "text-muted-foreground",
        )}
      >
        {hasValue ? (selectedLabel ?? stringValue) : placeholder}
      </span>
      <SelectTriggerEndActions>
        {loading && (
          <div className="pointer-events-auto flex h-6 items-center">
            <Loader2 className="size-4 shrink-0 animate-spin text-muted-foreground text-opacity-50" />
          </div>
        )}
        {!trailingBesideSuffix && showClear && (
          <div
            role="button"
            tabIndex={-1}
            aria-label="Clear"
            onClick={handleClear}
            onPointerDown={(e) => e.stopPropagation()}
            onKeyDown={(e) => {
              if (e.key === "Enter" || e.key === " ") {
                handleClear(e);
              }
            }}
            className="pointer-events-auto flex h-6 cursor-pointer items-center rounded p-1 hover:bg-accent focus:outline-none"
          >
            <X className={xIconVariant({ density })} />
          </div>
        )}
        {!trailingBesideSuffix && invalid && (
          <div
            className="pointer-events-auto flex h-6 cursor-default items-center"
            onPointerDown={(e) => e.stopPropagation()}
          >
            <InvalidIcon message={invalid} />
          </div>
        )}
      </SelectTriggerEndActions>
    </SelectTrigger>
  );

  const selectTriggerBranch =
    isEllipsed && selectedLabel ? (
      <TooltipProvider>
        <Tooltip delayDuration={300} open={isOpen ? false : undefined}>
          <TooltipTrigger asChild>{selectTriggerElement}</TooltipTrigger>
          <TooltipContent className="bg-popover text-popover-foreground shadow-md max-w-sm">
            <div className="whitespace-pre-wrap break-words">{selectedLabel}</div>
          </TooltipContent>
        </Tooltip>
      </TooltipProvider>
    ) : (
      selectTriggerElement
    );

  const selectContent = (
    <Select
      key={id}
      disabled={disabled}
      value={stringValue}
      onValueChange={handleValueChange}
      open={isOpen}
      onOpenChange={handleOpenChange}
      data-testid={dataTestId}
    >
      {hasAffixes ? (
        selectTriggerBranch
      ) : (
        <div
          className={cn(
            "relative flex w-full min-w-0 select-none items-stretch rounded-field border bg-transparent shadow-sm transition-colors dark:bg-white/5",
            isOpen
              ? "border-ring outline-none dark:border-ring"
              : "border-input outline-none dark:border-white/10 focus-within:border-ring dark:focus-within:border-ring",
            invalid && "border-destructive",
            disabled && "cursor-not-allowed opacity-50",
            ghost &&
              "border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent",
          )}
        >
          <div className="relative min-w-0 flex-1">{selectTriggerBranch}</div>
        </div>
      )}
      <SelectContent
        density={density}
        header={
          isSearchEnabled ? (
            <div className="p-2 border-b">
              <div className="relative">
                <Search className="absolute left-2.5 top-2.5 size-4 text-muted-foreground" />
                <Input
                  ref={searchInputRef}
                  type="text"
                  placeholder="Search..."
                  value={searchTerm}
                  onChange={(e) => {
                    userFilteringRef.current = true;
                    setSearchTerm(e.target.value);
                  }}
                  onKeyDown={(e) => e.stopPropagation()}
                  onClick={(e) => e.stopPropagation()}
                  className="pl-9 h-9"
                  disabled={disabled || loading}
                />
              </div>
            </div>
          ) : undefined
        }
      >
        {loading ? (
          <div className="flex justify-center p-4">
            <Loader2 className="size-5 animate-spin text-muted-foreground" />
          </div>
        ) : filteredOptions.length === 0 ? (
          <div className="p-4 text-center text-sm text-muted-foreground">
            {emptyMessage || "No options available"}
          </div>
        ) : (
          Object.entries(groupedOptions).map(([group, options], index) => (
            <React.Fragment key={group}>
              {index > 0 && <SelectSeparator />}
              <SelectGroup>
                {group !== "default" && <SelectLabel>{group}</SelectLabel>}
                {options.map((option) => (
                  <SelectItem
                    key={option.value}
                    value={option.value.toString()}
                    textValue={option.label}
                    density={density}
                    disabled={disabled || loading || option.disabled}
                  >
                    {option.tooltip ? (
                      <TooltipProvider>
                        <Tooltip delayDuration={300}>
                          <TooltipTrigger asChild>
                            <div className="flex items-center gap-2">
                              {option.icon && (
                                <Icon name={option.icon} className="size-4 flex-shrink-0" />
                              )}
                              {option.label}
                            </div>
                          </TooltipTrigger>
                          <TooltipContent>{option.tooltip}</TooltipContent>
                        </Tooltip>
                      </TooltipProvider>
                    ) : (
                      <div className="flex items-center gap-2">
                        {option.icon && (
                          <Icon name={option.icon} className="size-4 flex-shrink-0" />
                        )}
                        {option.label}
                      </div>
                    )}
                  </SelectItem>
                ))}
              </SelectGroup>
            </React.Fragment>
          ))
        )}
      </SelectContent>
    </Select>
  );

  return (
    <div className="flex items-center gap-2 w-full" style={styles}>
      {hasAffixes ? (
        <div
          className={cn(
            "relative flex flex-1 items-stretch rounded-field border bg-transparent shadow-sm transition-colors dark:bg-white/5",
            isOpen
              ? "border-ring outline-none dark:border-ring"
              : "border-input dark:border-white/10",
            invalid && "border-destructive",
            disabled && "cursor-not-allowed opacity-50",
            ghost &&
              "border-transparent shadow-none bg-transparent dark:border-transparent dark:bg-transparent",
          )}
        >
          {hasPrefix && (
            <div className={textInputAffixPrefixCellClasses(density, prefixContent)}>
              {prefixContent}
            </div>
          )}
          <div className="relative w-full min-w-0 flex-1">{selectContent}</div>
          {hasSuffix && (
            <div
              className={textInputAffixSuffixCellClasses(density, suffixContent, {
                showTrailing: trailingBesideSuffix && showTrailing,
              })}
            >
              {trailingBesideSuffix && showTrailing && (
                <>
                  {showClear && (
                    <button
                      type="button"
                      tabIndex={-1}
                      aria-label="Clear"
                      onClick={handleClear}
                      onPointerDown={(e) => e.stopPropagation()}
                      className={textInputTrailingIconButtonClasses(false, density)}
                    >
                      <X className={textInputTrailingIconSizeVariant({ density })} />
                    </button>
                  )}
                  {invalid && (
                    <InvalidIcon
                      message={invalid}
                      className={textInputTrailingInvalidSlotClasses(false, density)}
                      iconClassName={textInputTrailingIconSizeVariant({ density })}
                    />
                  )}
                </>
              )}
              {trailingBesideSuffix && showTrailing ? (
                <span className={textInputSuffixGlyphSlotClasses(density)}>{suffixContent}</span>
              ) : (
                suffixContent
              )}
            </div>
          )}
        </div>
      ) : (
        <div className="flex-1 relative w-full">{selectContent}</div>
      )}
    </div>
  );
};
