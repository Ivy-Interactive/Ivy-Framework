import React, { useCallback } from 'react';
import { Button } from '@/components/ui/button';
import Icon from '@/components/Icon';
import { cn, getIvyHost, camelCase } from '@/lib/utils';
import {
  validateLinkUrl,
  isAppProtocol,
  isAnchorLink,
  isExternalUrl,
  isMailtoUrl,
  normalizeRelativePath,
} from '@/lib/urlValidation';
import { useEventHandler } from '@/components/event-handler';
import withTooltip from '@/hoc/withTooltip';
import { Loader2 } from 'lucide-react';
import {
  BorderRadius,
  getBorderRadius,
  getColor,
  getWidth,
} from '@/lib/styles';
import { Scales } from '@/types/scale';

const ButtonWithTooltip = withTooltip(Button);

interface ButtonWidgetProps {
  id: string;
  title: string;
  icon?: string;
  iconPosition?: 'Left' | 'Right';
  scale?: Scales;
  variant?:
    | 'Primary'
    | 'Inline'
    | 'Destructive'
    | 'Outline'
    | 'Secondary'
    | 'Ghost'
    | 'SkinnyGhost'
    | 'Link'
    | 'Inline';
  disabled: boolean;
  tooltip?: string;
  foreground?: string;
  loading?: boolean;
  url?: string;
  width?: string;
  children?: React.ReactNode;
  borderRadius?: BorderRadius;
  'data-testid'?: string;
}

const getUrl = (
  url: string
): { url: string; isValid: boolean; isAnchorLink: boolean } => {
  // Validate URL to prevent dangerous protocols (javascript:, data:, etc.)
  // validateLinkUrl handles app://, anchor links, relative paths, and http/https URLs safely
  const validatedUrl = validateLinkUrl(url);

  // Check if the original URL was an anchor link (starts with #)
  const wasAnchorLink = url.trim().startsWith('#');

  // If validateLinkUrl returned '#' and the original wasn't an anchor link, it's invalid
  const isValid = validatedUrl !== '#' || wasAnchorLink;

  // Early returns for URLs that don't need host prefixing
  if (isAppProtocol(validatedUrl) || isAnchorLink(validatedUrl)) {
    return {
      url: validatedUrl,
      isValid,
      isAnchorLink: isAnchorLink(validatedUrl),
    };
  }

  if (isExternalUrl(validatedUrl) || isMailtoUrl(validatedUrl)) {
    return { url: validatedUrl, isValid, isAnchorLink: false };
  }

  // For relative paths, construct full URL with Ivy host
  // validatedUrl is already a safe relative path (starts with / or was normalized)
  const relativePath = normalizeRelativePath(validatedUrl);
  return {
    url: `${getIvyHost()}${relativePath}`,
    isValid,
    isAnchorLink: false,
  };
};

export const ButtonWidget: React.FC<ButtonWidgetProps> = ({
  id,
  title,
  icon,
  iconPosition,
  variant,
  disabled,
  tooltip,
  foreground,
  url,
  loading,
  width,
  children,
  borderRadius,
  scale = Scales.Medium,
  'data-testid': dataTestId,
}) => {
  const eventHandler = useEventHandler();

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getColor(foreground),
    ...getBorderRadius(borderRadius),
  };

  let buttonSize: 'icon' | 'default' | 'sm' | 'lg' | null | undefined =
    'default';
  let iconSize: number = 4;

  if (icon && icon != 'None' && !title) {
    buttonSize = 'icon';
  }

  if (scale == Scales.Small) {
    buttonSize = 'sm';
    iconSize = 3;
  }

  if (scale == Scales.Large) {
    buttonSize = 'lg';
    iconSize = 5;
  }

  const iconStyles = {
    width: `${iconSize * 0.25}rem`,
    height: `${iconSize * 0.25}rem`,
  };

  const effectiveUrl = url;

  const handleClick = useCallback(
    (e: React.MouseEvent) => {
      if (disabled) {
        e.preventDefault();
        return;
      }
      // Only call eventHandler for non-URL buttons
      if (!effectiveUrl) {
        eventHandler('OnClick', id, []);
      }
    },
    [id, disabled, effectiveUrl, eventHandler]
  );

  const hasChildren = !!children;
  const hasUrl = !!(effectiveUrl && !disabled);

  // Validate and sanitize URL to prevent open redirect vulnerabilities
  const urlResult = effectiveUrl && !disabled ? getUrl(effectiveUrl) : null;
  const validatedHref = urlResult?.isValid ? urlResult.url : null;
  const isInvalidUrl = urlResult && !urlResult.isValid;

  // Check if URL is a download link (starts with /ivy/download/)
  const isDownloadUrl = effectiveUrl?.startsWith('/ivy/download/') ?? false;

  // Check if URL is a mailto link (should not open in new tab)
  const isMailto = validatedHref ? isMailtoUrl(validatedHref) : false;

  // Show error message for invalid URLs (standardized error handling)
  if (isInvalidUrl) {
    return (
      <div
        key={id}
        style={styles}
        className="flex items-center justify-center bg-destructive/10 text-destructive rounded border-2 border-dashed border-destructive/25 p-4"
        role="alert"
        aria-label="Invalid button URL"
      >
        <span className="text-sm">
          {!effectiveUrl ? 'No URL provided' : 'Invalid button URL'}
        </span>
      </div>
    );
  }

  const buttonContent = (
    <>
      {!hasChildren && (
        <>
          {iconPosition == 'Left' && loading && (
            <Loader2 className="animate-spin" style={iconStyles} />
          )}
          {iconPosition == 'Left' && !loading && icon && icon != 'None' && (
            <Icon style={iconStyles} name={icon} />
          )}
          {variant === 'Link' || variant === 'Inline' ? (
            <span className="truncate">{title}</span>
          ) : (
            title
          )}
          {iconPosition == 'Right' && loading && (
            <Loader2 className="animate-spin" style={iconStyles} />
          )}
          {iconPosition == 'Right' && !loading && icon && icon != 'None' && (
            <Icon style={iconStyles} name={icon} />
          )}
        </>
      )}
      {children}
    </>
  );

  return (
    <ButtonWithTooltip
      asChild={hasUrl}
      style={styles}
      size={buttonSize}
      onClick={hasUrl ? undefined : handleClick}
      variant={
        (variant === 'Primary' ? 'default' : camelCase(variant)) as
          | 'default'
          | 'destructive'
          | 'outline'
          | 'secondary'
          | 'ghost'
          | 'skinnyGhost'
          | 'link'
          | 'inline'
      }
      disabled={disabled}
      className={cn(
        buttonSize !== 'icon' && 'w-min',
        hasChildren &&
          'p-2 h-auto items-start justify-start text-left inline-block',
        (variant === 'Link' || variant === 'Inline') &&
          'min-w-0 max-w-full overflow-hidden'
      )}
      tooltipText={
        tooltip ||
        ((variant === 'Link' || variant === 'Inline') && title
          ? title
          : undefined)
      }
      data-testid={dataTestId}
    >
      {hasUrl && validatedHref ? (
        <a
          href={validatedHref}
          {...(isDownloadUrl || isMailto
            ? {}
            : { target: '_blank', rel: 'noopener noreferrer' })}
        >
          {buttonContent}
        </a>
      ) : (
        buttonContent
      )}
    </ButtonWithTooltip>
  );
};
