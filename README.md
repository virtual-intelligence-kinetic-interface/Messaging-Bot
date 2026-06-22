# Context-Aware Messaging Bot · C# Console App

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- An Anthropic API key

## Setup

```bash
# 1. Set your API key
export ANTHROPIC_API_KEY=sk-ant-...      # macOS / Linux
set  ANTHROPIC_API_KEY=sk-ant-...        # Windows CMD
$env:ANTHROPIC_API_KEY="sk-ant-..."      # PowerShell

# 2. Restore packages and run
cd MessagingBot
dotnet run
```

## Run against a custom JSONL file

```bash
dotnet run -- path/to/your/records.jsonl
```

## Project structure

```
MessagingBot/
├── MessagingBot.csproj   # SDK: net8.0 + Spectre.Console
├── Models.cs             # Input / output record types
├── MessagingAgent.cs     # Claude API call + response parsing
├── Program.cs            # Entry point + terminal rendering
└── sample.jsonl          # Two test cases (prospect_welcome, long_horizon)
```

## How it works

1. **Parse** — each line of the JSONL file is deserialised into an `InputRecord`
2. **Infer** — `MessagingAgent` sends the full record to Claude with a system prompt
   that says *"infer all rules from the data"* — no hardcoded if/else logic
3. **Decide** — Claude returns a structured JSON object with:
   - `reasoning` — why each decision was made
   - `output.next_message` — channel, send_at, subject, body, CTA
   - `output.next_action` — follow-up cadence
   - `assertions_passed` — compliance checks
   - `semantic_match_score` — 0–1 match against expected
4. **Render** — Spectre.Console prints a colour-coded panel per task

## Adding your own records

Each JSONL line must be a valid JSON object. Minimum required fields:

```json
{
  "task_id": "...",
  "persona": "prospect",
  "lifecycle_stage": "new",
  "consent": { "email_opt_in": true, "sms_opt_in": false, "voice_opt_in": false },
  "channel_preferences": ["email"],
  "input": {
    "property_name": "...",
    "timezone": "America/New_York",
    "language": "en",
    "profile": { "first_name": "..." }
  }
}
```

The `expected` field is optional — it is passed to Claude so it can compute a
`semantic_match_score`, but the agent works without it.
