using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using AshaBridge.Extensions.Moodle.Contracts;
using AshaBridge.Extensions.Moodle.Handlers;
using AshaBridge.Sdk.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AshaBridge.Extensions.Moodle;

public sealed class MoodleExtension : IAshaBridgeExtension
{
    public string Id => "ashabridge.extensions.moodle";

    public string Version => "1.0.0";

    public void Configure(IAshaBridgeExtensionBuilder builder)
    {
        builder.Services
            .AddOptions<MoodleExtensionOptions>()
            .Bind(builder.Configuration.GetSection("moodle"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        builder.Services.AddHttpClient<MoodleWebServiceClient>((services, http) =>
        {
            var options = services.GetRequiredService<IOptions<MoodleExtensionOptions>>().Value;
            var instance = options.Instances.GetValueOrDefault(options.DefaultInstance);
            if (instance is null)
            {
                return;
            }

            http.Timeout = TimeSpan.FromSeconds(instance.TimeoutSeconds);
            http.BaseAddress = new Uri(EnsureTrailingSlash(instance.BaseUrl));
        });
        builder.AddToolMethod<MoodleCreateUserRequest, MoodleCreateUserResponse, MoodleCreateUserHandler>();
        builder.AddToolMethod<MoodleFindUserByEmailRequest, MoodleFindUserResponse, MoodleFindUserByEmailHandler>();
        builder.AddToolMethod<MoodleFindUserByIdRequest, MoodleFindUserResponse, MoodleFindUserByIdHandler>();
        builder.AddToolMethod<MoodleFindUserByUsernameRequest, MoodleFindUserResponse, MoodleFindUserByUsernameHandler>();
        builder.AddToolMethod<MoodleUserUpdateNameRequest, MoodleUpdateUserResponse, MoodleUserUpdateNameHandler>();
        builder.AddToolMethod<MoodleUserUpdateEmailRequest, MoodleUpdateUserResponse, MoodleUserUpdateEmailHandler>();
        builder.AddToolMethod<MoodleUserUpdateUsernameRequest, MoodleUpdateUserResponse, MoodleUserUpdateUsernameHandler>();
        builder.AddToolMethod<MoodleUserUpdatePasswordRequest, MoodleUpdateUserResponse, MoodleUserUpdatePasswordHandler>();
        builder.AddToolMethod<MoodleUserSuspendRequest, MoodleUpdateUserResponse, MoodleUserSuspendHandler>();
        builder.AddToolMethod<MoodleUserUnsuspendRequest, MoodleUpdateUserResponse, MoodleUserUnsuspendHandler>();
        builder.AddToolMethod<MoodleRequestPasswordResetByEmailRequest, MoodleRawResponse, MoodleRequestPasswordResetByEmailHandler>();
        builder.AddToolMethod<MoodleRequestPasswordResetByUsernameRequest, MoodleRawResponse, MoodleRequestPasswordResetByUsernameHandler>();
        builder.AddToolMethod<MoodleUserEnrolRequest, MoodleRawResponse, MoodleUserEnrolHandler>();
        builder.AddToolMethod<MoodleUserEnrolAsStudentRequest, MoodleRawResponse, MoodleUserEnrolAsStudentHandler>();
        builder.AddToolMethod<MoodleCourseGetByIdRequest, MoodleRawResponse, MoodleCourseGetByIdHandler>();
        builder.AddToolMethod<MoodleCourseFindByShortNameRequest, MoodleRawResponse, MoodleCourseFindByShortNameHandler>();
        builder.AddToolMethod<MoodleCourseFindByIdNumberRequest, MoodleRawResponse, MoodleCourseFindByIdNumberHandler>();
        builder.AddToolMethod<MoodleCoursesFindByCategoryRequest, MoodleRawResponse, MoodleCoursesFindByCategoryHandler>();
        builder.AddToolMethod<MoodleCourseGetContentsRequest, MoodleRawResponse, MoodleCourseGetContentsHandler>();
    }

    private static string EnsureTrailingSlash(string value) =>
        value.EndsWith("/", StringComparison.Ordinal) ? value : $"{value}/";
}

public sealed class MoodleExtensionOptions
{
    [Required]
    public string DefaultInstance { get; set; } = "main";

    public Dictionary<string, MoodleInstanceOptions> Instances { get; set; } = [];
}

public sealed class MoodleInstanceOptions
{
    [Required]
    public string BaseUrl { get; set; } = "https://example.edu";

    public string? Token { get; set; }

    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 20;
}

public sealed class MoodleWebServiceClient(
    HttpClient http,
    IOptions<MoodleExtensionOptions> options,
    ILogger<MoodleWebServiceClient> logger)
{
    public async Task<JsonObject> CallAsync(string function, JsonObject payload, CancellationToken ct)
    {
        if (http.BaseAddress is null)
        {
            return new JsonObject
            {
                ["function"] = function,
                ["payload"] = payload.DeepClone()
            };
        }

        var instance = options.Value.Instances.GetValueOrDefault(options.Value.DefaultInstance);
        if (string.IsNullOrWhiteSpace(instance?.Token))
        {
            throw new InvalidOperationException("Moodle token is not configured.");
        }

        var form = new Dictionary<string, string>
        {
            ["wstoken"] = instance.Token,
            ["wsfunction"] = function,
            ["moodlewsrestformat"] = "json"
        };

        AddFormFields(form, payload);

        using var content = new FormUrlEncodedContent(form);
        using var response = await PostAsync(function, payload, content, ct).ConfigureAwait(false);
        var text = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "Moodle REST call {Function} failed with HTTP {StatusCode} {ReasonPhrase}. Payload={Payload}; Response={Response}",
                function,
                (int)response.StatusCode,
                response.ReasonPhrase,
                RedactPayload(payload),
                Truncate(text));

            var statusCode = (int)response.StatusCode;
            throw new ExternalServiceException(
                "Moodle",
                function,
                ExternalServiceErrorKind.Http,
                $"Moodle returned HTTP {statusCode} {response.ReasonPhrase} for '{function}'.",
                retryable: statusCode is 408 or 429 || statusCode >= 500,
                statusCode: statusCode);
        }

        JsonNode? node;
        try
        {
            node = JsonNode.Parse(text);
        }
        catch (JsonException ex)
        {
            logger.LogError(
                ex,
                "Moodle REST call {Function} returned invalid JSON. Payload={Payload}; Response={Response}",
                function,
                RedactPayload(payload),
                Truncate(text));

            throw new ExternalServiceException(
                "Moodle",
                function,
                ExternalServiceErrorKind.InvalidResponse,
                $"Moodle returned invalid JSON for '{function}'.",
                innerException: ex);
        }

        if (node is JsonObject obj && obj["exception"] is not null)
        {
            logger.LogError(
                "Moodle REST call {Function} returned an exception. Payload={Payload}; Response={Response}",
                function,
                RedactPayload(payload),
                Truncate(text));

            var errorCode = obj["errorcode"]?.GetValue<string>();
            var errorMessage = obj["message"]?.GetValue<string>();
            var details = string.Join(": ", new[] { errorCode, errorMessage }.Where(value => !string.IsNullOrWhiteSpace(value)));
            throw new ExternalServiceException(
                "Moodle",
                function,
                ExternalServiceErrorKind.Api,
                string.IsNullOrWhiteSpace(details) ? $"Moodle rejected '{function}'." : details);
        }

        return node switch
        {
            JsonObject objectResult => objectResult,
            JsonArray array => new JsonObject { ["items"] = array },
            _ => new JsonObject()
        };
    }

