using AshaBridge.Core.Runtime;
using AshaBridge.Extensions.Bitrix24.Contracts;
using AshaBridge.Extensions.Moodle.Contracts;
using AshaBridge.Sdk.Contracts;
using Microsoft.AspNetCore.Http;
using ModelContextProtocol.Server;

namespace AshaBridge.AspNetCore.Extensions;

[McpServerToolType]
public sealed class AshaBridgeMcpToolSurface(
    StreamingInvocationRuntime runtime,
    IHttpContextAccessor httpContextAccessor,
    IServiceProvider services)
{
    [McpServerTool(Name = "bitrix_crm_item_get", ReadOnly = true)]
    public Task<BitrixCrmItemGetResponse> BitrixCrmItemGet(BitrixCrmItemGetRequest request, CancellationToken ct) =>
        InvokeAsync<BitrixCrmItemGetRequest, BitrixCrmItemGetResponse>("bitrix_crm_item_get", request, ct);

    [McpServerTool(Name = "bitrix_crm_item_list", ReadOnly = true)]
    public Task<BitrixCrmItemListResponse> BitrixCrmItemList(BitrixCrmItemListRequest request, CancellationToken ct) =>
        InvokeAsync<BitrixCrmItemListRequest, BitrixCrmItemListResponse>("bitrix_crm_item_list", request, ct);

    [McpServerTool(Name = "bitrix_crm_item_update", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<BitrixCrmItemUpdateResponse> BitrixCrmItemUpdate(BitrixCrmItemUpdateRequest request, CancellationToken ct) =>
        InvokeAsync<BitrixCrmItemUpdateRequest, BitrixCrmItemUpdateResponse>("bitrix_crm_item_update", request, ct);

    [McpServerTool(Name = "bitrix_crm_deal_get", ReadOnly = true)]
    public Task<BitrixCrmDealGetResponse> BitrixCrmDealGet(BitrixCrmDealGetRequest request, CancellationToken ct) =>
        InvokeAsync<BitrixCrmDealGetRequest, BitrixCrmDealGetResponse>("bitrix_crm_deal_get", request, ct);

    [McpServerTool(Name = "bitrix_crm_deal_list", ReadOnly = true)]
    public Task<BitrixCrmDealListResponse> BitrixCrmDealList(BitrixCrmDealListRequest request, CancellationToken ct) =>
        InvokeAsync<BitrixCrmDealListRequest, BitrixCrmDealListResponse>("bitrix_crm_deal_list", request, ct);

    [McpServerTool(Name = "bitrix_crm_contact_get", ReadOnly = true)]
    public Task<BitrixCrmContactGetResponse> BitrixCrmContactGet(BitrixCrmContactGetRequest request, CancellationToken ct) =>
        InvokeAsync<BitrixCrmContactGetRequest, BitrixCrmContactGetResponse>("bitrix_crm_contact_get", request, ct);

    [McpServerTool(Name = "bitrix_crm_contact_list", ReadOnly = true)]
    public Task<BitrixCrmContactListResponse> BitrixCrmContactList(BitrixCrmContactListRequest request, CancellationToken ct) =>
        InvokeAsync<BitrixCrmContactListRequest, BitrixCrmContactListResponse>("bitrix_crm_contact_list", request, ct);

    [McpServerTool(Name = "bitrix_crm_timeline_comment_add", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<BitrixCrmTimelineCommentAddResponse> BitrixCrmTimelineCommentAdd(BitrixCrmTimelineCommentAddRequest request, CancellationToken ct) =>
        InvokeAsync<BitrixCrmTimelineCommentAddRequest, BitrixCrmTimelineCommentAddResponse>("bitrix_crm_timeline_comment_add", request, ct);

    [McpServerTool(Name = "moodle_core_user_get_users_by_field", ReadOnly = true)]
    public Task<MoodleGetUsersByFieldResponse> MoodleGetUsersByField(MoodleGetUsersByFieldRequest request, CancellationToken ct) =>
        InvokeAsync<MoodleGetUsersByFieldRequest, MoodleGetUsersByFieldResponse>("moodle_core_user_get_users_by_field", request, ct);

    [McpServerTool(Name = "moodle_core_enrol_get_users_courses", ReadOnly = true)]
    public Task<MoodleGetUsersCoursesResponse> MoodleGetUsersCourses(MoodleGetUsersCoursesRequest request, CancellationToken ct) =>
        InvokeAsync<MoodleGetUsersCoursesRequest, MoodleGetUsersCoursesResponse>("moodle_core_enrol_get_users_courses", request, ct);

    [McpServerTool(Name = "moodle_core_completion_get_activities_completion_status", ReadOnly = true)]
    public Task<MoodleGetActivitiesCompletionStatusResponse> MoodleGetActivitiesCompletionStatus(MoodleGetActivitiesCompletionStatusRequest request, CancellationToken ct) =>
        InvokeAsync<MoodleGetActivitiesCompletionStatusRequest, MoodleGetActivitiesCompletionStatusResponse>("moodle_core_completion_get_activities_completion_status", request, ct);

    [McpServerTool(Name = "moodle_gradereport_user_get_grade_items", ReadOnly = true)]
    public Task<MoodleGetGradeItemsResponse> MoodleGetGradeItems(MoodleGetGradeItemsRequest request, CancellationToken ct) =>
        InvokeAsync<MoodleGetGradeItemsRequest, MoodleGetGradeItemsResponse>("moodle_gradereport_user_get_grade_items", request, ct);

    private async Task<TResponse> InvokeAsync<TRequest, TResponse>(string methodName, TRequest request, CancellationToken ct)
        where TRequest : IMcpRequest<TResponse>
    {
        var http = httpContextAccessor.HttpContext;
        var execution = new AshaBridgeExecutionContext(
            correlationId: http?.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? http?.TraceIdentifier ?? Guid.NewGuid().ToString("n"),
            userId: http?.User.Identity?.Name,
            organizationId: http?.User.FindFirst("organization_id")?.Value,
            tenantId: http?.User.FindFirst("tenant_id")?.Value,
            idempotencyKey: TryGetIdempotencyKey(http),
            permissions: http?.User.FindAll("permission").Select(c => c.Value).ToArray() ?? [],
            services: services,
            requestAborted: http?.RequestAborted ?? ct);

        await foreach (var @event in runtime.InvokeAsync(methodName, request, execution, ct).ConfigureAwait(false))
        {
            if (@event is MethodCompletedEvent<TResponse> completed)
            {
                return completed.Response;
            }

            if (@event is MethodFailedEvent failed)
            {
                throw new InvalidOperationException($"{failed.Error.Code}: {failed.Error.Message}");
            }
        }

        throw new InvalidOperationException($"MCP method '{methodName}' completed without a response.");
    }

    private static IdempotencyKey? TryGetIdempotencyKey(HttpContext? http)
    {
        var value = http?.Request.Headers["Idempotency-Key"].FirstOrDefault();
        return string.IsNullOrWhiteSpace(value) ? null : new IdempotencyKey(value);
    }
}
