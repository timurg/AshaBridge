using System.Text.Json.Nodes;
using AshaBridge.Extensions.Bitrix24.Contracts;
using AshaBridge.Sdk.Contracts;

namespace AshaBridge.Extensions.Bitrix24.Handlers;

public sealed class BitrixCrmItemGetHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmItemGetRequest, BitrixCrmItemGetResponse>
{
    public async Task<BitrixCrmItemGetResponse> HandleAsync(BitrixCrmItemGetRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("crm.item.get", new JsonObject { ["entityTypeId"] = request.EntityTypeId, ["id"] = request.Id }, ct);
        return new BitrixCrmItemGetResponse((result["result"]?["item"] as JsonObject) ?? result);
    }
}

public sealed class BitrixCrmItemListHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmItemListRequest, BitrixCrmItemListResponse>
{
    public async Task<BitrixCrmItemListResponse> HandleAsync(BitrixCrmItemListRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("crm.item.list", new JsonObject { ["entityTypeId"] = request.EntityTypeId, ["filter"] = request.Filter?.DeepClone() }, ct);
        return new BitrixCrmItemListResponse((result["result"]?["items"] as JsonArray) ?? []);
    }
}

public sealed class BitrixCrmItemUpdateHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmItemUpdateRequest, BitrixCrmItemUpdateResponse>
{
    public async Task<BitrixCrmItemUpdateResponse> HandleAsync(BitrixCrmItemUpdateRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        await client.CallAsync("crm.item.update", new JsonObject { ["entityTypeId"] = request.EntityTypeId, ["id"] = request.Id, ["fields"] = request.Fields.DeepClone() }, ct);
        return new BitrixCrmItemUpdateResponse(true);
    }
}

public sealed class BitrixCrmDealGetHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmDealGetRequest, BitrixCrmDealGetResponse>
{
    public async Task<BitrixCrmDealGetResponse> HandleAsync(BitrixCrmDealGetRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        new(((await client.CallAsync("crm.deal.get", new JsonObject { ["id"] = request.Id }, ct))["result"] as JsonObject) ?? []);
}

public sealed class BitrixCrmDealListHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmDealListRequest, BitrixCrmDealListResponse>
{
    public async Task<BitrixCrmDealListResponse> HandleAsync(BitrixCrmDealListRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("crm.deal.list", new JsonObject { ["filter"] = request.Filter?.DeepClone() }, ct);
        return new BitrixCrmDealListResponse((result["result"] as JsonArray) ?? []);
    }
}

public sealed class BitrixCrmContactGetHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmContactGetRequest, BitrixCrmContactGetResponse>
{
    public async Task<BitrixCrmContactGetResponse> HandleAsync(BitrixCrmContactGetRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        new(((await client.CallAsync("crm.contact.get", new JsonObject { ["id"] = request.Id }, ct))["result"] as JsonObject) ?? []);
}

public sealed class BitrixCrmContactListHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmContactListRequest, BitrixCrmContactListResponse>
{
    public async Task<BitrixCrmContactListResponse> HandleAsync(BitrixCrmContactListRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("crm.contact.list", new JsonObject { ["filter"] = request.Filter?.DeepClone() }, ct);
        return new BitrixCrmContactListResponse((result["result"] as JsonArray) ?? []);
    }
}

public sealed class BitrixCrmContactUpdateHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmContactUpdateRequest, BitrixCrmContactUpdateResponse>
{
    public async Task<BitrixCrmContactUpdateResponse> HandleAsync(BitrixCrmContactUpdateRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var fields = new JsonObject();
        AddIfProvided(fields, "NAME", request.Name);
        AddIfProvided(fields, "LAST_NAME", request.LastName);
        AddIfProvided(fields, "SECOND_NAME", request.MiddleName);

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            fields["EMAIL"] = new JsonArray
            {
                new JsonObject
                {
                    ["VALUE"] = request.Email.Trim(),
                    ["VALUE_TYPE"] = "WORK"
                }
            };
        }

        if (fields.Count == 0)
        {
            throw new ArgumentException("At least one contact field must be provided.");
        }

