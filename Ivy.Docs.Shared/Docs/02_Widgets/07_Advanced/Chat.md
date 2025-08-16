# Chat

The `Chat` widget renders a conversation between a user and an assistant.

Messages are supplied as `ChatMessage` objects and new messages are sent through the `OnSendMessage` event.

## Basic Chat

A simple chat with an echo bot that repeats user messages.

This demonstrates the fundamental usage of the Chat widget with basic message handling and state management.

```csharp demo-tabs 
public class BasicChatDemo : ViewBase
{   
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant, "Hello! I'm an echo bot. I'll repeat whatever you say!")
        ));

        void OnSendMessage(Event<Chat, string> @event)
        {
            var currentMessages = messages.Value;
            messages.Set(currentMessages.Add(new ChatMessage(ChatSender.User, @event.Value)));
            messages.Set(currentMessages.Add(new ChatMessage(ChatSender.Assistant, $"You said: {@event.Value}")));
        }

        return new Chat(messages.Value.ToArray(), OnSendMessage)
            .Width(Size.Full().Max(400))
            .Height(Size.Auto());
    }
}
```

## AI Assistant with Loading States

A chat that simulates AI processing with loading indicators.

This example shows how to implement async message handling, display loading states using ChatStatus, and manage message updates during processing.

```csharp demo-tabs 
public class LoadingChatDemo : ViewBase
{   
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant, "I'm an AI assistant! Ask me anything and I'll respond with a loading state.")
        ));

        async void OnSendMessage(Event<Chat, string> @event)
        {
            var currentMessages = messages.Value;
            messages.Set(currentMessages.Add(new ChatMessage(ChatSender.User, @event.Value)));
            
            // Show loading state
            messages.Set(currentMessages.Add(new ChatMessage(ChatSender.Assistant, new ChatStatus("Thinking..."))));
            
            // Simulate processing delay
            await Task.Delay(2000);
            
            // Remove loading and add response
            var updatedMessages = messages.Value.Take(messages.Value.Length - 1).ToImmutableArray();
            messages.Set(updatedMessages.Add(new ChatMessage(ChatSender.Assistant, 
                $"I processed your message: '{@event.Value}'. Here's a thoughtful response based on what you said.")));
        }

        return new Chat(messages.Value.ToArray(), OnSendMessage)
            .Width(Size.Full().Max(400))
            .Height(Size.Auto());
    }
}
```

## Interactive Chat with Rich Content

A chat that responds with interactive elements like buttons and cards.

This demonstrates how to return complex UI components as chat responses, creating dynamic and engaging conversations with rich media content.

```csharp demo-tabs 
public class InteractiveChatDemo : ViewBase
{   
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant, "I can show interactive elements! Try sending 'buttons', 'card', or 'form' to see different responses.")
        ));

        void OnSendMessage(Event<Chat, string> @event)
        {
            var currentMessages = messages.Value;
            messages.Set(currentMessages.Add(new ChatMessage(ChatSender.User, @event.Value)));
            
            object response = @event.Value.ToLower() switch
            {
                "buttons" => Layout.Horizontal().Gap(1)
                    | new Button("Primary").Variant(ButtonVariant.Primary)
                    | new Button("Secondary").Variant(ButtonVariant.Secondary)
                    | new Button("Outline").Variant(ButtonVariant.Outline),
                
                "card" => new Card("Interactive Card", new Button("Action")),
                
                "form" => Layout.Vertical().Gap(1)
                    | new TextInput().Placeholder("Enter your name")
                    | new TextInput().Placeholder("Enter your email")
                    | new Button("Submit").Variant(ButtonVariant.Primary),
                
                _ => $"You said: '{@event.Value}'. Try sending 'buttons', 'card', or 'form' for interactive responses!"
            };
            
            messages.Set(currentMessages.Add(new ChatMessage(ChatSender.Assistant, response)));
        }

        return new Chat(messages.Value.ToArray(), OnSendMessage)
            .Width(Size.Full().Max(400))
            .Height(Size.Auto());
    }
}
```

## Error Handling Chat

A chat that demonstrates error handling and different message types.

This example shows how to use the Error widget for different message severities and how to integrate ChatStatus for loading indicators within chat conversations.

