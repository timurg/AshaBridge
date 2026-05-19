using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using AshaBridge.Extensions.Bitrix24.Contracts;
using AshaBridge.Extensions.Bitrix24.Handlers;
using AshaBridge.Sdk.Contracts;
using Microsoft.Extensions.DependencyInjection;
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
        builder.AddMethod<BitrixCrmItemUpdateRequest, BitrixCrmItemUpdateResponse, BitrixCrmItemUpdateHandler>();
        builder.AddMethod<BitrixCrmDealGetRequest, BitrixCrmDealGetResponse, BitrixCrmDealGetHandler>();
        builder.AddMethod<BitrixCrmDealListRequest, BitrixCrmDealListResponse, BitrixCrmDealListHandler>();
        builder.AddMethod<BitrixCrmContactGetRequest, BitrixCrmContactGetResponse, BitrixCrmContactGetHandler>();
        builder.AddMethod<BitrixCrmContactListRequest, BitrixCrmContactListResponse, BitrixCrmContactListHandler>();
        builder.AddMethod<BitrixCrmContactUpdateRequest, BitrixCrmContactUpdateResponse, BitrixCrmContactUpdateHandler>();
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

public sealed class BitrixRestClient(HttpClient http)
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

        using var response = await http.PostAsJsonAsync(method, payload, ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            throw new HttpRequestException($"Bitrix24 REST call '{method}' failed with {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken: ct).ConfigureAwait(false) ?? [];
    }

}