        var result = await client.CallAsync("crm.contact.update", new JsonObject
        {
            ["id"] = request.Id,
            ["fields"] = fields
        }, ct);

        var success = GetBooleanResult(result, defaultValue: true);
        return new BitrixCrmContactUpdateResponse(
            success,
            success ? "Contact updated successfully." : "Failed to update contact.");
    }

    private static void AddIfProvided(JsonObject fields, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            fields[name] = value.Trim();
        }
    }

    private static bool GetBooleanResult(JsonObject result, bool defaultValue) =>
        result["result"]?.GetValue<bool?>() ?? defaultValue;
}

public sealed class BitrixCrmDealTrainingDirectionUpdateHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmDealTrainingDirectionUpdateRequest, BitrixCrmDealTrainingDirectionUpdateResponse>
{
    private const string TrainingDirectionField = "UF_CRM_6283BEE95507A";

    public async Task<BitrixCrmDealTrainingDirectionUpdateResponse> HandleAsync(BitrixCrmDealTrainingDirectionUpdateRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Direction))
        {
            throw new ArgumentException("Training direction must not be empty.");
        }

        var result = await client.CallAsync("crm.deal.update", new JsonObject
        {
            ["id"] = request.Id,
            ["fields"] = new JsonObject
            {
                [TrainingDirectionField] = request.Direction.Trim()
            }
        }, ct);

        var success = result["result"]?.GetValue<bool?>() ?? true;
        return new BitrixCrmDealTrainingDirectionUpdateResponse(
            success,
            success ? "Training direction updated successfully." : "Failed to update training direction.");
    }
}

public sealed class BitrixCrmDealPartyEmailAddHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmDealPartyEmailAddRequest, BitrixCrmDealPartyEmailAddResponse>
{
    private const int DealEntityTypeId = 2;
    private const int ContactEntityTypeId = 3;
    private const int EmailActivityTypeId = 4;
    private const int OutgoingDirection = 2;
    private const int TextDescriptionType = 1;
    private const int HtmlDescriptionType = 2;

    public async Task<BitrixCrmDealPartyEmailAddResponse> HandleAsync(BitrixCrmDealPartyEmailAddRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            throw new ArgumentException("Email subject must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            throw new ArgumentException("Email body must not be empty.");
        }

        var deal = ((await client.CallAsync("crm.deal.get", new JsonObject { ["id"] = request.DealId }, ct))["result"] as JsonObject)
            ?? throw new InvalidOperationException("Deal was not found.");

        var (email, contactId) = await ResolveRecipientAsync(request.Recipient, deal, ct).ConfigureAwait(false);

        var activity = new JsonObject
        {
            ["fields"] = new JsonObject
            {
                ["OWNER_TYPE_ID"] = DealEntityTypeId,
                ["OWNER_ID"] = request.DealId,
                ["TYPE_ID"] = EmailActivityTypeId,
                ["PROVIDER_ID"] = "CRM_EMAIL",
                ["PROVIDER_TYPE_ID"] = "EMAIL",
                ["SUBJECT"] = request.Subject.Trim(),
                ["DESCRIPTION"] = request.Body,
                ["DESCRIPTION_TYPE"] = request.IsHtml ? HtmlDescriptionType : TextDescriptionType,
                ["DIRECTION"] = OutgoingDirection,
                ["COMPLETED"] = "Y",
                ["SETTINGS"] = BuildEmailSettings(request.DisableCopyToSelf),
                ["COMMUNICATIONS"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["TYPE"] = "EMAIL",
                        ["VALUE"] = email,
                        ["ENTITY_TYPE_ID"] = contactId.HasValue ? ContactEntityTypeId : null,
                        ["ENTITY_ID"] = contactId
                    }
                }
            }
        };