```csharp demo-tabs 
public class ErrorHandlingChatDemo : ViewBase
{   
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant, "I demonstrate error handling! Try sending 'error', 'warning', or 'success' to see different message types.")
        ));

        void OnSendMessage(Event<Chat, string> @event)
        {
            var currentMessages = messages.Value;
            messages.Set(currentMessages.Add(new ChatMessage(ChatSender.User, @event.Value)));
            
            object response = @event.Value.ToLower() switch
            {
                "error" => new Error("Something went wrong!", "This is an error message! Something went wrong."),
                
                "warning" => new Error("Be careful!", "This is a warning message! Be careful."),
                
                "success" => new Error("Great job!", "This is a success message! Everything worked."),
                
                "loading" => new ChatStatus("Processing your request..."),
                
                _ => $"You said: '{@event.Value}'. Try sending 'error', 'warning', 'success', or 'loading'!"
            };
            
            messages.Set(currentMessages.Add(new ChatMessage(ChatSender.Assistant, response)));
        }

        return new Chat(messages.Value.ToArray(), OnSendMessage)
            .Width(Size.Full().Max(400))
            .Height(Size.Auto());
    }
}
```

## Advanced Chat with Commands

A sophisticated chat that responds to specific commands with different content types.

This example showcasing the full range of Ivy widgets that can be embedded in chat responses.

```csharp demo-tabs 
public class AdvancedChatDemo : ViewBase
{   
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant, 
                "Welcome to the Advanced Chat! Try these commands:\n" +
                "• 'analyze code' - I'll show code analysis\n" +
                "• 'create form' - I'll show an interactive form\n" +
                "• 'show chart' - I'll display a chart\n" +
                "• 'table data' - I'll show tabular data\n" +
                "• Any other message - I'll respond normally")
        ));

        void OnSendMessage(Event<Chat, string> @event)
        {
            var currentMessages = messages.Value;
            messages.Set(currentMessages.Add(new ChatMessage(ChatSender.User, @event.Value)));
            
            object response = @event.Value.ToLower() switch
            {
                "analyze code" => new Code("""
                    public class Example
                    {
                        public string ProcessData(string input)
                        {
                            if (string.IsNullOrEmpty(input))
                                return "Empty input";
                            
                            return input.ToUpper();
                        }
                    }
                    """, Languages.Csharp),
                
                "create form" => Layout.Vertical().Gap(1)
                    | new TextInput().Placeholder("Enter project name")
                    | new TextInput().Placeholder("Enter description")
                    | new SelectInput<string>(new[] { new Option<string>("Web", "Web"), new Option<string>("Mobile", "Mobile"), new Option<string>("Desktop", "Desktop") })
                    | new SelectInput<string>(new[] { new Option<string>("Low", "Low"), new Option<string>("Medium", "Medium"), new Option<string>("High", "High") })
                    | new Button("Create Project").Variant(ButtonVariant.Primary),
                
                "show chart" => new LineChart(
                    new[] { 
                        new { Month = "Jan", Value = 10 },
                        new { Month = "Feb", Value = 20 },
                        new { Month = "Mar", Value = 15 },
                        new { Month = "Apr", Value = 25 },
                        new { Month = "May", Value = 30 },
                        new { Month = "Jun", Value = 35 },
                        new { Month = "Jul", Value = 40 }
                    }, 
                    "Value", 
                    "Month"
                ).Height(Size.Units(50)),
                
                "table data" => new Table(
                    new TableRow(new TableCell("Name"), new TableCell("Age"), new TableCell("Role")).IsHeader(),
                    new TableRow(new TableCell("John Doe"), new TableCell("30"), new TableCell("Developer")),
                    new TableRow(new TableCell("Jane Smith"), new TableCell("25"), new TableCell("Designer"))
                ),
                
                _ => $"You said: '{@event.Value}'. Try the commands: 'analyze code', 'create form', 'show chart', or 'table data'!"
            };
            
            messages.Set(currentMessages.Add(new ChatMessage(ChatSender.Assistant, response)));
        }

        return new Chat(messages.Value.ToArray(), OnSendMessage)
            .Width(Size.Full().Max(400))
            .Height(Size.Auto());
    }
}
```

## Chat with Custom Placeholder

Customize the input placeholder text.

This example shows how to use the Placeholder extension method to provide custom guidance text for users, improving the user experience by making it clear what type of input is expected.

```csharp demo-tabs 
public class CustomPlaceholderDemo : ViewBase
{   
    public override object? Build()
    {
        var messages = UseState(ImmutableArray.Create<ChatMessage>(
            new ChatMessage(ChatSender.Assistant, "This chat has a custom placeholder text. Try typing something!")
        ));

        void OnSendMessage(Event<Chat, string> @event)
        {
            var currentMessages = messages.Value;
            messages.Set(currentMessages.Add(new ChatMessage(ChatSender.User, @event.Value)));
            messages.Set(currentMessages.Add(new ChatMessage(ChatSender.Assistant, $"Thanks for your message: {@event.Value}")));
        }

        return new Chat(messages.Value.ToArray(), OnSendMessage)
            .Placeholder("Type your message here...")
            .Width(Size.Full().Max(400))
            .Height(Size.Auto());
    }
}
```

<WidgetDocs Type="Ivy.Chat" ExtensionTypes="Ivy.ChatExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Chat.cs"/>