    private static void AddFormFields(Dictionary<string, string> form, JsonObject payload)
    {
        foreach (var (key, value) in payload)
        {
            AddFormField(form, key, value);
        }
    }

    private async Task<HttpResponseMessage> PostAsync(
        string function,
        JsonObject payload,
        HttpContent content,
        CancellationToken ct)
    {
        try
        {
            return await http.PostAsync("webservice/rest/server.php", content, ct).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "Moodle REST transport failed. Function={Function}; Payload={Payload}",
                function,
                RedactPayload(payload));

            throw new ExternalServiceException(
                "Moodle",
                function,
                ExternalServiceErrorKind.Transport,
                $"Could not reach Moodle for '{function}': {ex.Message}",
                retryable: true,
                innerException: ex);
        }
    }

    private static void AddFormField(Dictionary<string, string> form, string key, JsonNode? value)
    {
        switch (value)
        {
            case null:
                return;
            case JsonValue jsonValue:
                form[key] = jsonValue.ToString();
                return;
            case JsonObject jsonObject:
                foreach (var (childKey, childValue) in jsonObject)
                {
                    AddFormField(form, $"{key}[{childKey}]", childValue);
                }

                return;
            case JsonArray jsonArray:
                for (var i = 0; i < jsonArray.Count; i++)
                {
                    AddFormField(form, $"{key}[{i}]", jsonArray[i]);
                }

                return;
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