        var activityId = ((await client.CallAsync("crm.activity.add", activity, ct))["result"]?.GetValue<long?>()) ?? 0;
        return new BitrixCrmDealPartyEmailAddResponse(true, activityId, request.Recipient);
    }

    private async Task<(string Email, long? ContactId)> ResolveRecipientAsync(string recipient, JsonObject deal, CancellationToken ct)
    {
        return NormalizeRecipient(recipient) switch
        {
            "student" => await ResolveStudentAsync(deal, ct).ConfigureAwait(false),
            "student_curator" => await ResolveStudentCuratorAsync(deal, ct).ConfigureAwait(false),
            "manager" => ResolveManager(),
            _ => throw new ArgumentException("Recipient must be one of: student, student_curator, manager.")
        };
    }

    private async Task<(string Email, long? ContactId)> ResolveStudentAsync(JsonObject deal, CancellationToken ct)
    {
        var contactId = GetLong(deal, "CONTACT_ID") ?? throw new InvalidOperationException("Deal has no CONTACT_ID.");
        var contact = ((await client.CallAsync("crm.contact.get", new JsonObject { ["id"] = contactId }, ct))["result"] as JsonObject)
            ?? throw new InvalidOperationException("Deal contact was not found.");
        var email = GetFirstEmail(contact["EMAIL"] as JsonArray);
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Deal contact has no email.");
        }

        return (email, contactId);
    }

    private async Task<(string Email, long? ContactId)> ResolveStudentCuratorAsync(JsonObject deal, CancellationToken ct)
    {
        var assignedById = GetLong(deal, "ASSIGNED_BY_ID") ?? throw new InvalidOperationException("Deal has no ASSIGNED_BY_ID.");
        var users = (await client.CallAsync("user.get", new JsonObject
        {
            ["filter"] = new JsonObject { ["ID"] = assignedById }
        }, ct))["result"] as JsonArray;

        var user = users?.OfType<JsonObject>().FirstOrDefault();
        var email = user?["EMAIL"]?.GetValue<string>()?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Deal assignee has no email.");
        }

        return (email, null);
    }

    private static (string Email, long? ContactId) ResolveManager()
    {
        var email = Environment.GetEnvironmentVariable("MCP_MANAGER_EMAIL")?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("MCP_MANAGER_EMAIL is not set.");
        }

        return (email, null);
    }

    private static JsonObject BuildEmailSettings(bool disableCopyToSelf)
    {
        var settings = new JsonObject
        {
            ["DISABLE_COPY_TO_SELF"] = disableCopyToSelf ? "Y" : "N"
        };

        var fromEmail = Environment.GetEnvironmentVariable("MCP_FROM_EMAIL")?.Trim();
        if (!string.IsNullOrWhiteSpace(fromEmail))
        {
            var fromName = Environment.GetEnvironmentVariable("MCP_FROM_NAME")?.Trim();
            settings["MESSAGE_FROM"] = string.IsNullOrWhiteSpace(fromName)
                ? fromEmail
                : $"{fromName} <{fromEmail}>";
        }

        return settings;
    }

    private static string NormalizeRecipient(string recipient) =>
        recipient.Trim().Replace("-", "_", StringComparison.Ordinal).ToLowerInvariant();

    private static string? GetFirstEmail(JsonArray? emails) =>
        emails?.OfType<JsonObject>()
            .Select(email => email["VALUE"]?.GetValue<string>()?.Trim())
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private static long? GetLong(JsonObject source, string name)
    {
        var node = source[name];
        if (node is null)
        {
            return null;
        }

        if (node.GetValueKind() == System.Text.Json.JsonValueKind.Number && node.GetValue<long?>() is { } numeric)
        {
            return numeric;
        }

        var text = node.GetValue<string>();
        return long.TryParse(text, out var parsed) ? parsed : null;
    }
}

public sealed class BitrixCrmTimelineCommentAddHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmTimelineCommentAddRequest, BitrixCrmTimelineCommentAddResponse>
{
    public async Task<BitrixCrmTimelineCommentAddResponse> HandleAsync(BitrixCrmTimelineCommentAddRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("crm.timeline.comment.add", new JsonObject
        {
            ["fields"] = new JsonObject
            {
                ["ENTITY_TYPE"] = request.EntityType,
                ["ENTITY_ID"] = request.EntityId,
                ["COMMENT"] = request.Comment
            }
        }, ct);

        return new BitrixCrmTimelineCommentAddResponse(result["result"]?.GetValue<long>() ?? result["id"]?.GetValue<long>() ?? 0);
    }
}
