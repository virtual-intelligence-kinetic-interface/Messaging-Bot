using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MessagingBot;

public class MessagingAgent(string apiKey, string model)
{
    private static readonly HttpClient Http = new();

    private const string SystemPrompt = """
        You are an autonomous messaging agent for a real-estate leasing platform.

        You receive a JSON record describing a prospect or resident. Your job is to:
        1. Determine IF a message should be sent
        2. Determine WHICH channel to use (sms, email, voice)
        3. Determine WHEN to send it (ISO 8601 timestamp, localised to the user's timezone)
        4. Compose the message body (personalised, concise, compliant)
        5. Define the CTA and next action

        RULES — infer dynamically from the data (no hardcoded if/else logic):
        - Consent governs channels: only use channels where opt_in is true
        - Channel preferences signal priority when multiple channels are available
        - Move-date horizon affects urgency: short horizon = urgency; long horizon = nurture
        - Amenity interests, city_interest, and property features should appear in the message
        - All messages MUST include opt-out instructions (STOP or unsubscribe link)
        - No PII leakage, no discriminatory language, follow fair housing standards
        - Primary CTA should align with lifecycle stage and the constraints field
        - Timing must respect the user's timezone (send during business hours ~9–10 am local)
        - SMS: short, conversational, reply-options format; subject must be null
        - Email: slightly longer, subject line required

        ALWAYS respond with a JSON object in EXACTLY this shape (no markdown, no prose):
        {
          "reasoning": {
            "channel_decision": "...",
            "timing_decision": "...",
            "content_decision": "...",
            "compliance_notes": "..."
          },
          "output": {
            "next_message": {
              "channel": "sms|email|voice|none",
              "send_at": "<ISO 8601>",
              "subject": "<string or null>",
              "body": "<message body>",
              "cta": { "type": "<cta_type>", "options": ["..."] }
            },
            "next_action": {
              "type": "<action_type>",
              "name": "<cadence name or null>",
              "value": <days as int, or null>
            }
          },
          "assertions_passed": {
            "consent_verified": true,
            "fair_housing_check_passed": true,
            "brand_style_applied": true,
            "no_pii_leak": true,
            "include_opt_out_instructions": true
          },
          "semantic_match_score": <0.0–1.0>
        }
        """;

    public async Task<AgentResult> ProcessAsync(InputRecord record, CancellationToken ct = default)
    {
        var userMessage = $"""
            Process this record and decide what message to send:

            {JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true })}
            """;

        var requestBody = JsonSerializer.Serialize(new
        {
            model = model,
            max_tokens = 1024,
            system = SystemPrompt,
            messages = new[] { new { role = "user", content = userMessage } }
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        using var response = await Http.SendAsync(request, ct);       
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        // Extract the text block from the response
        var raw = doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? throw new InvalidOperationException("Empty response from API.");

        // Strip accidental markdown fences
        var clean = raw
            .Replace("```json", "").Replace("```", "")
            .Trim();

        return JsonSerializer.Deserialize<AgentResult>(clean,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Failed to deserialise agent result.");
    }
}
