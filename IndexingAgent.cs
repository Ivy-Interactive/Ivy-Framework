using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace Ivy.Docs.IndexingAgent;

public class DocumentationIndexer
{
    private readonly string _docsPath;
    private readonly List<DocumentIndex> _index;

    public DocumentationIndexer(string docsPath)
    {
        _docsPath = docsPath;
        _index = new List<DocumentIndex>();
    }

    public void GenerateIndex()
    {
        Console.WriteLine("📚 Generating documentation index...");
        
        var markdownFiles = Directory.GetFiles(_docsPath, "*.md", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).Equals("_Index.md", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f);

        foreach (var filePath in markdownFiles)
        {
            var relativePath = Path.GetRelativePath(_docsPath, filePath);
            var content = File.ReadAllText(filePath);
            var docIndex = AnalyzeDocument(relativePath, content);
            _index.Add(docIndex);
        }

        Console.WriteLine($"✅ Generated index for {_index.Count} documents");
    }

    private DocumentIndex AnalyzeDocument(string relativePath, string content)
    {
        var title = ExtractTitle(content);
        var category = DetermineCategory(relativePath);
        var questions = GenerateQuestions(title, content, category);

        return new DocumentIndex
        {
            FilePath = relativePath,
            Title = title,
            Category = category,
            Questions = questions
        };
    }

    private string ExtractTitle(string content)
    {
        var match = Regex.Match(content, @"^#\s+(.+)$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : "Unknown";
    }

    private string DetermineCategory(string relativePath)
    {
        var parts = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length < 2) return "General";
        
        return parts[0] switch
        {
            "01_Onboarding" => parts.Length > 1 ? parts[1] switch
            {
                "01_GettingStarted" => "Getting Started",
                "02_Concepts" => "Concepts",
                "03_CLI" => "CLI",
                _ => "Onboarding"
            } : "Onboarding",
            "02_Widgets" => parts.Length > 1 ? parts[1] switch
            {
                "01_Common" => "Common Widgets",
                "02_Inputs" => "Input Widgets",
                "03_Primitives" => "Primitive Widgets",
                "04_Layouts" => "Layout Widgets",
                "05_Effects" => "Effect Widgets",
                "06_Charts" => "Chart Widgets",
                "07_Advanced" => "Advanced Widgets",
                _ => "Widgets"
            } : "Widgets",
            "03_ApiReference" => "API Reference",
            _ => "General"
        };
    }

