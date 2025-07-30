import { FirebaseError, initializeApp } from 'firebase/app';
import {
  signInWithPopup,
  GoogleAuthProvider,
  TwitterAuthProvider,
  GithubAuthProvider,
  OAuthProvider,
  initializeAuth,
  browserPopupRedirectResolver,
  AuthProvider,
} from 'firebase/auth';
import { logger } from '@/lib/logger';

export type FirebaseConfig = {
  apiKey: string;
  authDomain: string;
  projectId: string;
};

export type AuthOptionData = {
  flow?: string;
  name?: string;
  id?: string;
  icon?: string;
  tag?: string;
};

export type AuthToken = {
  jwt: string;
  refreshToken?: string;
  expiresAt?: string;
  tag?: unknown;
};

export type AuthResult = {
  success: boolean;
  token?: AuthToken;
  errorMessage?: string;
  errorCode?: string;
};

/**
 * Hook for Firebase authentication
 *
 * @param config - Firebase configuration
 * @param authOption - Authentication option
 * @returns Promise resolving to authentication result
 */
export async function signInWithFirebase(
  config: FirebaseConfig,
  authOption?: AuthOptionData | null
): Promise<AuthResult> {
  try {
    // Initialize Firebase app
    const app = initializeApp({
      apiKey: config.apiKey,
      authDomain: config.authDomain,
      projectId: config.projectId,
    });

    const auth = initializeAuth(app, {
      popupRedirectResolver: browserPopupRedirectResolver,
    });

    // Select the appropriate auth provider based on authOption
    let provider: AuthProvider;

    // Check auth flow type (email password or OAuth)
    if (authOption?.flow === 'EmailPassword') {
      const errorMsg = 'Email/Password flow not supported in popup mode';
      logger.error(errorMsg);
      return {
        success: false,
        errorMessage: errorMsg,
      };
    }

    // Select OAuth provider based on the authOption ID
    switch (authOption?.id?.toLowerCase()) {
      case 'twitter':
        provider = new TwitterAuthProvider();
        break;
      case 'github':
        provider = new GithubAuthProvider();
        break;
      case 'microsoft':
        provider = new OAuthProvider('microsoft.com');
        break;
      case 'apple':
        provider = new OAuthProvider('apple.com');
        break;
      case 'google':
      default:
        // Default to Google if not specified or unknown
        provider = new GoogleAuthProvider();
        break;
    }

    // Add common scopes
    if (provider instanceof GoogleAuthProvider) {
      provider.addScope('email');
      provider.addScope('profile');
    } else if (provider instanceof OAuthProvider) {
      provider.addScope('email');
      if (provider.providerId === 'apple.com') {
        provider.addScope('name');
      } else {
        provider.addScope('profile');
      }
    }

    // Sign in with popup
    const result = await signInWithPopup(auth, provider);
    const user = result.user;

    // Get the ID token
    const idToken = await user.getIdToken();

    logger.info('Firebase authentication successful');

    // Compute default expiry date (one hour from now)
    let expiresAt = new Date(Date.now() + 3600_000);

    // Decode the JWT token to get the real expiry date, if possible
    const tokenParts = idToken.split('.');
    if (tokenParts.length === 3) {
      try {
        // Base64 decode JWT payload
        const payload = JSON.parse(
          atob(tokenParts[1].replace(/-/g, '+').replace(/_/g, '/'))
        );

        // Get expiry time from the 'exp' claim (seconds since epoch)
        if (payload.exp) {
          expiresAt = new Date(payload.exp * 1000);
        }
      } catch (e) {
        logger.warn('Failed to decode JWT token:', e);
      }
    }

    // Fallback to approximate expiry if decoding fails
    return {
      success: true,
      token: {
        jwt: idToken,
        refreshToken: user.refreshToken,
        expiresAt: expiresAt.toISOString(),
      },
    };
  } catch (error: unknown) {
    if (error instanceof FirebaseError) {
      logger.error('Firebase authentication error:', error);

      return {
        success: false,
        errorMessage: error.message,
        errorCode: error.code,
      };
    }

    logger.error('Unknown error during Firebase authentication:', error);

    return {
      success: false,
      errorMessage: 'An unknown error occurred',
      errorCode: undefined,
    };
  }
}
