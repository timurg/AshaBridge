using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using AshaBridge.Extensions.Moodle.Contracts;
using AshaBridge.Extensions.Moodle.Handlers;
using AshaBridge.Sdk.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AshaBridge.Extensions.Moodle;

public sealed class MoodleExtension : IAshaBridgeExtension
{
    public string Id => "ashabridge.extensions.moodle";

    public string Version => "1.0.0";

    public void Configure(IAshaBridgeExtensionBuilder builder)
    {
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
        builder.AddMethod<MoodleGetUsersByFieldRequest, MoodleGetUsersByFieldResponse, MoodleGetUsersByFieldHandler>();
        builder.AddMethod<MoodleCreateUserRequest, MoodleCreateUserResponse, MoodleCreateUserHandler>();
        builder.AddMethod<MoodleUpdateUserRequest, MoodleUpdateUserResponse, MoodleUpdateUserHandler>();
        builder.AddMethod<MoodleGetUserRequest, MoodleGetUserResponse, MoodleGetUserHandler>();
        builder.AddMethod<MoodleRequestPasswordResetRequest, MoodleRawResponse, MoodleRequestPasswordResetHandler>();
        builder.AddMethod<MoodleGetUsersCoursesRequest, MoodleGetUsersCoursesResponse, MoodleGetUsersCoursesHandler>();
        builder.AddMethod<MoodleManualEnrolUserRequest, MoodleRawResponse, MoodleManualEnrolUserHandler>();
        builder.AddMethod<MoodleGetActivitiesCompletionStatusRequest, MoodleGetActivitiesCompletionStatusResponse, MoodleGetActivitiesCompletionStatusHandler>();
        builder.AddMethod<MoodleGetCourseCompletionStatusRequest, MoodleRawResponse, MoodleGetCourseCompletionStatusHandler>();
        builder.AddMethod<MoodleListUserPlansRequest, MoodleRawResponse, MoodleListUserPlansHandler>();
        builder.AddMethod<MoodleGetGradeItemsRequest, MoodleGetGradeItemsResponse, MoodleGetGradeItemsHandler>();
        builder.AddMethod<MoodleGetCoursesRequest, MoodleRawResponse, MoodleGetCoursesHandler>();
        builder.AddMethod<MoodleGetCoursesByFieldRequest, MoodleRawResponse, MoodleGetCoursesByFieldHandler>();
        builder.AddMethod<MoodleGetCourseContentsRequest, MoodleRawResponse, MoodleGetCourseContentsHandler>();
        builder.AddMethod<MoodleFindUserByEmailRequest, MoodleFindUserResponse, MoodleFindUserByEmailHandler>();
        builder.AddMethod<MoodleFindUserByIdRequest, MoodleFindUserResponse, MoodleFindUserByIdHandler>();
        builder.AddMethod<MoodleFindUserByUsernameRequest, MoodleFindUserResponse, MoodleFindUserByUsernameHandler>();
        builder.AddMethod<MoodleUserUpdateNameRequest, MoodleUpdateUserResponse, MoodleUserUpdateNameHandler>();
        builder.AddMethod<MoodleUserUpdateEmailRequest, MoodleUpdateUserResponse, MoodleUserUpdateEmailHandler>();
        builder.AddMethod<MoodleUserUpdateUsernameRequest, MoodleUpdateUserResponse, MoodleUserUpdateUsernameHandler>();
        builder.AddMethod<MoodleUserUpdatePasswordRequest, MoodleUpdateUserResponse, MoodleUserUpdatePasswordHandler>();
        builder.AddMethod<MoodleUserSuspendRequest, MoodleUpdateUserResponse, MoodleUserSuspendHandler>();
        builder.AddMethod<MoodleUserUnsuspendRequest, MoodleUpdateUserResponse, MoodleUserUnsuspendHandler>();
        builder.AddMethod<MoodleRequestPasswordResetByEmailRequest, MoodleRawResponse, MoodleRequestPasswordResetByEmailHandler>();
        builder.AddMethod<MoodleRequestPasswordResetByUsernameRequest, MoodleRawResponse, MoodleRequestPasswordResetByUsernameHandler>();
        builder.AddMethod<MoodleUserEnrolRequest, MoodleRawResponse, MoodleUserEnrolHandler>();
        builder.AddMethod<MoodleUserEnrolAsStudentRequest, MoodleRawResponse, MoodleUserEnrolAsStudentHandler>();
        builder.AddMethod<MoodleCourseGetByIdRequest, MoodleRawResponse, MoodleCourseGetByIdHandler>();
        builder.AddMethod<MoodleCourseFindByShortNameRequest, MoodleRawResponse, MoodleCourseFindByShortNameHandler>();
        builder.AddMethod<MoodleCourseFindByIdNumberRequest, MoodleRawResponse, MoodleCourseFindByIdNumberHandler>();
        builder.AddMethod<MoodleCoursesFindByCategoryRequest, MoodleRawResponse, MoodleCoursesFindByCategoryHandler>();
        builder.AddMethod<MoodleCourseGetContentsRequest, MoodleRawResponse, MoodleCourseGetContentsHandler>();
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

public sealed class MoodleWebServiceClient(HttpClient http, IOptions<MoodleExtensionOptions> options)
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

        using var response = await http.PostAsync("webservice/rest/server.php", new FormUrlEncodedContent(form), ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var text = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        var node = JsonNode.Parse(text);
        if (node is JsonObject obj && obj["exception"] is not null)
        {
            throw new InvalidOperationException(text);
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
}
