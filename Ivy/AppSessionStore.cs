using System.Collections.Concurrent;
using Ivy.Apps;
using Spectre.Console;

namespace Ivy;

public class AppSessionStore
{
    public readonly ConcurrentDictionary<string, AppSession> Sessions = new();

    public AppSession? FindChrome(AppSession session)
    {
        if (session.ParentId == null)
        {
            return session.AppDescriptor.IsChrome ? session : null;
        }
        var parent = Sessions.Values.FirstOrDefault(s => s.ConnectionId == session.ParentId);
        return parent == null ? null : FindChrome(parent!);
    }

    public void Dump()
    {
        var rows = Sessions.Values.Select(e => new
        {
            e.MachineId,
            e.AppId,
            e.ConnectionId,
            e.ParentId,
            e.LastInteraction
        });

        var table = new Spectre.Console.Table();
        table.AddColumn("MachineId");
        table.AddColumn("AppId");
        table.AddColumn("ConnectionId");
        table.AddColumn("ParentId");
        table.AddColumn("LastInteraction");

        foreach (var row in rows)
        {
            table.AddRow(row.MachineId, row.AppId, row.ConnectionId, row.ParentId ?? "", row.LastInteraction.ToString("HH:mm:ss"));
        }

        AnsiConsole.Write(table);
    }
}