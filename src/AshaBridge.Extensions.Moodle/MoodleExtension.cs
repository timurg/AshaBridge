using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using AshaBridge.Extensions.Moodle.Contracts;
using AshaBridge.Extensions.Moodle.Handlers;
using AshaBridge.Sdk.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AshaBridge.Extensions.Moodle;

public sealed class MoodleExtension : IAshaBridgeExtension
{
    public string Id => "ashabridge.extensions.moodle";

    public string Version => "1.0.0";

    public void Configure(IAshaBridgeExtensionBuilder builder)
    {
        builder.Services.AddHttpClient<MoodleWebServiceClient>();
        builder.AddMethod<MoodleGetUsersByFieldRequest, MoodleGetUsersByFieldResponse, MoodleGetUsersByFieldHandler>();
        builder.AddMethod<MoodleGetUsersCoursesRequest, MoodleGetUsersCoursesResponse, MoodleGetUsersCoursesHandler>();
        builder.AddMethod<MoodleGetActivitiesCompletionStatusRequest, MoodleGetActivitiesCompletionStatusResponse, MoodleGetActivitiesCompletionStatusHandler>();
        builder.AddMethod<MoodleGetGradeItemsRequest, MoodleGetGradeItemsResponse, MoodleGetGradeItemsHandler>();
    }
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

public sealed class MoodleWebServiceClient(HttpClient http)
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

        using var response = await http.PostAsJsonAsync("webservice/rest/server.php", payload, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: ct).ConfigureAwait(false) ?? [];
    }
}
