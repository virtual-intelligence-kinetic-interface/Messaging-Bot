using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessagingBot;

// ─── Config Models ────────────────────────────────────────────────────────────
public record Config(
    [property: JsonPropertyName("anthropic")] AnthropicConfig Anthropic
);

public record AnthropicConfig(
    [property: JsonPropertyName("api_key")] string ApiKey,
    [property: JsonPropertyName("model")] string Model
);

// ─── Input Models ────────────────────────────────────────────────────────────

public record InputRecord(
    [property: JsonPropertyName("task_id")] string TaskId,
    [property: JsonPropertyName("persona")] string Persona,
    [property: JsonPropertyName("lifecycle_stage")] string LifecycleStage,
    [property: JsonPropertyName("consent")] Consent Consent,
    [property: JsonPropertyName("channel_preferences")] List<string> ChannelPreferences,
    [property: JsonPropertyName("input")] InputData Input,
    [property: JsonPropertyName("assertions")] Assertions? Assertions,
    [property: JsonPropertyName("thresholds")] JsonElement? Thresholds,
    [property: JsonPropertyName("expected")] JsonElement? Expected
);

public record Consent(
    [property: JsonPropertyName("email_opt_in")] bool EmailOptIn,
    [property: JsonPropertyName("sms_opt_in")] bool SmsOptIn,
    [property: JsonPropertyName("voice_opt_in")] bool VoiceOptIn
);

public record InputData(
    [property: JsonPropertyName("property_name")] string PropertyName,
    [property: JsonPropertyName("move_date_target")] string? MoveDateTarget,
    [property: JsonPropertyName("last_interaction")] string? LastInteraction,
    [property: JsonPropertyName("timezone")] string Timezone,
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("profile")] JsonElement Profile     // flexible — fields vary per record
);

public record Assertions(
    [property: JsonPropertyName("required_states")] List<string>? RequiredStates,
    [property: JsonPropertyName("constraints")] JsonElement? Constraints
);

// ─── Agent Output Models ──────────────────────────────────────────────────────

public record AgentResult(
    [property: JsonPropertyName("reasoning")] Reasoning Reasoning,
    [property: JsonPropertyName("output")] AgentOutput Output,
    [property: JsonPropertyName("assertions_passed")] Dictionary<string, bool> AssertionsPassed,
    [property: JsonPropertyName("semantic_match_score")] double SemanticMatchScore
);

public record Reasoning(
    [property: JsonPropertyName("channel_decision")] string ChannelDecision,
    [property: JsonPropertyName("timing_decision")] string TimingDecision,
    [property: JsonPropertyName("content_decision")] string ContentDecision,
    [property: JsonPropertyName("compliance_notes")] string ComplianceNotes
);

public record AgentOutput(
    [property: JsonPropertyName("next_message")] NextMessage NextMessage,
    [property: JsonPropertyName("next_action")] NextAction NextAction
);

public record NextMessage(
    [property: JsonPropertyName("channel")] string Channel,
    [property: JsonPropertyName("send_at")] string SendAt,
    [property: JsonPropertyName("subject")] string? Subject,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("cta")] JsonElement? Cta
);

public record NextAction(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("value")] JsonElement? Value
);
