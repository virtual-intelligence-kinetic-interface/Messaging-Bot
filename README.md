# 📨 MessagingBot – AI-Powered Outbound Messaging Engine

> A context-aware, AI-driven outbound messaging system for real-estate leasing platforms. Reads prospect/resident records and autonomously decides **what** to send, **on which channel**, **at what time**, with **what content**.

[![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![C# 12](https://img.shields.io/badge/C%23-12-239120?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![AI Model](https://img.shields.io/badge/AI-Claude%20Sonnet%204-9B59B6?logo=anthropic)](https://www.anthropic.com/claude)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## 📖 What Is This?

**MessagingBot** is a zero-dependency .NET 8 console application that uses Claude Sonnet 4 to generate personalized, compliant outbound messages for real-estate prospects and residents. It:

- ✅ **Reads JSONL records** – Prospect/resident data with consent, preferences, and context
- ✅ **Infers rules dynamically** – No hardcoded if/else logic; the AI adapts to each record
- ✅ **Selects channels intelligently** – SMS, Email, or Voice based on consent + context
- ✅ **Generates personalized content** – Messages that reference specific amenities, move dates, and resident history
- ✅ **Self-audits compliance** – Fair housing, TCPA, brand guidelines, PII protection
- ✅ **Rich terminal UI** – Color-coded output with reasoning, assertions, and quality scores

---

## 🚀 Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [Anthropic API Key](https://console.anthropic.com/)

### Clone & Run

```bash
git clone https://github.com/yourusername/messagingbot.git
cd messagingbot
dotnet build
```

### Configure API Key

Edit `appsettings.json`:

```json
{
  "Anthropic": {
    "ApiKey": "sk-ant-your-actual-key-here",
    "Model": "claude-sonnet-4-20250514"
  }
}
```

### Run the Bot

```bash
dotnet run --project src/MessagingBot
```

---

## 🎯 What It Looks Like

```
╔═══════════════════════════════════════════════════════════════════╗
║  📨 MESSAGINGBOT                                                ║
║  AI-Powered Outbound Messaging Engine                           ║
╚═══════════════════════════════════════════════════════════════════╝

Processing 5 records...

┌──────────────────────────────────────────────────────────────┐
│ 📝 Record #1 — PROP-1001 (Emily Chen)                     │
│ 📅 Move-in: 2026-08-01 (45 days)                          │
├──────────────────────────────────────────────────────────────┤
│ 📱 Channel: SMS                                           │
│ ⏰ Timing: 2026-06-18T14:30:00Z                           │
│ 📧 Subject: "Your move to The Parker is almost here!"     │
├──────────────────────────────────────────────────────────────┤
│ Hey Emily! Just a quick check-in about your upcoming move │
│ to The Parker on August 1st. We're excited to welcome you!│
│ Reply if you have questions about parking or move-in day. │
│                                                           │
│ 🟢 View lease documents → https://theparker.com/portal    │
├──────────────────────────────────────────────────────────────┤
│ 🧠 Reasoning:                                             │
│   • Channel: SMS opt-in present, prefers text             │
│   • Timing: 45 days out, nurture pace                     │
│   • Compliance: TCPA consent verified                     │
├──────────────────────────────────────────────────────────────┤
│ ✅ consent_verified       │ ✅ fair_housing_check_passed  │
│ ✅ brand_style_applied    │ ✅ no_pii_leak               │
│ ✅ include_opt_out        │                               │
├──────────────────────────────────────────────────────────────┤
│ 📊 Semantic Match: 92% ████████████████████████████████  │
│ 🎯 Quality Score: 9.2/10                                 │
└──────────────────────────────────────────────────────────────┘

══════════════════════════════════════════════════════════════
📊 SUMMARY: 5/5 records successful
💡 Avg Semantic Match: 94% ████████████████████████████████
══════════════════════════════════════════════════════════════
```

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Program.cs                              │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────────────┐  │
│  │ Load JSONL  │──▶│ Agent Loop  │──▶│ Render Results      │  │
│  │ (streaming) │   │ (per record)│   │ (Spectre.Console)   │  │
│  └─────────────┘   └──────┬──────┘   └─────────────────────┘  │
│                           │                                     │
└───────────────────────────┼─────────────────────────────────────┘
                            ▼
                  ┌─────────────────────┐
                  │  MessagingAgent.cs   │
                  │  ┌───────────────┐   │
                  │  │ System Prompt │   │
                  │  │ (core logic)  │   │
                  │  └───────┬───────┘   │
                  │          ▼           │
                  │  ┌───────────────┐   │
                  │  │  Anthropic    │   │
                  │  │  Messages API │   │
                  │  └───────┬───────┘   │
                  │          ▼           │
                  │  ┌───────────────┐   │
                  │  │ AgentResult   │   │
                  │  │ (structured)  │   │
                  │  └───────────────┘   │
                  └─────────────────────┘
```

---

## 📁 Project Structure

```
MessagingBot/
├── src/
│   └── MessagingBot/
│       ├── Program.cs              # Entry point, orchestration, rendering
│       ├── MessagingAgent.cs       # AI call layer with system prompt
│       ├── Models.cs               # All data contracts (records)
│       └── appsettings.json        # Configuration (API key, model)
├── data/
│   └── sample.jsonl                # Input records (one JSON per line)
├── tests/
│   └── MessagingBot.Tests/         # Unit tests (optional)
├── MessagingBot.sln
└── README.md
```

---

## 📄 Input Format (JSONL)

Each line in `sample.jsonl` represents one prospect or resident:

```json
{
  "task_id": "PROP-1001",
  "prospect_or_resident": "prospect",
  "profile": {
    "first_name": "Emily",
    "last_name": "Chen",
    "city_interest": "San Francisco",
    "neighborhood_interest": "SoMa",
    "move_date": "2026-08-01",
    "unit_type": "1BR",
    "budget": 3500,
    "amenities_of_interest": ["gym", "rooftop", "parking"],
    "lead_source": "apartments.com"
  },
  "consent": {
    "sms_opt_in": true,
    "email_opt_in": true,
    "voice_opt_in": false,
    "opted_out_at": null
  },
  "channel_preferences": ["sms", "email"],
  "lifecycle_stage": "active_lead",
  "context": {
    "last_contact": "2026-06-01T10:30:00Z",
    "previously_sent": ["Welcome email", "Virtual tour link"]
  },
  "expected": {
    "message": "Hi Emily! Excited to have you joining The Parker...",
    "personalization_score_min": 0.85
  },
  "thresholds": {
    "urgent_if_move_within_days": 14,
    "nurture_if_move_beyond_days": 60
  }
}
```

---

## 🧠 How It Works

### 1. The AI Reads Each Record

Every record is serialized to JSON and sent to Claude with a system prompt that tells it:
- You are a real-estate messaging specialist
- Infer rules from the data (no hardcoded logic)
- Return a specific JSON structure

### 2. The System Prompt Is the "Brain"

The system prompt (in `MessagingAgent.cs`) contains the complete rules of engagement:

```
RULES — infer dynamically from the data:
- Channel selection: based on consent + preferences
- Timing: based on move date (urgent vs nurture)
- Content: personalized with amenities and context
- Compliance: fair housing, TCPA, PII protection
```

**Why this approach?** Business rules change constantly. Instead of rewriting `if/else` chains, you edit the prompt. No deployment required.

### 3. Claude Returns Structured Output

```json
{
  "channel": "sms",
  "timing": "2026-06-18T14:30:00Z",
  "content": {
    "subject": "Your move to The Parker is almost here!",
    "body": "Hey Emily! ...",
    "cta": {
      "type": "link",
      "url": "https://theparker.com/portal",
      "label": "View lease documents"
    }
  },
  "reasoning": {
    "channel_decision": "SMS opt-in present and listed as first preference",
    "timing_decision": "45 days out → nurture cadence",
    "content_decision": "Referenced amenities of interest and move date",
    "compliance_notes": "TCPA consent verified, opt-out instructions included"
  },
  "assertions_passed": {
    "consent_verified": true,
    "fair_housing_check_passed": true,
    "brand_style_applied": true,
    "no_pii_leak": true,
    "include_opt_out_instructions": true
  },
  "semantic_match_score": 0.92
}
```

### 4. The UI Makes It Human-Readable

Spectre.Console renders each record with:
- Color-coded channel badges (cyan=SMS, magenta=Email, yellow=Voice)
- Green/red assertion checkmarks
- Semantic match score as a progress bar
- Reasoning block for auditability

---

## 🔧 Configuration

### `appsettings.json`

```json
{
  "Anthropic": {
    "ApiKey": "sk-ant-your-key-here",
    "Model": "claude-sonnet-4-20250514"
  }
}
```

### Environment Variables (Alternative)

```bash
export ANTHROPIC_API_KEY="sk-ant-your-key-here"
export ANTHROPIC_MODEL="claude-sonnet-4-20250514"
```

---

## 📊 Understanding the Output

| Field | Purpose |
|-------|---------|
| **Channel** | SMS, Email, or Voice based on consent + preferences |
| **Timing** | ISO-8601 timestamp for when to send |
| **Subject** | Email subject line (or SMS preview) |
| **Body** | The actual message content |
| **CTA** | Call-to-action (schedule tour, view documents, etc.) |
| **Reasoning** | Why the AI made each decision (audit trail) |
| **Assertions** | Self-checked compliance flags |
| **Semantic Match** | 0.0–1.0 similarity to expected message |

### Color Thresholds

| Score | Color | Meaning |
|-------|-------|---------|
| ≥ 85% | 🟢 Green | Exceeds quality expectations |
| 65–84% | 🟡 Yellow | Acceptable, room for improvement |
| < 65% | 🔴 Red | Below spec, needs review |

---

## 🧪 Testing

```bash
dotnet test tests/MessagingBot.Tests
```

---

## 🚢 Deployment Options

### Local Console

```bash
dotnet run --project src/MessagingBot
```

### Containerized (Docker)

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY src/MessagingBot/bin/Release/net8.0/publish/ .
ENTRYPOINT ["dotnet", "MessagingBot.dll"]
```

```bash
docker build -t messagingbot .
docker run --rm -v $(pwd)/data:/data messagingbot
```

### CI/CD Pipeline (GitHub Actions)

```yaml
name: Run MessagingBot
on: [push]
jobs:
  run:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0'
      - run: dotnet run --project src/MessagingBot
        env:
          ANTHROPIC_API_KEY: ${{ secrets.ANTHROPIC_API_KEY }}
```

---

## 🎯 Why This Design?

| Decision | Reasoning |
|----------|-----------|
| **No database** | Evaluation tool — results displayed, not persisted |
| **No queue** | Sequential processing avoids rate limits |
| **No DI container** | Three-file tool doesn't need it |
| **JsonElement for flexible fields** | Record profiles are heterogeneous |
| **C# Records for models** | Immutable, value equality, concise |
| **Raw HttpClient** | No third-party Anthropic SDK dependency |
| **System prompt as "rules engine"** | Business rules change faster than code |
| **Reasoning block in output** | Audit trail and debugging |
| **Self-assertions** | Machine-readable compliance signals |

---

## 📚 Resources

- [Anthropic Messages API](https://docs.anthropic.com/en/api/messages)
- [JSONL Specification](https://jsonlines.org/)
- [Spectre.Console Documentation](https://spectreconsole.net/)
- [Fair Housing Act Guidelines](https://www.hud.gov/program_offices/fair_housing_equal_opp)

---

## 🤝 Contributing

1. Fork the repo
2. Create a branch (`git checkout -b feature/improvement`)
3. Commit changes (`git commit -m "Add improvement"`)
4. Push (`git push origin feature/improvement`)
5. Open a Pull Request

---

## 📄 License

MIT © 2026 [Your Name]

---

## ⭐ Show Your Support

If MessagingBot saved you time or inspired your AI workflow, give it a ⭐ on GitHub!

---

> *"The best rules engine is one that infers its own rules from the data."*

---

**Questions?** Open an [issue](https://github.com/yourusername/messagingbot/issues) or start a [discussion](https://github.com/yourusername/messagingbot/discussions).