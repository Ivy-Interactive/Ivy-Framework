﻿using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Ivy.Docs.Tools;

public static class Utils
{
    public static string GetPathForLink(string source, string link)
    {
        var sourceDir = Path.GetDirectoryName(source) ?? "";
        var combined = Path.Combine(sourceDir, link);
        var normalized = Path.GetFullPath(combined, Directory.GetCurrentDirectory());
        var cwd = Path.GetFullPath(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar);

        if (normalized.StartsWith(cwd, StringComparison.OrdinalIgnoreCase))
            return normalized.Substring(cwd.Length).Replace('\\', '/');

        return normalized.Replace('\\', '/');
    }

    public static string GetTypeNameFromPath(string path)
    {
        path = path.EatRight(".md");
        var parts = path
            .Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => Regex.Replace(p, @"^\d+_", ""))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();
        return string.Join(".", [.. parts[..^1], parts[^1] + "App"]);
    }

    public static string GetAppIdFromTypeName(string typeName)
    {
        var ns = typeName.Split(".");
        if (ns.Contains("Apps"))
        {
            ns = ns[(Array.IndexOf(ns, "Apps") + 1)..];
        }
        return string.Join("/", ns.Select(Utils.TitleCaseToFriendlyUrl));
    }

    /// <summary>
    /// FooBar => foo-bar
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string TitleCaseToFriendlyUrl(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // if (input.EndsWith("app", StringComparison.InvariantCultureIgnoreCase))
        // {
        //     input = input[..^3];
        // }

        bool hadUnderscore = input.StartsWith("_");
        if (hadUnderscore)
        {
            input = input[1..];
        }

        StringBuilder sb = new();

        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && i > 0)
            {
                sb.Append('-');
            }

            sb.Append(char.ToLower(input[i]));
        }

        if (hadUnderscore)
        {
            sb.Insert(0, '_');
        }

        return sb.ToString();
    }

    public static string GetRelativeFolderWithoutOrder(string inputFolder, string inputFile)
    {
        var relativePath = Path.GetRelativePath(inputFolder, Path.GetDirectoryName(inputFile)!);
        var parts = relativePath
            .Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => Regex.Replace(p, @"^\d+_", ""))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

        var result = Path.Combine(parts);
        return result == "." ? "" : result;
    }

    public static (int? order, string name) GetOrderFromFileName(string filename)
    {
        // Get name without extension first
        string nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

        var parts = nameWithoutExtension.Split('_');
        if (parts.Length > 1 && int.TryParse(parts[0], out int order))
        {
            return (order, string.Join("_", parts.Skip(1)));
        }
        return (null, nameWithoutExtension);
    }

    public static bool IsView(string code, out string? className)
    {
        className = null;

        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetCompilationUnitRoot();

        var classes = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>();

        foreach (var cls in classes)
        {
            if (cls.BaseList == null) continue;

            var inheritsViewBase = cls.BaseList.Types
                .Any(t => t.Type.ToString() == "ViewBase");

            if (!inheritsViewBase) continue;

            var hasBuildOverride = cls.Members
                .OfType<MethodDeclarationSyntax>()
                .Any(m =>
                    m.Identifier.Text == "Build" &&
                    m.Modifiers.Any(SyntaxKind.OverrideKeyword));

            if (!hasBuildOverride) continue;

            className = cls.Identifier.Text;
            return true;
        }

        return false;
    }

    public static string RenameClass(string code, string className)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();

        var classNode = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classNode == null)
            return code;

        var renamedClass = classNode.WithIdentifier(SyntaxFactory.Identifier(className));
        var newRoot = root.ReplaceNode(classNode, renamedClass);

        return newRoot.NormalizeWhitespace().ToFullString();
    }

    public static string? GetGitFileUrl(string localFilePath)
    {
        try
        {
            // Ensure the file exists
            if (!File.Exists(localFilePath))
                return null;

            // Get the directory containing the file
            string directory = Path.GetDirectoryName(localFilePath)!;

            // Change to the directory
            string currentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(directory);

            // Get the repository remote URL
            string remoteUrl = RunGitCommand("config --get remote.origin.url");
            if (string.IsNullOrEmpty(remoteUrl))
                throw new Exception("No remote origin found for this Git repository.");

            // Clean up the remote URL (convert SSH to HTTPS if needed)
            remoteUrl = ConvertToHttpsUrl(remoteUrl.Trim());

            // Get the repository root directory
            string repoRoot = RunGitCommand("rev-parse --show-toplevel").Trim();

            // Get the current branch name
            string branch = RunGitCommand("rev-parse --abbrev-ref HEAD").Trim();

            // Get the relative path of the file within the repo
            string relativePath = localFilePath;
            if (!string.IsNullOrEmpty(repoRoot))
            {
                relativePath = Path.GetRelativePath(repoRoot, localFilePath);
            }

            // Replace backslashes with forward slashes for URL
            relativePath = relativePath.Replace('\\', '/');

            // Construct the URL
            string fileUrl = $"{remoteUrl}/blob/{branch}/{relativePath}";

            // Restore the original directory
            Directory.SetCurrentDirectory(currentDirectory);

            return fileUrl;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string RunGitCommand(string arguments)
    {
        using Process process = new Process();
        process.StartInfo.FileName = "git";
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            string error = process.StandardError.ReadToEnd();
            throw new Exception($"Git command failed: {error}");
        }

        return output;
    }

    private static string ConvertToHttpsUrl(string gitUrl)
    {
        // Convert SSH URL to HTTPS URL if needed
        // Example: git@github.com:username/repo.git -> https://github.com/username/repo
        if (gitUrl.StartsWith("git@"))
        {
            // SSH format: git@github.com:username/repo.git
            Regex sshRegex = new Regex(@"git@([^:]+):([^\.]+)\.git");
            Match match = sshRegex.Match(gitUrl);

            if (match.Success)
            {
                string host = match.Groups[1].Value;
                string path = match.Groups[2].Value;
                return $"https://{host}/{path}";
            }
        }

        // Already HTTPS format or other format
        // Remove .git suffix if present
        if (gitUrl.EndsWith(".git"))
        {
            gitUrl = gitUrl.Substring(0, gitUrl.Length - 4);
        }

        return gitUrl;
    }

    public static string GetShortHash(string input, int length = 8)
    {
        using var sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        string base64 = System.Convert.ToBase64String(hash);
        return new string(base64.Replace("+", "-").Replace("/", "_").ToLower().Where(char.IsLetterOrDigit).ToArray())[..length];
    }

    public static string EatRight(this string input, char food)
    {
        return EatRight(input, c => c == food);
    }

    public static string EatRight(this string input, Func<char, bool> foodType)
    {
        if (string.IsNullOrEmpty(input)) return input;
        int i = input.Length - 1;
        while (i >= 0)
        {
            if (foodType(input[i]))
            {
                i--;
            }
            else
            {
                break;
            }
        }
        return input.Substring(0, i + 1);
    }

    public static string EatRight(this string input, string food, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(food)) return input;

        int cursor = input.Length;
        while (true)
        {
            if (cursor - food.Length >= 0)
            {
                if (input.Substring(cursor - food.Length, food.Length).Equals(food, stringComparison))
                {
                    cursor = cursor - food.Length;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        return input.Substring(0, cursor);
    }
}