    private List<string> GenerateQuestions(string title, string content, string category)
    {
        var questions = new List<string>();
        var lowerTitle = title.ToLower();
        var lowerContent = content.ToLower();

        // Base questions that apply to most documents
        questions.Add($"How do I use {title}?");
        questions.Add($"What is {title}?");

        // Category-specific questions
        if (category.Contains("Widget"))
        {
            questions.Add($"How do I create a {title}?");
            questions.Add($"How do I style a {title}?");
            questions.Add($"Can I customize {title}?");
            questions.Add($"What are the properties of {title}?");
            
            if (lowerContent.Contains("event") || lowerContent.Contains("onclick") || lowerContent.Contains("onchange"))
            {
                questions.Add($"How do I handle {title} events?");
                questions.Add($"What events does {title} support?");
            }
            
            if (lowerContent.Contains("variant") || lowerContent.Contains("type"))
            {
                questions.Add($"What types of {title} are available?");
                questions.Add($"How do I change {title} variants?");
            }
            
            if (lowerContent.Contains("disabled") || lowerContent.Contains("state"))
            {
                questions.Add($"How do I disable {title}?");
                questions.Add($"How do I manage {title} state?");
            }
        }
        else if (category == "Getting Started")
        {
            questions.Add($"How do I get started with {title}?");
            questions.Add($"What do I need to know about {title}?");
            questions.Add($"How do I setup {title}?");
        }
        else if (category == "Concepts")
        {
            questions.Add($"How does {title} work?");
            questions.Add($"When should I use {title}?");
            questions.Add($"What are the benefits of {title}?");
            questions.Add($"How do I implement {title}?");
        }
        else if (category == "CLI")
        {
            questions.Add($"How do I use the {title} command?");
            questions.Add($"What does the {title} command do?");
            questions.Add($"What are the options for {title}?");
        }

        // Content-specific questions based on keywords
        if (lowerContent.Contains("install") || lowerContent.Contains("setup"))
        {
            questions.Add($"How do I install {title}?");
            questions.Add($"How do I setup {title}?");
        }
        
        if (lowerContent.Contains("config") || lowerContent.Contains("option"))
        {
            questions.Add($"How do I configure {title}?");
            questions.Add($"What are the {title} options?");
        }
        
        if (lowerContent.Contains("example") || lowerContent.Contains("demo"))
        {
            questions.Add($"Do you have examples of {title}?");
            questions.Add($"Show me how to use {title}");
        }
        
        if (lowerContent.Contains("troubleshoot") || lowerContent.Contains("error"))
        {
            questions.Add($"How do I troubleshoot {title}?");
            questions.Add($"What are common {title} errors?");
        }
        
        if (lowerContent.Contains("data") || lowerContent.Contains("binding"))
        {
            questions.Add($"How do I bind data to {title}?");
            questions.Add($"How do I use {title} with data?");
        }
        
        if (lowerContent.Contains("form") || lowerContent.Contains("input"))
        {
            questions.Add($"How do I use {title} in forms?");
            questions.Add($"How do I validate {title}?");
        }
        
        if (lowerContent.Contains("layout") || lowerContent.Contains("responsive"))
        {
            questions.Add($"How do I make {title} responsive?");
            questions.Add($"How do I layout {title}?");
        }
        
        if (lowerContent.Contains("theme") || lowerContent.Contains("color"))
        {
            questions.Add($"How do I theme {title}?");
            questions.Add($"How do I change {title} colors?");
        }
        
        if (lowerContent.Contains("animation") || lowerContent.Contains("transition"))
        {
            questions.Add($"How do I animate {title}?");
            questions.Add($"Does {title} support animations?");
        }

        // Add some natural language variations
        var naturalQuestions = new List<string>();
        foreach (var question in questions)
        {
            if (question.StartsWith("How do I"))
            {
                naturalQuestions.Add(question.Replace("How do I", "Can I"));
                naturalQuestions.Add(question.Replace("How do I", "Show me how to"));
            }
            else if (question.StartsWith("What"))
            {
                naturalQuestions.Add(question.Replace("What", "Tell me about"));
            }
        }
        
        questions.AddRange(naturalQuestions);

        return questions.Distinct().Take(15).ToList(); // Limit to 15 most relevant questions
    }

    public void SaveIndex(string outputPath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(_index, options);
        File.WriteAllText(outputPath, json);
        Console.WriteLine($"💾 Index saved to {outputPath}");
    }

    public void PrintIndex()
    {
        Console.WriteLine("\n📋 DOCUMENTATION INDEX\n");
        Console.WriteLine("=".PadRight(60, '='));
        
        var groupedByCategory = _index.GroupBy(d => d.Category).OrderBy(g => g.Key);
        
        foreach (var group in groupedByCategory)
        {
            Console.WriteLine($"\n## {group.Key}");
            Console.WriteLine("-".PadRight(40, '-'));
            
            foreach (var doc in group.OrderBy(d => d.Title))
            {
                Console.WriteLine($"\n### {doc.Title}");
                Console.WriteLine($"📄 File: {doc.FilePath}");
                Console.WriteLine($"🤔 Questions ({doc.Questions.Count}):");
                
                foreach (var question in doc.Questions)
                {
                    Console.WriteLine($"  • {question}");
                }
            }
        }
    }
}

public class DocumentIndex
{
    public string FilePath { get; set; } = "";
    public string Title { get; set; } = "";
    public string Category { get; set; } = "";
    public List<string> Questions { get; set; } = new();
}

public class Program
{
    public static void Main(string[] args)
    {
        var docsPath = Path.Combine(Directory.GetCurrentDirectory(), "Ivy.Docs", "Docs");
        
        if (!Directory.Exists(docsPath))
        {
            Console.WriteLine($"❌ Documentation directory not found: {docsPath}");
            return;
        }

        var indexer = new DocumentationIndexer(docsPath);
        indexer.GenerateIndex();
        indexer.PrintIndex();
        
        var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "documentation-index.json");
        indexer.SaveIndex(outputPath);
        
        Console.WriteLine($"\n✅ Documentation indexing completed!");
        Console.WriteLine($"📊 Total documents indexed: {Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories).Length}");
    }
}