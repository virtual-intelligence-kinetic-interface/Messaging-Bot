using System.Text;
using System.Text.Json;
using MessagingBot;
using Spectre.Console;

// ─── Setup ────────────────────────────────────────────────────────────────────

var configPath = "appsettings.json";
if (!File.Exists(configPath))
    throw new FileNotFoundException($"Configuration file not found: {configPath}");

var jsonlPath = "sample.jsonl";
if (!File.Exists(jsonlPath))
    throw new FileNotFoundException($"JSONL file not found: {jsonlPath}");

var config = JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync(configPath)) ?? throw new InvalidOperationException("Failed to deserialize configuration");
var agent = new MessagingAgent(config.Anthropic.ApiKey, config.Anthropic.Model);

AnsiConsole.Write(new FigletText("MSG BOT").Color(Color.Cyan1));
AnsiConsole.MarkupLine("[grey]Context-aware messaging agent · infers all rules from input data[/]\n");

// ─── Parse JSONL ──────────────────────────────────────────────────────────────

var lines = (await File.ReadAllLinesAsync(jsonlPath, Encoding.UTF8))
    .Select(l => l.Trim())
    .Where(l => l.Length > 0)
    .ToList();

var records = new List<InputRecord>();
foreach (var (line, i) in lines.Select((l, i) => (l, i)))
{
    try
    {
        var record = JsonSerializer.Deserialize<InputRecord>(line,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new Exception("null result");
        records.Add(record);
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]✗ Line {i + 1} parse error:[/] {ex.Message}");
    }
}

AnsiConsole.MarkupLine($"[green]✓[/] Loaded [bold]{records.Count}[/] record(s) from [dim]{jsonlPath}[/]\n");

// ─── Run Agent ────────────────────────────────────────────────────────────────

var results = new List<(InputRecord Record, AgentResult? Result, string? Error)>();

foreach (var record in records)
{
    AgentResult? result = null;
    string? error = null;

    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .SpinnerStyle(Style.Parse("cyan"))
        .StartAsync($"Processing [bold]{record.TaskId}[/]...", async _ =>
        {
            try { result = await agent.ProcessAsync(record); }
            catch (Exception ex) { error = ex.Message; }
        });

    results.Add((record, result, error));
    RenderResult(record, result, error);
}

// ─── Summary ──────────────────────────────────────────────────────────────────

var successful = results.Where(r => r.Result != null).ToList();
if (successful.Count > 0)
{
    var avg = successful.Average(r => r.Result!.SemanticMatchScore) * 100;
    AnsiConsole.MarkupLine(
        $"\n[bold]Summary:[/] {successful.Count}/{results.Count} processed · " +
        $"avg semantic match [bold {(avg >= 85 ? "green" : avg >= 65 ? "yellow" : "red")}]{avg:F0}%[/]");
}

// ─── Renderer ─────────────────────────────────────────────────────────────────

static void RenderResult(InputRecord record, AgentResult? result, string? error)
{
    var channelColor = result?.Output.NextMessage.Channel switch
    {
        "sms" => "cyan",
        "email" => "magenta",
        "voice" => "yellow",
        _ => "grey"
    };

    var panel = new Panel(BuildContent(record, result, error))
    {
        Header = new PanelHeader($" [bold {channelColor}]{record.TaskId}[/] ", Justify.Left),
        Border = BoxBorder.Rounded,
        Padding = new Padding(1, 0),
    };

    AnsiConsole.Write(panel);
    AnsiConsole.WriteLine();
}

static Markup BuildContent(InputRecord record, AgentResult? result, string? error)
{
    if (error != null)
        return new Markup($"[red]✗ Error:[/] {Markup.Escape(error)}");

    if (result == null)
        return new Markup("[grey]No result[/]");

    var msg = result.Output.NextMessage;
    var act = result.Output.NextAction;
    var r = result.Reasoning;
    var pct = (int)(result.SemanticMatchScore * 100);
    var scoreColor = pct >= 85 ? "green" : pct >= 65 ? "yellow" : "red";

    var sb = new StringBuilder();

    // Channel + time + score
    sb.AppendLine($"[bold]Channel:[/]  [{channelColor(msg.Channel)}]{msg.Channel.ToUpper()}[/]   " +
                  $"[bold]Send at:[/] [dim]{msg.SendAt}[/]   " +
                  $"[bold]Match:[/] [{scoreColor}]{pct}%[/]");
    sb.AppendLine();

    // Subject (email only)
    if (!string.IsNullOrWhiteSpace(msg.Subject))
        sb.AppendLine($"[bold]Subject:[/]  {Markup.Escape(msg.Subject)}");

    // Body
    sb.AppendLine($"[bold]Message:[/]");
    foreach (var line in msg.Body.Split('\n'))
        sb.AppendLine($"  [italic]{Markup.Escape(line)}[/]");

    // CTA + next action
    sb.AppendLine();
    if (msg.Cta.HasValue)
        sb.AppendLine($"[bold]CTA:[/]      [dim]{Markup.Escape(msg.Cta.Value.ToString())}[/]");
    sb.AppendLine($"[bold]Next:[/]     [dim]{act.Type}" +
                  $"{(act.Name != null ? $" · {act.Name}" : "")}" +
                  $"{(act.Value.HasValue ? $" · +{act.Value}d" : "")}[/]");

    // Reasoning
    sb.AppendLine();
    sb.AppendLine("[bold underline]Reasoning[/]");
    sb.AppendLine($"  [cyan]Channel:[/]    {Markup.Escape(r.ChannelDecision)}");
    sb.AppendLine($"  [cyan]Timing:[/]     {Markup.Escape(r.TimingDecision)}");
    sb.AppendLine($"  [cyan]Content:[/]    {Markup.Escape(r.ContentDecision)}");
    sb.AppendLine($"  [cyan]Compliance:[/] {Markup.Escape(r.ComplianceNotes)}");

    // Assertions
    sb.AppendLine();
    sb.AppendLine("[bold underline]Assertions[/]");
    foreach (var (key, val) in result.AssertionsPassed)
        sb.AppendLine($"  {(val ? "[green]✓[/]" : "[red]✗[/]")} {key.Replace("_", " ")}");

    return new Markup(sb.ToString());

    static string channelColor(string ch) => ch switch
    {
        "sms" => "cyan",
        "email" => "magenta",
        "voice" => "yellow",
        _ => "grey"
    };
}
