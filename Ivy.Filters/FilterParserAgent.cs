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
You are an intelligent filter expression converter. Your task is to convert natural language filter expressions into the formal filter grammar, using creative interpretation when needed.

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

**INTELLIGENT FIELD INTERPRETATION:**
When a query references a field with an incompatible operation, look for related fields that make sense:

1. **Type Mismatch Resolution:**
   - If user requests numeric comparison on a text/icon field, look for related numeric fields
   - Example: "[Activity] above 45" where Activity is text/icon → Infer they mean a related numeric field like Age
   - Example: "Status greater than 5" where Status is text → Look for numeric fields that could relate

2. **Conceptual Query Mapping:**
   - Map abstract concepts to available fields using common sense
   - Example: "drinking age in sweden" → Interpret as age restriction → [Age] >= 18
   - Example: "retirement age" → [Age] >= 65
   - Example: "minor users" → [Age] < 18
   - Example: "senior citizens" → [Age] >= 65

3. **Superlative and Positional Queries:**
   - Convert superlatives to appropriate comparisons when possible
   - Example: "who is the eldest?" → Since we can't do MAX, suggest [Age] is not blank (note: results need sorting)
   - Example: "the youngest users" → [Age] is not blank (note: results need sorting by age ascending)
   - Example: "first user created" → [Created Date] is not blank (note: results need sorting by date)
   - Example: "most recent users" → If there's a date field, use date comparison like [Created Date] >= "2024-01-01"

4. **Compound Field Decomposition:**
   - When query references subfields not available, work with the compound field creatively
   - Example: "first name starts with J and last name starts with S" with only [Name] field →
     Transform to: [Name] starts with "J" (for first name starting with J)
     Note: Full last name filtering not possible with current grammar
   - Example: "last name Smith" with only [Name] field → [Name] contains "Smith"

5. **Domain Knowledge Application:**
   - Apply common domain knowledge to interpret queries
   - Example: "active users" → [Is Active] = true OR [Status] = "active" (depending on available fields)
   - Example: "VIP customers" → Look for priority, status, or tier fields
   - Example: "verified accounts" → Look for verification, status, or confirmed fields

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

**Intelligent Interpretation Examples:**

Example 8 (Type Mismatch - Icon field with numeric comparison):
Input: "[Activity] show all above 45"
Reasoning: Activity is an icon/text field, numeric comparison likely refers to Age
Output: [Age] > 45

Example 9 (Conceptual Query - Domain knowledge):
Input: "drinking age in sweden"
Reasoning: Legal drinking age concept maps to age restriction
Output: [Age] >= 18

Example 10 (Superlative Query):
Input: "who is the eldest?"
Reasoning: Cannot express MAX in grammar, return all with ages for client-side sorting
Output: [Age] is not blank

Example 11 (Compound Field Handling):
Input: "first name starting with O and last name starting with S"
Reasoning: Only [Name] field exists, partial match on first name
Output: [Name] starts with "O"

Example 12 (Positional Query):
Input: "who is the first user created?"
Reasoning: Cannot express ordinal position, return all with dates for sorting
Output: [Created] is not blank

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
- BEFORE calling the fail tool, attempt intelligent interpretation using the guidelines above
- Only call fail tool if there's absolutely no reasonable interpretation possible

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

**INTELLIGENT FIELD SELECTION FOR TYPE MISMATCHES:**
When a field is referenced with an incompatible operation:
1. First check if there's a semantically related field of the correct type
2. Look for fields that commonly correlate (e.g., Status icons often correlate with Age ranges)
3. Consider the overall context of the query to infer the intended field
4. If multiple numeric fields exist, choose the most logical one based on the value range mentioned

Examples:
- "[Activity] above 45" + Activity is icon → Check for Age field (45 is typical age range)
- "[Status] greater than 1000" + Status is text → Check for Salary/Price fields (1000+ suggests money)
- "[Priority] less than 30" + Priority is boolean → Check for Age field (under 30 is common age filter)

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