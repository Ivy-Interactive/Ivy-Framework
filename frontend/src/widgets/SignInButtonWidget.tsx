import React from 'react';
import { SignInButton } from '@clerk/clerk-react';

interface SignInButtonWidgetProps {
  id: string;
  mode?: 'modal' | 'redirect';
  fallbackRedirectUrl?: string;
  forceRedirectUrl?: string | null;
  signUpForceRedirectUrl?: string | null;
  signUpFallbackRedirectUrl?: string;
  initialValues?: Record<string, unknown>;
  children?: React.ReactNode;
  'data-testid'?: string;
}

export const SignInButtonWidget: React.FC<SignInButtonWidgetProps> = ({
  mode = 'modal',
  fallbackRedirectUrl,
  forceRedirectUrl,
  signUpForceRedirectUrl,
  signUpFallbackRedirectUrl,
  initialValues,
  children,
}) => {
  return (
    <SignInButton
      mode={mode}
      fallbackRedirectUrl={fallbackRedirectUrl}
      forceRedirectUrl={forceRedirectUrl}
      signUpForceRedirectUrl={signUpForceRedirectUrl}
      signUpFallbackRedirectUrl={signUpFallbackRedirectUrl}
      initialValues={initialValues}
    >
      {children}
    </SignInButton>
  );
};
