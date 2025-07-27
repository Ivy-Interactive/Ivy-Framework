import { jwtVerify, JWTPayload } from 'jose';

const publicKeyPem = '%IVY_LICENSE_PUBLIC_KEY%';

function pemToUint8Array(pem: string) {
  const b64 = pem.replace(/-----.*?-----/g, '').replace(/\s+/g, '');
  return Uint8Array.from(atob(b64), c => c.charCodeAt(0));
}

function getIvyLicense(): string | null {
  return (
    document
      .querySelector('meta[name="ivy-license"]')
      ?.getAttribute('content') ?? null
  );
}

let licensePayload: JWTPayload | null = null;
let keyPromise: Promise<CryptoKey> | null = null;

async function getPublicKey(): Promise<CryptoKey> {
  if (!keyPromise) {
    keyPromise = crypto.subtle.importKey(
      'spki',
      pemToUint8Array(publicKeyPem),
      { name: 'ECDSA', namedCurve: 'P-256' },
      true,
      ['verify']
    );
  }
  return keyPromise;
}

export async function hasLicensedFeature(hasFeature: string): Promise<boolean> {
  try {
    if (publicKeyPem.includes('IVY_LICENSE_PUBLIC_KEY')) return false;
    if (!licensePayload) {
      const token = getIvyLicense();
      if (!token) return false;
      const key = await getPublicKey();
      const { payload } = await jwtVerify(token, key);
      licensePayload = payload;
    }

    const features = licensePayload.features;
    if (typeof features === 'string') {
      return features
        .split(',')
        .map(s => s.trim())
        .includes(hasFeature);
    }
    if (Array.isArray(features)) {
      return features.includes(hasFeature);
    }

    return false;
  } catch (error) {
    console.error('License validation failed', error);
    return false;
  }
}
