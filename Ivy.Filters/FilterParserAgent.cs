using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace Ivy.Filters;

public class FilterParserAgent(IChatClient chatClient, ILogger? logger = null)
{
    private const int MaxRetries = 2;
    private readonly ISerializer _yamlSerializer = new SerializerBuilder().Build();
    private readonly IChatClient _chatClient = chatClient
        .AsBuilder().UseFunctionInvocation().Build();

    public async Task<FilterParseResult> Parse(string filterExpression, FieldMeta[] fields)
    {
        var fieldsYaml = SerializeFieldsAsYaml(fields);
        var grammarContent = LoadGrammarFile();

        var chatOptions = new ChatOptions
        {
            Tools = [
                AIFunctionFactory.Create(
                    (string expression) =>
                    {
                        var parser = new FilterParser(fields);
                        var result = parser.Parse(expression);

                        if (result.HasErrors)
                        {
                            var errors = string.Join("; ", result.Diagnostics.Select(d => d.Message));
                            return $"PARSE_ERROR: {errors}";
                        }

                        return "PARSE_SUCCESS";
                    },
                    name: "parse_filter",
                    description: "Parses and validates a filter expression against the available fields. Returns success status and any error messages."
                ),
                AIFunctionFactory.Create(
                    (string reason) => $"PARSE_FAILED: {reason}",
                    name: "fail",
                    description: "Call this when it's impossible to create a valid filter from the user's expression with the available columns."
                )
            ]
        };

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, BuildSystemPrompt(fieldsYaml, grammarContent)),
            new(ChatRole.User, $"Convert this filter expression to the filter grammar:\n{filterExpression}")
        };

        // Track total usage across all attempts
        long totalInputTokens = 0;
        long totalOutputTokens = 0;
        string? lastAttemptedExpression = null;
        string? lastAttemptErrors = null;

        // Try up to MaxRetries times
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            logger?.LogDebug("Filter parsing attempt {Attempt}/{MaxRetries}", attempt, MaxRetries);

            try
            {
                var completion = await _chatClient.GetResponseAsync(messages, chatOptions);

                // Track usage
                if (completion.Usage != null)
                {
                    totalInputTokens += completion.Usage.InputTokenCount ?? 0;
                    totalOutputTokens += completion.Usage.OutputTokenCount ?? 0;
                }

                // Check if the agent called the fail tool
                var responseText = completion.Text ?? string.Empty;
                if (responseText.Contains("PARSE_FAILED:"))
                {
                    var reason = responseText.Replace("PARSE_FAILED:", "").Trim();
                    return new FilterParseResult
                    {
                        Filter = filterExpression,
                        Diagnostics =
                        [
                            new Diagnostic(DiagnosticSeverity.Error, reason, 1, 0)
                        ],
                        Usage = new UsageInfo
                        {
                            InputTokens = totalInputTokens,
                            OutputTokens = totalOutputTokens,
                            TotalTokens = totalInputTokens + totalOutputTokens
                        },
                        Iterations = attempt
                    };
                }

                // Extract the filter expression from the response
                var filterText = ExtractFilterExpression(responseText);

                if (string.IsNullOrWhiteSpace(filterText))
                {
                    logger?.LogWarning("AI did not provide a filter expression on attempt {Attempt}", attempt);
                    messages.Add(new ChatMessage(ChatRole.Assistant, responseText));
                    messages.Add(new ChatMessage(ChatRole.User,
                        "Please call the parse_filter tool with your generated filter expression."));
                    continue;
                }

                // Try to parse the generated expression
                var parser = new FilterParser(fields);
                var result = parser.Parse(filterText);

                if (!result.HasErrors)
                {
                    logger?.LogInformation("Successfully parsed filter on attempt {Attempt}", attempt);
                    return result with
                    {
                        Usage = new UsageInfo
                        {
                            InputTokens = totalInputTokens,
                            OutputTokens = totalOutputTokens,
                            TotalTokens = totalInputTokens + totalOutputTokens
                        },
                        Iterations = attempt
                    };
                }

                // Parsing failed, store the attempt details and provide feedback to the agent
                lastAttemptedExpression = filterText;
                var errors = string.Join(", ", result.Diagnostics.Select(d => d.Message));
                lastAttemptErrors = errors;
                logger?.LogWarning("Parse failed on attempt {Attempt}/{MaxRetries}. Expression: '{Expression}', Errors: {Errors}",
                    attempt, MaxRetries, filterText, errors);

                messages.Add(new ChatMessage(ChatRole.Assistant, responseText));
                messages.Add(new ChatMessage(ChatRole.User,
                    $"The filter expression failed to parse with these errors: {errors}\n" +
                    $"Please try again with a corrected expression."));
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error during filter parsing attempt {Attempt}/{MaxRetries}", attempt, MaxRetries);

                if (attempt == MaxRetries)
                {
                    var exceptionErrorMessage = $"Failed to parse filter after {MaxRetries} attempts: {ex.Message}";
                    if (!string.IsNullOrEmpty(lastAttemptedExpression))
                    {
                        exceptionErrorMessage += $". Last attempted expression: '{lastAttemptedExpression}'";
                    }
                    if (!string.IsNullOrEmpty(lastAttemptErrors))
                    {
                        exceptionErrorMessage += $". Parser errors: {lastAttemptErrors}";
                    }

                    return new FilterParseResult
                    {
                        Filter = filterExpression,
                        Diagnostics =
                        [
                            new Diagnostic(DiagnosticSeverity.Error, exceptionErrorMessage, 1, 0)
                        ],
                        Usage = new UsageInfo
                        {
                            InputTokens = totalInputTokens,
                            OutputTokens = totalOutputTokens,
                            TotalTokens = totalInputTokens + totalOutputTokens
                        },
                        Iterations = attempt
                    };
                }

                messages.Add(new ChatMessage(ChatRole.User,
                    $"An error occurred: {ex.Message}. Please try again."));
            }
        }

        // Build detailed error message
        var errorMessage = $"Failed to generate valid filter expression after {MaxRetries} attempts";
        if (!string.IsNullOrEmpty(lastAttemptedExpression))
        {
            errorMessage += $". Last attempted expression: '{lastAttemptedExpression}'";
        }
        if (!string.IsNullOrEmpty(lastAttemptErrors))
        {
            errorMessage += $". Parser errors: {lastAttemptErrors}";
        }

        logger?.LogError("Filter parsing exhausted all retries. {ErrorMessage}", errorMessage);

        return new FilterParseResult
        {
            Filter = filterExpression,
            Diagnostics =
            [
                new Diagnostic(DiagnosticSeverity.Error, errorMessage, 1, 0)
            ],
            Usage = new UsageInfo
            {
                InputTokens = totalInputTokens,
                OutputTokens = totalOutputTokens,
                TotalTokens = totalInputTokens + totalOutputTokens
            },
            Iterations = MaxRetries
        };
    }

    private string SerializeFieldsAsYaml(FieldMeta[] fields)
    {
        var fieldsList = fields.Select(f => new
        {
            f.DisplayName,
            f.ColId,
            Type = f.Type.ToString()
        }).ToList();

        return _yamlSerializer.Serialize(fieldsList);
    }

    private static string LoadGrammarFile()
    {
        var assembly = typeof(FilterParserAgent).Assembly;
        var resourceName = "Ivy.Filters.Filters.g4";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string BuildSystemPrompt(string fieldsYaml, string grammarContent)
    {
        return $"""
You are a filter expression converter. Your task is to convert natural language filter expressions into the formal filter grammar.

**Available Fields:**
```yaml
{fieldsYaml}
```

**Complete ANTLR4 Grammar Definition:**
```antlr4
{grammarContent}
```

**Key Grammar Rules Summary:**
- Field references must be enclosed in square brackets: [FieldName]
- Text operations: [Field] contains "text", [Field] starts with "text", [Field] ends with "text"
- Text operations can be negated: [Field] not contains "text"
- Comparisons: [Field] = value, [Field] != value, [Field] > value, [Field] >= value, [Field] < value, [Field] <= value
- You can also use: equals, not equals, greater than, greater than or equal, less than, less than or equal
- Existence: [Field] is blank, [Field] is not blank
- Logical operators: AND, OR, NOT
- Grouping: use parentheses for precedence
- **CRITICAL: String literals MUST use double quotes (") - NOT single quotes (')**
- Numbers can be integers or decimals with optional sign
- **CRITICAL: All keywords are case-insensitive (IS, BLANK, AND, OR, NOT, CONTAINS, etc.)**

**CRITICAL: Automatic Syntax Correction**
You MUST automatically correct common syntax errors in user input:

1. **Single Quotes → Double Quotes:**
   - WRONG: [Name] = 'John'
   - CORRECT: [Name] = "John"
   - ALWAYS convert single quotes to double quotes for string literals

2. **Missing "is" before blank:**
   - WRONG: [Email] blank
   - CORRECT: [Email] is blank
   - WRONG: [Department] not blank
   - CORRECT: [Department] is not blank
   - ALWAYS include the word "is" before blank/not blank

**Examples of Syntax Correction:**
- Input: [Name] contains 'John' → Output: [Name] contains "John"
- Input: [Email] blank → Output: [Email] is blank
- Input: [Status] = 'active' → Output: [Status] = "active"
- Input: [Department] not blank → Output: [Department] is not blank

**Examples of Conversions:**

Example 1:
Input: "Show me orders where status is active"
Output: [Status] = "active"

Example 2:
Input: "Find customers with name containing John and age over 30"
Output: [Name] contains "John" AND [Age] > 30

Example 3:
Input: "Products priced between 10 and 100"
Output: [Price] >= 10 AND [Price] <= 100

Example 4:
Input: "Incomplete records (where description is empty)"
Output: [Description] is blank

Example 5:
Input: "Sales from New York or California, but not pending"
Output: ([Region] = "New York" OR [Region] = "California") AND [Status] != "pending"

Example 6:
Input: "Items that don't contain the word 'test'"
Output: [Name] not contains "test"

Example 7:
Input: "Age is not blank and greater than 18"
Output: [Age] is not blank AND [Age] > 18

**CRITICAL: Output Format**
- **YOUR RESPONSE MUST BE ONLY THE FILTER EXPRESSION - NOTHING ELSE**
- **DO NOT include any explanations, reasoning, or commentary**
- **DO NOT say "I need to convert..." or "Looking at the fields..." or any other preamble**
- **DO NOT wrap the expression in extra brackets or add trailing ']'**
- **ONLY output the final filter expression itself**
- Example of CORRECT response: [Age] > 30 AND [Country] = "USA"
- Example of WRONG response: I need to convert this filter. Looking at the fields, I can see there's an Age field. [Age] > 30 AND [Country] = "USA"
- Example of WRONG response: [Status] != "inactive"]
- Example of CORRECT response: [Status] != "inactive"

**Important Instructions:**
- Field names in the output MUST use the DisplayName exactly as shown in the available fields
- Use [DisplayName] in your filter expression, not [ColId]
- When you have a valid filter expression, call the parse_filter tool to validate it
- If it's impossible to create a filter with the available fields, call the fail tool with a reason

**CRITICAL: Be Forgiving with Human Input Errors:**
- Users often make typos and spelling mistakes - be intelligent about mapping these to real fields
- If a word is misspelled (e.g., "diabetees" → "Has Diabetes", "prise" → "Price", "scolarship" → "Has Scholarship"):
  * Match it to the most phonetically or visually similar field name
  * Consider both the field DisplayName and the concept it represents
  * DO NOT fail just because of spelling errors - make your best effort to map to the correct field
- If a field name is abbreviated or uses informal language (e.g., "dept" → "Department", "addr" → "Address"):
  * Infer the intended field from context and available options
- If a value is misspelled (e.g., "Samsumg" → "Samsung", "Fransisco" → "Francisco"):
  * Use the correct spelling in your output
- Be flexible with plurals, tenses, and word forms (e.g., "enginering" → "Engineering", "sience" → "Science")
- Only use the fail tool if:
  * There are genuinely NO fields that could reasonably match the user's intent
  * The request is so vague or ambiguous that no meaningful filter can be created
  * The user is asking for something completely outside the available field set

**Examples of Handling Misspellings:**
- "diabetees" → Understand this means diabetes → Use [Has Diabetes] field
- "prise less than 100" → Understand "prise" means price → [Price] < 100
- "Enginering major" → Understand this means Engineering → [Major Department] = "Engineering"
- "scolarship students" → Understand this means scholarship → [Has Scholarship] = true
- "swiming pool" → Understand this means swimming → [Has Pool] = true

**Remember:** Real humans wrote these queries. They make mistakes. Your job is to understand their INTENT and produce valid filter expressions despite typos, misspellings, and informal language. Be helpful, not pedantic.
""";
    }

    private static string ExtractFilterExpression(string response)
    {
        // Remove common AI response patterns
        var text = response.Trim();

        // If the response contains "Output:" or similar markers, extract after it
        var outputMarkers = new[] { "Output:", "Expression:", "Filter:" };
        foreach (var marker in outputMarkers)
        {
            var index = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                text = text[(index + marker.Length)..].Trim();
                break;
            }
        }

        // Remove code blocks if present
        if (text.StartsWith("```"))
        {
            var lines = text.Split('\n');
            text = string.Join('\n', lines.Skip(1).TakeWhile(l => !l.Trim().Equals("```")));
        }

        // Handle multi-line responses where AI includes explanation before the filter
        // Filter expressions typically start with '[', '(', or 'NOT'
        var allLines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (allLines.Length > 1)
        {
            // Find lines that look like filter expressions
            var filterLines = allLines
                .Select(l => l.Trim())
                .Where(l => l.StartsWith('[') || l.StartsWith('(') || l.StartsWith("NOT", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // If we found filter-like lines, use the last one (most likely to be the actual filter)
            if (filterLines.Count > 0)
            {
                text = filterLines.Last();
            }
        }

        text = text.Trim();

        // Remove trailing unbalanced brackets - AI sometimes adds extra ']' at end
        // Count opening and closing brackets
        int openSquare = text.Count(c => c == '[');
        int closeSquare = text.Count(c => c == ']');

        // If there are more closing brackets than opening, remove trailing ']'
        while (closeSquare > openSquare && text.EndsWith(']'))
        {
            text = text[..^1].TrimEnd();
            closeSquare--;
        }

        return text;
    }

}