using System.Diagnostics;
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
        var userProfileDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(userProfileDir))
            userProfileDir = AppDomain.CurrentDomain.BaseDirectory;
        var certDir = Path.Join(userProfileDir, ".ivy", "certs");
        bool isBundledPath = false;

        // Also check if there's a pre-bundled certificate in the app bundle resources (macOS)
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        if (OperatingSystem.IsMacOS() && baseDir.Contains(".app/Contents/MacOS"))
        {
            var bundleCertDir = Path.GetFullPath(Path.Join(baseDir, "..", "Resources", "certs"));
            if (File.Exists(Path.Join(bundleCertDir, "localhost.pfx")))
            {
                certDir = bundleCertDir;
                isBundledPath = true;
            }
        }
        else
        {
            // On Windows (or other OS if not in macOS app bundle), check local certs folder under base directory
            var localCertDir = Path.Join(baseDir, "certs");
            if (File.Exists(Path.Join(localCertDir, "localhost.pfx")))
            {
                certDir = localCertDir;
                isBundledPath = true;
            }
        }

        if (!isBundledPath)
        {
            Directory.CreateDirectory(certDir);
        }

        var pfxPath = Path.Join(certDir, "localhost.pfx");
        var crtPath = Path.Join(certDir, "localhost.crt");

        X509Certificate2? loadedCert = null;

        if (File.Exists(pfxPath))
        {
            try
            {
                loadedCert = X509CertificateLoader.LoadPkcs12FromFile(
                    pfxPath,
                    password: null,
                    keyStorageFlags: X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);

                // Check if expired
                if (DateTime.UtcNow > loadedCert.NotAfter.ToUniversalTime())
                {
                    loadedCert.Dispose();
                    loadedCert = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Failed to load existing certificate from {pfxPath}: {ex.Message}");
                loadedCert = null;
            }
        }

        if (loadedCert == null)
        {
            var (pfxBytes, crtBytes) = GenerateSelfSignedCertificateBytes();

            File.WriteAllBytes(pfxPath, pfxBytes);
            File.WriteAllBytes(crtPath, crtBytes);

            loadedCert = X509CertificateLoader.LoadPkcs12(
                pfxBytes,
                password: null,
                keyStorageFlags: X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);

            // Trust the certificate if running on macOS/Windows
            if (OperatingSystem.IsMacOS() && !isBundledPath)
            {
                TrustCertificateOnMac(crtPath, loadedCert);
            }
            else if (OperatingSystem.IsWindows() && !isBundledPath)
            {
                TrustCertificateOnWindows(loadedCert);
            }
        }
        else
        {
            // If the certificate already exists, check if it's trusted.
            if (OperatingSystem.IsMacOS() && !isBundledPath && !IsCertificateTrustedOnMac(crtPath, loadedCert))
            {
                TrustCertificateOnMac(crtPath, loadedCert);
            }
            else if (OperatingSystem.IsWindows() && !isBundledPath && !IsCertificateTrustedOnWindows(loadedCert))
            {
                TrustCertificateOnWindows(loadedCert);
            }
        }

        _cachedCertificate = loadedCert;
        return loadedCert;
    }

    public static void GenerateAndSaveCertificate(string pfxPath, string crtPath)
    {
        var (pfxBytes, crtBytes) = GenerateSelfSignedCertificateBytes();

        var pfxDir = Path.GetDirectoryName(pfxPath);
        if (!string.IsNullOrEmpty(pfxDir)) Directory.CreateDirectory(pfxDir);

        var crtDir = Path.GetDirectoryName(crtPath);
        if (!string.IsNullOrEmpty(crtDir)) Directory.CreateDirectory(crtDir);

        File.WriteAllBytes(pfxPath, pfxBytes);
        File.WriteAllBytes(crtPath, crtBytes);
    }

    private static (byte[] PfxBytes, byte[] CrtBytes) GenerateSelfSignedCertificateBytes()
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

        return (pfxBytes, crtBytes);
    }

    private static X509Certificate2? FindDeveloperCertificate()
    {
        try
        {
            using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            return store.Certificates
                .Cast<X509Certificate2>()
                .Where(cert => cert.Subject.Contains("CN=localhost", StringComparison.OrdinalIgnoreCase))
                .Where(cert => cert.Extensions.Cast<X509Extension>()
                    .Any(extension => extension.Oid?.Value == "1.3.6.1.4.1.311.84.1.1")) // ASP.NET Core HTTPS developer certificate
                .FirstOrDefault(cert => DateTime.UtcNow >= cert.NotBefore.ToUniversalTime() && DateTime.UtcNow <= cert.NotAfter.ToUniversalTime());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Failed to load ASP.NET Core developer certificate from store: {ex.Message}");
        }
        return null;
    }

    private static readonly HashSet<string> _attemptedMacTrustThumbprints = new(StringComparer.OrdinalIgnoreCase);

    private static bool IsCertificateTrustedOnMac(string crtPath, X509Certificate2 cert)
    {
        try
        {
            // Check native macOS Security Framework trust via security verify-cert
            if (File.Exists(crtPath))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "security",
                    Arguments = $"verify-cert -c \"{crtPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
                using var process = Process.Start(psi);
                if (process != null)
                {
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                        return true;
                }

                // Check if certificate is present in System.keychain
                var checkSystemPsi = new ProcessStartInfo
                {
                    FileName = "security",
                    Arguments = "find-certificate -c localhost /Library/Keychains/System.keychain",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };
                using var sysProc = Process.Start(checkSystemPsi);
                if (sysProc != null)
                {
                    sysProc.WaitForExit();
                    if (sysProc.ExitCode == 0)
                        return true;
                }
            }

            // Check CurrentUser Root store
            using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                if (store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, validOnly: false).Count > 0)
                    return true;
            }

            // Check LocalMachine Root store (where installer trusts cert system-wide in System.keychain)
            using (var lmStore = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
            {
                lmStore.Open(OpenFlags.ReadOnly);
                if (lmStore.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, validOnly: false).Count > 0)
                    return true;
            }

            // Check LocalMachine My store as fallback
            using (var lmMyStore = new X509Store(StoreName.My, StoreLocation.LocalMachine))
            {
                lmMyStore.Open(OpenFlags.ReadOnly);
                if (lmMyStore.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, validOnly: false).Count > 0)
                    return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Failed to check macOS root certificate trust: {ex.Message}");
            return false;
        }
    }

    private static void TrustCertificateOnMac(string crtPath, X509Certificate2 cert)
    {
        // Don't repeat runtime security prompt if already attempted for this cert
        if (!_attemptedMacTrustThumbprints.Add(cert.Thumbprint))
            return;

        try
        {
            Console.WriteLine($"[INFO] Trusting self-signed certificate on macOS: {crtPath}");
            var psi = new ProcessStartInfo
            {
                FileName = "security",
                Arguments = $"add-trusted-cert -r trustRoot -k \"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/Library/Keychains/login.keychain-db\" \"{crtPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };
            using var process = Process.Start(psi);
            if (process != null)
            {
                var stderrTask = process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                var stderr = stderrTask.Result;

                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"[WARNING] Certificate trust command exited with code {process.ExitCode}. Error: {stderr}");
                }
            }
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
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Failed to check Windows root certificate trust: {ex.Message}");
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
