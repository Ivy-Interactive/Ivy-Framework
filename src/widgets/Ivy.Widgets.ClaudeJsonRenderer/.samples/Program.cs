using Ivy;
using Ivy.Widgets.ClaudeJsonRenderer;

var server = new Server();
server.AddApp<ClaudeJsonRendererDemo>();
await server.RunAsync();

[App]
class ClaudeJsonRendererDemo : ViewBase
{
    public override object Build()
    {
        var sampleStream = """
            {"type":"system","subtype":"init","session_id":"demo-session","tools":[],"model":"claude-sonnet-4-20250514","mcp_servers":[]}
            {"type":"assistant","message":{"id":"msg_01","type":"message","role":"assistant","content":[{"type":"text","text":"I'll help you create a new file. Let me start by checking the current directory structure."}],"model":"claude-sonnet-4-20250514","stop_reason":"tool_use","usage":{"input_tokens":100,"output_tokens":50,"cache_creation_input_tokens":0,"cache_read_input_tokens":0}}}
            {"type":"assistant","message":{"id":"msg_02","type":"message","role":"assistant","content":[{"type":"tool_use","id":"tool_01","name":"Bash","input":{"command":"ls -la"}}],"model":"claude-sonnet-4-20250514","stop_reason":"tool_use","usage":{"input_tokens":150,"output_tokens":30,"cache_creation_input_tokens":0,"cache_read_input_tokens":0}}}
            {"type":"user","message":{"id":"msg_03","type":"message","role":"user","content":[{"type":"tool_result","tool_use_id":"tool_01","content":"total 16\ndrwxr-xr-x  4 user staff  128 Apr 10 10:00 .\ndrwxr-xr-x 20 user staff  640 Apr 10 09:00 ..\n-rw-r--r--  1 user staff  256 Apr 10 10:00 README.md\n-rw-r--r--  1 user staff 1024 Apr 10 10:00 index.ts"}]},"tool_use_result":{"tool_use_id":"tool_01","content":"total 16\ndrwxr-xr-x  4 user staff  128 Apr 10 10:00 .\ndrwxr-xr-x 20 user staff  640 Apr 10 09:00 ..\n-rw-r--r--  1 user staff  256 Apr 10 10:00 README.md\n-rw-r--r--  1 user staff 1024 Apr 10 10:00 index.ts"}}
            {"type":"assistant","message":{"id":"msg_04","type":"message","role":"assistant","content":[{"type":"text","text":"I can see the directory has a README.md and index.ts. Let me create the new component file for you."}],"model":"claude-sonnet-4-20250514","stop_reason":"tool_use","usage":{"input_tokens":200,"output_tokens":40,"cache_creation_input_tokens":0,"cache_read_input_tokens":0}}}
            {"type":"assistant","message":{"id":"msg_05","type":"message","role":"assistant","content":[{"type":"tool_use","id":"tool_02","name":"Write","input":{"file_path":"component.tsx","content":"export const Hello = () => <div>Hello World</div>;"}}],"model":"claude-sonnet-4-20250514","stop_reason":"tool_use","usage":{"input_tokens":250,"output_tokens":35,"cache_creation_input_tokens":0,"cache_read_input_tokens":0}}}
            {"type":"user","message":{"id":"msg_06","type":"message","role":"user","content":[{"type":"tool_result","tool_use_id":"tool_02","content":"File written successfully."}]},"tool_use_result":{"tool_use_id":"tool_02","content":"File written successfully."}}
            {"type":"result","subtype":"success","cost_usd":0.0042,"duration_ms":8500,"duration_api_ms":6200,"is_error":false,"num_turns":3,"result":"Created component.tsx with a Hello World component.","session_id":"demo-session","total_cost_usd":0.0042,"usage":{"input_tokens":700,"output_tokens":155,"cache_creation_input_tokens":0,"cache_read_input_tokens":0}}
            """;

        return new ClaudeJsonRenderer()
            .JsonStream(sampleStream)
            .ShowThinking(true)
            .ShowSystemEvents(true);
    }
}
