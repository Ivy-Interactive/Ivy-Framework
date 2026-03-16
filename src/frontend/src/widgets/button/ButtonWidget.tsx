import React, { useCallback } from 'react';
import { m, LazyMotion, domAnimation } from 'framer-motion';
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
} from '@/lib/url';
import { useEventHandler } from '@/components/event-handler';
import withTooltip from '@/hoc/withTooltip';
import { Loader2 } from 'lucide-react';
import { BorderRadius, getColor, getWidth } from '@/lib/styles';
import { Densities } from '@/types/density';

const ButtonWithTooltip = withTooltip(Button);

interface ButtonWidgetProps {
  id: string;
  title: string;
  icon?: string;
  iconPosition?: 'Left' | 'Right';
  density?: Densities;
  variant?:
    | 'Primary'
    | 'Inline'
    | 'Destructive'
    | 'Outline'
    | 'Secondary'
    | 'Ghost'
    | 'Link'
    | 'Inline'
    | 'Ai';
  disabled: boolean;
  tooltip?: string;
  foreground?: string;
  loading?: boolean;
  url?: string;
  target?: 'Blank' | 'Self';
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
  iconPosition = 'Left',
  variant = 'Primary',
  disabled = false,
  tooltip,
  foreground,
  url,
  target = 'Self',
  loading = false,
  width,
  children,
  borderRadius = 'Rounded',
  density = Densities.Medium,
  'data-testid': dataTestId,
}) => {
  const eventHandler = useEventHandler();

  // For 'Rounded' (default), rely on the 'rounded-field' class from buttonVariant.
  // Only add inline style to override the class for 'None'/'Full'.
  const borderRadiusStyle: React.CSSProperties =
    borderRadius === 'Full'
      ? { borderRadius: '9999px' }
      : borderRadius === 'None'
        ? { borderRadius: '0' }
        : {}; // 'Rounded' uses the rounded-field class

  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getColor(foreground),
    ...borderRadiusStyle,
  };

  let buttonSize:
    | 'icon'
    | 'icon-sm'
    | 'default'
    | 'sm'
    | 'lg'
    | null
    | undefined = 'default';
  let iconSize: number = 4;
  const isIconOnly = icon && icon != 'None' && !title;

  if (isIconOnly) {
    buttonSize = 'icon';
  }

  if (density == Densities.Small) {
    buttonSize = isIconOnly ? 'icon-sm' : 'sm';
    iconSize = 3;
  }

  if (density == Densities.Large) {
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

  if (variant === 'Ai') {
    // Determine border radius classes based on borderRadius prop
    const getBorderRadiusClass = () => {
      if (borderRadius === 'Full') {
        return {
          container: 'rounded-full',
          button: 'rounded-full',
          buttonStyle: { borderRadius: '9999px' } as React.CSSProperties,
        };
      }
      if (borderRadius === 'Rounded') {
        return { container: 'rounded-lg', button: 'rounded-md' };
      }
      return { container: 'rounded-none', button: 'rounded-md' };
    };

    const borderRadiusClasses = getBorderRadiusClass();

    // Determine container height based on button size
    const getContainerHeight = () => {
      if (buttonSize === 'icon') return { container: 'h-10', button: 'h-9' };
      if (buttonSize === 'sm') return { container: 'h-9', button: 'h-[35px]' };
      if (buttonSize === 'lg') return { container: 'h-11', button: 'h-10' };
      return { container: 'h-10', button: 'h-9' };
    };

    return (
      <div
        className={cn(
          'relative p-[2px] w-fit overflow-hidden',
          getContainerHeight().container,
          borderRadiusClasses.container,
          disabled && 'opacity-50'
        )}
      >
        {/* Rotating RGB gradient border - scaled up to cover entire button */}
        <LazyMotion features={domAnimation}>
          <m.div
            className="absolute inset-[-50%] aspect-square"
            style={{
              background:
                'conic-gradient(from 0deg, #ff0000, #ff8000, #ffff00, #80ff00, #00ff00, #00ff80, #00ffff, #0080ff, #0000ff, #8000ff, #ff00ff, #ff0080, #ff0000)',
              filter: 'blur(10px)',
            }}
            animate={{ rotate: 360 }}
            transition={{
              duration: 3,
              repeat: Infinity,
              ease: 'linear',
            }}
          />
        </LazyMotion>
        <ButtonWithTooltip
          asChild={hasUrl}
          size={buttonSize}
          onClick={hasUrl ? undefined : handleClick}
          variant={'ai'}
          disabled={disabled}
          className={cn(
            'relative z-10 flex items-center gap-1',
            getContainerHeight().button,
            borderRadiusClasses.button,
            buttonSize !== 'icon' && 'w-min',
            hasChildren &&
              'p-2 h-auto items-start justify-start text-left inline-block'
          )}
          style={borderRadiusClasses.buttonStyle}
          tooltipText={tooltip || undefined}
          data-testid={dataTestId}
        >
          {hasUrl && validatedHref ? (
            <a
              href={validatedHref}
              {...(isDownloadUrl || isMailto
                ? {}
                : {
                    target: target === 'Self' ? '_self' : '_blank',
                    rel: target === 'Self' ? undefined : 'noopener noreferrer',
                  })}
            >
              {buttonContent}
            </a>
          ) : (
            buttonContent
          )}
        </ButtonWithTooltip>
      </div>
    );
  }

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
          | 'link'
          | 'inline'
      }
      disabled={disabled}
      className={cn(
        ['icon', 'icon-sm'].includes(buttonSize) ? '' : 'w-min',
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
            : {
                target: target === 'Self' ? '_self' : '_blank',
                rel: target === 'Self' ? undefined : 'noopener noreferrer',
              })}
        >
          {buttonContent}
        </a>
      ) : (
        buttonContent
      )}
    </ButtonWithTooltip>
  );
};
