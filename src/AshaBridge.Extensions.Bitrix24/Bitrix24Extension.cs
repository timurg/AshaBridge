using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using AshaBridge.Extensions.Bitrix24.Contracts;
using AshaBridge.Extensions.Bitrix24.Handlers;
using AshaBridge.Sdk.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AshaBridge.Extensions.Bitrix24;

public sealed class Bitrix24Extension : IAshaBridgeExtension
{
    public string Id => "ashabridge.extensions.bitrix24";

    public string Version => "1.0.0";

    public void Configure(IAshaBridgeExtensionBuilder builder)
    {
        builder.Services.AddHttpClient<BitrixRestClient>((services, http) =>
        {
            var options = services.GetRequiredService<IOptions<BitrixExtensionOptions>>().Value;
            var instance = options.Instances.GetValueOrDefault(options.DefaultInstance);
            if (instance is null)
            {
                return;
            }

            http.Timeout = TimeSpan.FromSeconds(instance.TimeoutSeconds);
            http.BaseAddress = new Uri(EnsureTrailingSlash(instance.WebhookUrl ?? instance.BaseUrl));
        });
        builder.AddMethod<BitrixCrmItemGetRequest, BitrixCrmItemGetResponse, BitrixCrmItemGetHandler>();
        builder.AddMethod<BitrixCrmItemListRequest, BitrixCrmItemListResponse, BitrixCrmItemListHandler>();
        builder.AddMethod<BitrixCrmDynamicItemsListAllRequest, BitrixCrmItemListResponse, BitrixCrmDynamicItemsListAllHandler>();
        builder.AddMethod<BitrixCrmItemUpdateRequest, BitrixCrmItemUpdateResponse, BitrixCrmItemUpdateHandler>();
        builder.AddMethod<BitrixCrmDealGetRequest, BitrixCrmDealGetResponse, BitrixCrmDealGetHandler>();
        builder.AddMethod<BitrixCrmDealListRequest, BitrixCrmDealListResponse, BitrixCrmDealListHandler>();
        builder.AddMethod<BitrixCrmDealsListAllRequest, BitrixCrmDealListResponse, BitrixCrmDealsListAllHandler>();
        builder.AddMethod<BitrixCrmDealsFindByContactIdRequest, BitrixCrmDealListResponse, BitrixCrmDealsFindByContactIdHandler>();
        builder.AddMethod<BitrixCrmContactGetRequest, BitrixCrmContactGetResponse, BitrixCrmContactGetHandler>();
        builder.AddMethod<BitrixCrmContactListRequest, BitrixCrmContactListResponse, BitrixCrmContactListHandler>();
        builder.AddMethod<BitrixCrmContactsListAllRequest, BitrixCrmContactListResponse, BitrixCrmContactsListAllHandler>();
        builder.AddMethod<BitrixCrmContactsFindByEmailRequest, BitrixCrmContactListResponse, BitrixCrmContactsFindByEmailHandler>();
        builder.AddMethod<BitrixCrmContactUpdateRequest, BitrixCrmContactUpdateResponse, BitrixCrmContactUpdateHandler>();
        builder.AddMethod<BitrixCrmContactUpdateNameRequest, BitrixCrmContactUpdateResponse, BitrixCrmContactUpdateNameHandler>();
        builder.AddMethod<BitrixCrmContactUpdateEmailRequest, BitrixCrmContactUpdateResponse, BitrixCrmContactUpdateEmailHandler>();
        builder.AddMethod<BitrixCrmDealTrainingDirectionUpdateRequest, BitrixCrmDealTrainingDirectionUpdateResponse, BitrixCrmDealTrainingDirectionUpdateHandler>();
        builder.AddMethod<BitrixCrmDealPartyEmailAddRequest, BitrixCrmDealPartyEmailAddResponse, BitrixCrmDealPartyEmailAddHandler>();
        builder.AddMethod<BitrixCrmTimelineCommentAddRequest, BitrixCrmTimelineCommentAddResponse, BitrixCrmTimelineCommentAddHandler>();
    }

    private static string EnsureTrailingSlash(string value) =>
        value.EndsWith("/", StringComparison.Ordinal) ? value : $"{value}/";
}

public sealed class BitrixExtensionOptions
{
    [Required]
    public string DefaultInstance { get; set; } = "office";

