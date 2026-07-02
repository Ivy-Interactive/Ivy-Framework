using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Ivy.Desktop;

public static class CertificateHelper
{
    private static X509Certificate2? _cachedCertificate;

    public static X509Certificate2 GetOrCreateCertificate()
    {
        if (_cachedCertificate != null)
            return _cachedCertificate;

        // 1. Try to find the ASP.NET Core developer certificate in the local user store
        var devCert = FindDeveloperCertificate();
        if (devCert != null)
        {
            _cachedCertificate = devCert;
            return devCert;
        }

        // 2. If not found, use a persistent self-signed certificate in the user's home directory (~/.ivy/certs/...)
        var certDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ivy", "certs");
        bool isBundledPath = false;

        // Also check if there's a pre-bundled certificate in the app bundle resources (macOS)
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        if (OperatingSystem.IsMacOS() && baseDir.Contains(".app/Contents/MacOS"))
        {
            var bundleCertDir = Path.GetFullPath(Path.Combine(baseDir, "..", "Resources", "certs"));
            if (File.Exists(Path.Combine(bundleCertDir, "localhost.pfx")))
            {
                certDir = bundleCertDir;
                isBundledPath = true;
            }
        }
        else
        {
            // On Windows (or other OS if not in macOS app bundle), check local certs folder under base directory
            var localCertDir = Path.Combine(baseDir, "certs");
            if (File.Exists(Path.Combine(localCertDir, "localhost.pfx")))
            {
                certDir = localCertDir;
                isBundledPath = true;
            }
        }

        if (!isBundledPath)
        {
            Directory.CreateDirectory(certDir);
        }

        var pfxPath = Path.Combine(certDir, "localhost.pfx");
        var crtPath = Path.Combine(certDir, "localhost.crt");

        X509Certificate2? loadedCert = null;

        if (File.Exists(pfxPath))
        {
            try
            {
                loadedCert = X509CertificateLoader.LoadPkcs12FromFile(pfxPath, password: null);
                // Check if expired
                if (DateTime.UtcNow > loadedCert.NotAfter.ToUniversalTime())
                {
                    loadedCert.Dispose();
                    loadedCert = null;
                }
            }
            catch
            {
                loadedCert = null;
            }
        }

        if (loadedCert == null)
        {
            // Generate new certificate
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest(
                "CN=localhost",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(false, false, 0, false));

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                    false));

            request.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, // Server Authentication
                    false));

            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddDnsName("localhost");
            sanBuilder.AddIpAddress(IPAddress.Loopback);
            sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
            request.CertificateExtensions.Add(sanBuilder.Build());

            var cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

            // Export as PFX (with private key) and CRT (public key only)
            var pfxBytes = cert.Export(X509ContentType.Pfx);
            var crtBytes = cert.Export(X509ContentType.Cert);

            File.WriteAllBytes(pfxPath, pfxBytes);
            File.WriteAllBytes(crtPath, crtBytes);

            loadedCert = X509CertificateLoader.LoadPkcs12(pfxBytes, password: null);

            // Trust the certificate if running on macOS/Windows
            if (OperatingSystem.IsMacOS())
            {
                TrustCertificateOnMac(crtPath);
            }
            else if (OperatingSystem.IsWindows())
            {
                TrustCertificateOnWindows(loadedCert);
            }
        }
        else
        {
            // If the certificate already exists, check if it's trusted.
            if (OperatingSystem.IsMacOS() && !IsCertificateTrustedOnMac(loadedCert))
            {
                TrustCertificateOnMac(crtPath);
            }
            else if (OperatingSystem.IsWindows() && !IsCertificateTrustedOnWindows(loadedCert))
            {
                TrustCertificateOnWindows(loadedCert);
            }
        }

        _cachedCertificate = loadedCert;
        return loadedCert;
    }

    public static void GenerateAndSaveCertificate(string pfxPath, string crtPath)
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=localhost",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, false));

        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                false));

        request.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, // Server Authentication
                false));

        var sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddDnsName("localhost");
        sanBuilder.AddIpAddress(IPAddress.Loopback);
        sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
        request.CertificateExtensions.Add(sanBuilder.Build());

        var cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

        // Export as PFX (with private key) and CRT (public key only)
        var pfxBytes = cert.Export(X509ContentType.Pfx);
        var crtBytes = cert.Export(X509ContentType.Cert);

        var pfxDir = Path.GetDirectoryName(pfxPath);
        if (!string.IsNullOrEmpty(pfxDir)) Directory.CreateDirectory(pfxDir);

        var crtDir = Path.GetDirectoryName(crtPath);
        if (!string.IsNullOrEmpty(crtDir)) Directory.CreateDirectory(crtDir);

        File.WriteAllBytes(pfxPath, pfxBytes);
        File.WriteAllBytes(crtPath, crtBytes);
    }

    private static X509Certificate2? FindDeveloperCertificate()
    {
        try
        {
            using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            foreach (var cert in store.Certificates)
            {
                if (cert.Subject.Contains("CN=localhost", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var extension in cert.Extensions)
                    {
                        if (extension.Oid?.Value == "1.3.6.1.4.1.311.84.1.1") // ASP.NET Core HTTPS developer certificate
                        {
                            if (DateTime.UtcNow >= cert.NotBefore.ToUniversalTime() && DateTime.UtcNow <= cert.NotAfter.ToUniversalTime())
                            {
                                return cert;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Failed to load ASP.NET Core developer certificate from store: {ex.Message}");
        }
        return null;
    }

    private static bool IsCertificateTrustedOnMac(X509Certificate2 cert)
    {
        try
        {
            using var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var results = store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, validOnly: false);
            if (results.Count > 0) return true;

            using var storeMy = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            storeMy.Open(OpenFlags.ReadOnly);
            var resultsMy = storeMy.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, validOnly: false);
            return resultsMy.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private static void TrustCertificateOnMac(string crtPath)
    {
        try
        {
            Console.WriteLine($"[INFO] Trusting self-signed certificate on macOS: {crtPath}");
            var psi = new ProcessStartInfo
            {
                FileName = "security",
                Arguments = $"add-trusted-cert -d -r trustRoot -k \"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/Library/Keychains/login.keychain-db\" \"{crtPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            process?.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Failed to add certificate to macOS keychain: {ex.Message}");
        }
    }

    private static bool IsCertificateTrustedOnWindows(X509Certificate2 cert)
    {
        try
        {
            using var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var results = store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, validOnly: false);
            return results.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private static void TrustCertificateOnWindows(X509Certificate2 cert)
    {
        try
        {
            Console.WriteLine($"[INFO] Trusting self-signed certificate on Windows");
            using var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Failed to trust certificate on Windows: {ex.Message}");
        }
    }
}
