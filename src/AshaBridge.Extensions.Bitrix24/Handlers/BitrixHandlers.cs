using System.Text.Json.Nodes;
using AshaBridge.Extensions.Bitrix24.Contracts;
using AshaBridge.Sdk.Contracts;

namespace AshaBridge.Extensions.Bitrix24.Handlers;

public sealed class BitrixCrmItemGetHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmItemGetRequest, BitrixCrmItemGetResponse>
{
    public async Task<BitrixCrmItemGetResponse> HandleAsync(BitrixCrmItemGetRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("crm.item.get", new JsonObject { ["entityTypeId"] = request.EntityTypeId, ["id"] = request.Id }, ct);
        return new BitrixCrmItemGetResponse(result);
    }
}

public sealed class BitrixCrmItemListHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmItemListRequest, BitrixCrmItemListResponse>
{
    public async Task<BitrixCrmItemListResponse> HandleAsync(BitrixCrmItemListRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("crm.item.list", new JsonObject { ["entityTypeId"] = request.EntityTypeId, ["filter"] = request.Filter?.DeepClone() }, ct);
        return new BitrixCrmItemListResponse((result["items"] as JsonArray) ?? []);
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
        new(await client.CallAsync("crm.deal.get", new JsonObject { ["id"] = request.Id }, ct));
}

public sealed class BitrixCrmDealListHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmDealListRequest, BitrixCrmDealListResponse>
{
    public async Task<BitrixCrmDealListResponse> HandleAsync(BitrixCrmDealListRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("crm.deal.list", new JsonObject { ["filter"] = request.Filter?.DeepClone() }, ct);
        return new BitrixCrmDealListResponse((result["deals"] as JsonArray) ?? []);
    }
}

public sealed class BitrixCrmContactGetHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmContactGetRequest, BitrixCrmContactGetResponse>
{
    public async Task<BitrixCrmContactGetResponse> HandleAsync(BitrixCrmContactGetRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        new(await client.CallAsync("crm.contact.get", new JsonObject { ["id"] = request.Id }, ct));
}

public sealed class BitrixCrmContactListHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmContactListRequest, BitrixCrmContactListResponse>
{
    public async Task<BitrixCrmContactListResponse> HandleAsync(BitrixCrmContactListRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("crm.contact.list", new JsonObject { ["filter"] = request.Filter?.DeepClone() }, ct);
        return new BitrixCrmContactListResponse((result["contacts"] as JsonArray) ?? []);
    }
}

public sealed class BitrixCrmTimelineCommentAddHandler(BitrixRestClient client) : IMcpMethodHandler<BitrixCrmTimelineCommentAddRequest, BitrixCrmTimelineCommentAddResponse>
{
    public async Task<BitrixCrmTimelineCommentAddResponse> HandleAsync(BitrixCrmTimelineCommentAddRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("crm.timeline.comment.add", new JsonObject { ["entityType"] = request.EntityType, ["entityId"] = request.EntityId, ["comment"] = request.Comment }, ct);
        return new BitrixCrmTimelineCommentAddResponse(result["id"]?.GetValue<long>() ?? 0);
    }
}