    public Dictionary<string, BitrixInstanceOptions> Instances { get; set; } = [];
}

public sealed class BitrixInstanceOptions
{
    [Required]
    public string BaseUrl { get; set; } = "https://example.bitrix24.com";

    public string AuthMode { get; set; } = "webhook";

    public string? WebhookUrl { get; set; }

    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 20;
}

public sealed class BitrixRestClient(
    HttpClient http,
    ILogger<BitrixRestClient> logger)
{
    public async Task<JsonObject> CallAsync(string method, JsonObject payload, CancellationToken ct)
    {
        if (http.BaseAddress is null)
        {
            return new JsonObject
            {
                ["method"] = method,
                ["payload"] = payload.DeepClone()
            };
        }

        using var response = await PostAsync(method, payload, ct).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "Bitrix24 REST call {Method} failed with HTTP {StatusCode} {ReasonPhrase}. Payload={Payload}; Response={Response}",
                method,
                (int)response.StatusCode,
                response.ReasonPhrase,
                RedactPayload(payload),
                Truncate(body));

            var statusCode = (int)response.StatusCode;
            throw new ExternalServiceException(
                "Bitrix24",
                method,
                ExternalServiceErrorKind.Http,
                $"Bitrix24 returned HTTP {statusCode} {response.ReasonPhrase} for '{method}'.",
                retryable: statusCode is 408 or 429 || statusCode >= 500,
                statusCode: statusCode);
        }

        JsonObject result;
        try
        {
            result = JsonNode.Parse(body) as JsonObject ?? [];
        }
        catch (JsonException ex)
        {
            logger.LogError(
                ex,
                "Bitrix24 REST call {Method} returned invalid JSON. Payload={Payload}; Response={Response}",
                method,
                RedactPayload(payload),
                Truncate(body));

            throw new ExternalServiceException(
                "Bitrix24",
                method,
                ExternalServiceErrorKind.InvalidResponse,
                $"Bitrix24 returned invalid JSON for '{method}'.",
                innerException: ex);
        }

        if (result["error"] is not null)
        {
            logger.LogError(
                "Bitrix24 REST call {Method} returned an error. Payload={Payload}; Response={Response}",
                method,
                RedactPayload(payload),
                Truncate(body));

            var errorCode = result["error"]?.GetValue<string>();
            var errorMessage = result["error_description"]?.GetValue<string>();
            var details = string.Join(": ", new[] { errorCode, errorMessage }.Where(value => !string.IsNullOrWhiteSpace(value)));
            throw new ExternalServiceException(
                "Bitrix24",
                method,
                ExternalServiceErrorKind.Api,
                string.IsNullOrWhiteSpace(details) ? $"Bitrix24 rejected '{method}'." : details);
        }

        return result;
    }

    private async Task<HttpResponseMessage> PostAsync(string method, JsonObject payload, CancellationToken ct)
    {
        try
        {
            return await http.PostAsJsonAsync(method, payload, ct).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "Bitrix24 REST transport failed. Method={Method}; Payload={Payload}",
                method,
                RedactPayload(payload));

            throw new ExternalServiceException(
                "Bitrix24",
                method,
                ExternalServiceErrorKind.Transport,
                $"Could not reach Bitrix24 for '{method}': {ex.Message}",
                retryable: true,
                innerException: ex);
        }
    }

    private static string RedactPayload(JsonObject payload)
    {
        var clone = payload.DeepClone();
        RedactNode(clone);
        return Truncate(clone.ToJsonString());
    }

    private static void RedactNode(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var key in obj.Select(property => property.Key).ToArray())
                {
                    if (IsSensitiveKey(key))
                    {
                        obj[key] = "<redacted>";
                        continue;
                    }

                    RedactNode(obj[key]);
                }

                break;
            case JsonArray array:
                foreach (var item in array)
                {
                    RedactNode(item);
                }

                break;
        }
    }

    private static bool IsSensitiveKey(string key) =>
        key.Contains("password", StringComparison.OrdinalIgnoreCase)
        || key.Contains("token", StringComparison.OrdinalIgnoreCase)
        || key.Contains("body", StringComparison.OrdinalIgnoreCase)
        || key.Contains("comment", StringComparison.OrdinalIgnoreCase);

    private static string Truncate(string value) =>
        value.Length <= 4000 ? value : string.Concat(value.AsSpan(0, 4000), "...");
}
