/**
 * Client-side validation for TextInput variants. Mirrors backend Ivy.Validation.Validators
 * so the input shows invalid state on blur (email @ and ., tel 7–15 digits, url http/https, password min length).
 */

export type TextInputVariantForValidation =
  | 'email'
  | 'tel'
  | 'url'
  | 'password';

const EMAIL_INVALID = 'Please enter a valid email address';
const TEL_INVALID = 'Please enter a valid phone number';
const URL_INVALID = 'Please enter a valid URL (http or https)';
const PASSWORD_INVALID = 'Password must be at least 8 characters';

function isEmpty(s: string | undefined | null): boolean {
  return s == null || String(s).trim() === '';
}

export function validateTextInputVariant(
  variant: TextInputVariantForValidation,
  value: string | undefined | null,
  passwordMinLength: number = 8
): string | null {
  if (isEmpty(value)) return null;

  const s = String(value).trim();

  switch (variant) {
    case 'email': {
      if (!s.includes('@')) return EMAIL_INVALID;
      const at = s.indexOf('@');
      const host = s.slice(at + 1);
      if (!host.includes('.')) return EMAIL_INVALID;
      return null;
    }
    case 'tel': {
      const digitsOnly = s.replace(/\D/g, '');
      if (digitsOnly.length < 7 || digitsOnly.length > 15) return TEL_INVALID;
      if (!/^[\d\s+\-().]+$/.test(s)) return TEL_INVALID;
      return null;
    }
    case 'url': {
      try {
        const url = new URL(s);
        if (url.protocol !== 'http:' && url.protocol !== 'https:')
          return URL_INVALID;
        return null;
      } catch {
        return URL_INVALID;
      }
    }
    case 'password': {
      if (s.length < passwordMinLength) return PASSWORD_INVALID;
      return null;
    }
    default:
      return null;
  }
}

export function hasVariantValidation(
  variant: string
): variant is TextInputVariantForValidation {
  return ['email', 'tel', 'url', 'password'].includes(variant.toLowerCase());
}
