using System.Text.Json.Nodes;
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
    public Task<BitrixCrmItemGetResponse> BitrixCrmItemGet(int entityTypeId, long id, CancellationToken ct) =>
        InvokeAsync<BitrixCrmItemGetRequest, BitrixCrmItemGetResponse>("bitrix_crm_item_get", new BitrixCrmItemGetRequest(entityTypeId, id), ct);

    [McpServerTool(Name = "bitrix_crm_item_list", ReadOnly = true)]
    public Task<BitrixCrmItemListResponse> BitrixCrmItemList(int entityTypeId, JsonObject? filter = null, CancellationToken ct = default) =>
        InvokeAsync<BitrixCrmItemListRequest, BitrixCrmItemListResponse>("bitrix_crm_item_list", new BitrixCrmItemListRequest(entityTypeId, filter), ct);

    [McpServerTool(Name = "bitrix_crm_item_update", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<BitrixCrmItemUpdateResponse> BitrixCrmItemUpdate(int entityTypeId, long id, JsonObject fields, CancellationToken ct) =>
        InvokeAsync<BitrixCrmItemUpdateRequest, BitrixCrmItemUpdateResponse>("bitrix_crm_item_update", new BitrixCrmItemUpdateRequest(entityTypeId, id, fields), ct);

    [McpServerTool(Name = "bitrix_crm_deal_get", ReadOnly = true)]
    public Task<BitrixCrmDealGetResponse> BitrixCrmDealGet(long id, CancellationToken ct) =>
        InvokeAsync<BitrixCrmDealGetRequest, BitrixCrmDealGetResponse>("bitrix_crm_deal_get", new BitrixCrmDealGetRequest(id), ct);

    [McpServerTool(Name = "bitrix_crm_deal_list", ReadOnly = true)]
    public Task<BitrixCrmDealListResponse> BitrixCrmDealList(JsonObject? filter = null, CancellationToken ct = default) =>
        InvokeAsync<BitrixCrmDealListRequest, BitrixCrmDealListResponse>("bitrix_crm_deal_list", new BitrixCrmDealListRequest(filter), ct);

    [McpServerTool(Name = "bitrix_crm_contact_get", ReadOnly = true)]
    public Task<BitrixCrmContactGetResponse> BitrixCrmContactGet(long id, CancellationToken ct) =>
        InvokeAsync<BitrixCrmContactGetRequest, BitrixCrmContactGetResponse>("bitrix_crm_contact_get", new BitrixCrmContactGetRequest(id), ct);

    [McpServerTool(Name = "bitrix_crm_contact_list", ReadOnly = true)]
    public Task<BitrixCrmContactListResponse> BitrixCrmContactList(JsonObject? filter = null, CancellationToken ct = default) =>
        InvokeAsync<BitrixCrmContactListRequest, BitrixCrmContactListResponse>("bitrix_crm_contact_list", new BitrixCrmContactListRequest(filter), ct);

    [McpServerTool(Name = "bitrix_crm_contact_update", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<BitrixCrmContactUpdateResponse> BitrixCrmContactUpdate(long id, string? name = null, string? lastName = null, string? middleName = null, string? email = null, CancellationToken ct = default) =>
        InvokeAsync<BitrixCrmContactUpdateRequest, BitrixCrmContactUpdateResponse>("bitrix_crm_contact_update", new BitrixCrmContactUpdateRequest(id, name, lastName, middleName, email), ct);

    [McpServerTool(Name = "bitrix_crm_deal_training_direction_update", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<BitrixCrmDealTrainingDirectionUpdateResponse> BitrixCrmDealTrainingDirectionUpdate(long id, string direction, CancellationToken ct) =>
        InvokeAsync<BitrixCrmDealTrainingDirectionUpdateRequest, BitrixCrmDealTrainingDirectionUpdateResponse>("bitrix_crm_deal_training_direction_update", new BitrixCrmDealTrainingDirectionUpdateRequest(id, direction), ct);

    [McpServerTool(Name = "bitrix_crm_deal_party_email_add", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<BitrixCrmDealPartyEmailAddResponse> BitrixCrmDealPartyEmailAdd(long dealId, string recipient, string subject, string body, bool isHtml = true, bool disableCopyToSelf = false, CancellationToken ct = default) =>
        InvokeAsync<BitrixCrmDealPartyEmailAddRequest, BitrixCrmDealPartyEmailAddResponse>("bitrix_crm_deal_party_email_add", new BitrixCrmDealPartyEmailAddRequest(dealId, recipient, subject, body, isHtml, disableCopyToSelf), ct);

    [McpServerTool(Name = "bitrix_crm_timeline_comment_add", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<BitrixCrmTimelineCommentAddResponse> BitrixCrmTimelineCommentAdd(string entityType, long entityId, string comment, CancellationToken ct) =>
        InvokeAsync<BitrixCrmTimelineCommentAddRequest, BitrixCrmTimelineCommentAddResponse>("bitrix_crm_timeline_comment_add", new BitrixCrmTimelineCommentAddRequest(entityType, entityId, comment), ct);

    [McpServerTool(Name = "moodle_core_user_get_users_by_field", ReadOnly = true)]
    public Task<MoodleGetUsersByFieldResponse> MoodleGetUsersByField(MoodleUserLookupField field, IReadOnlyList<string> values, CancellationToken ct) =>
        InvokeAsync<MoodleGetUsersByFieldRequest, MoodleGetUsersByFieldResponse>("moodle_core_user_get_users_by_field", new MoodleGetUsersByFieldRequest(field, values), ct);

    [McpServerTool(Name = "moodle_core_user_create_user", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<MoodleCreateUserResponse> MoodleCreateUser(string email, string password, string firstName, string lastName, CancellationToken ct) =>
        InvokeAsync<MoodleCreateUserRequest, MoodleCreateUserResponse>("moodle_core_user_create_user", new MoodleCreateUserRequest(email, password, firstName, lastName), ct);

    [McpServerTool(Name = "moodle_core_user_update_user", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<MoodleUpdateUserResponse> MoodleUpdateUser(
        long id,
        string? username = null,
        string? auth = null,
        bool? suspended = null,
        string? password = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        int? mailDisplay = null,
        string? city = null,
        string? country = null,
        string? timezone = null,
        string? description = null,
        string? idNumber = null,
        string? institution = null,
        string? department = null,
        string? phone1 = null,
        string? phone2 = null,
        string? address = null,
        string? lang = null,
        CancellationToken ct = default) =>
        InvokeAsync<MoodleUpdateUserRequest, MoodleUpdateUserResponse>(
            "moodle_core_user_update_user",
            new MoodleUpdateUserRequest(id, username, auth, suspended, password, firstName, lastName, email, mailDisplay, city, country, timezone, description, idNumber, institution, department, phone1, phone2, address, lang),
            ct);

    [McpServerTool(Name = "moodle_core_user_get_user", ReadOnly = true)]
    public Task<MoodleGetUserResponse> MoodleGetUser(JsonObject? request = null, string? key = null, string? value = null, CancellationToken ct = default)
    {
        key ??= request?["key"]?.GetValue<string>() ?? request?["Key"]?.GetValue<string>();
        value ??= request?["value"]?.GetValue<string>() ?? request?["Value"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("The 'key' argument is required.", nameof(key));
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("The 'value' argument is required.", nameof(value));
        }

        return InvokeAsync<MoodleGetUserRequest, MoodleGetUserResponse>("moodle_core_user_get_user", new MoodleGetUserRequest(key, value), ct);
    }

    [McpServerTool(Name = "moodle_core_auth_request_password_reset", ReadOnly = false, Destructive = false)]
    public Task<MoodleRawResponse> MoodleRequestPasswordReset(string? username = null, string? email = null, CancellationToken ct = default) =>
        InvokeAsync<MoodleRequestPasswordResetRequest, MoodleRawResponse>("moodle_core_auth_request_password_reset", new MoodleRequestPasswordResetRequest(username, email), ct);

    [McpServerTool(Name = "moodle_core_enrol_get_users_courses", ReadOnly = true)]
    public Task<MoodleGetUsersCoursesResponse> MoodleGetUsersCourses(long userId, CancellationToken ct) =>
        InvokeAsync<MoodleGetUsersCoursesRequest, MoodleGetUsersCoursesResponse>("moodle_core_enrol_get_users_courses", new MoodleGetUsersCoursesRequest(userId), ct);

    [McpServerTool(Name = "moodle_enrol_manual_enrol_user", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<MoodleRawResponse> MoodleManualEnrolUser(long roleId, long userId, long courseId, long? timeStart = null, long? timeEnd = null, int? suspend = null, CancellationToken ct = default) =>
        InvokeAsync<MoodleManualEnrolUserRequest, MoodleRawResponse>("moodle_enrol_manual_enrol_user", new MoodleManualEnrolUserRequest(roleId, userId, courseId, timeStart, timeEnd, suspend), ct);

    [McpServerTool(Name = "moodle_core_completion_get_activities_completion_status", ReadOnly = true)]
    public Task<MoodleGetActivitiesCompletionStatusResponse> MoodleGetActivitiesCompletionStatus(long courseId, long userId, CancellationToken ct) =>
        InvokeAsync<MoodleGetActivitiesCompletionStatusRequest, MoodleGetActivitiesCompletionStatusResponse>("moodle_core_completion_get_activities_completion_status", new MoodleGetActivitiesCompletionStatusRequest(courseId, userId), ct);

    [McpServerTool(Name = "moodle_core_completion_get_course_completion_status", ReadOnly = true)]
    public Task<MoodleRawResponse> MoodleGetCourseCompletionStatus(long courseId, long userId, CancellationToken ct) =>
        InvokeAsync<MoodleGetCourseCompletionStatusRequest, MoodleRawResponse>("moodle_core_completion_get_course_completion_status", new MoodleGetCourseCompletionStatusRequest(courseId, userId), ct);

    [McpServerTool(Name = "moodle_core_competency_list_user_plans", ReadOnly = true)]
    public Task<MoodleRawResponse> MoodleListUserPlans(long userId, CancellationToken ct) =>
        InvokeAsync<MoodleListUserPlansRequest, MoodleRawResponse>("moodle_core_competency_list_user_plans", new MoodleListUserPlansRequest(userId), ct);

    [McpServerTool(Name = "moodle_gradereport_user_get_grade_items", ReadOnly = true)]
    public Task<MoodleGetGradeItemsResponse> MoodleGetGradeItems(long courseId, long userId, CancellationToken ct) =>
        InvokeAsync<MoodleGetGradeItemsRequest, MoodleGetGradeItemsResponse>("moodle_gradereport_user_get_grade_items", new MoodleGetGradeItemsRequest(courseId, userId), ct);

    [McpServerTool(Name = "moodle_core_course_get_courses", ReadOnly = true)]
    public Task<MoodleRawResponse> MoodleGetCourses(IReadOnlyList<long>? ids = null, CancellationToken ct = default) =>
        InvokeAsync<MoodleGetCoursesRequest, MoodleRawResponse>("moodle_core_course_get_courses", new MoodleGetCoursesRequest(ids), ct);

    [McpServerTool(Name = "moodle_core_course_get_courses_by_field", ReadOnly = true)]
    public Task<MoodleRawResponse> MoodleGetCoursesByField(string? field = null, string? value = null, CancellationToken ct = default) =>
        InvokeAsync<MoodleGetCoursesByFieldRequest, MoodleRawResponse>("moodle_core_course_get_courses_by_field", new MoodleGetCoursesByFieldRequest(field, value), ct);

    [McpServerTool(Name = "moodle_core_course_get_contents", ReadOnly = true)]
    public Task<MoodleRawResponse> MoodleGetCourseContents(long courseId, IReadOnlyList<MoodleNameValueOption>? options = null, CancellationToken ct = default) =>
        InvokeAsync<MoodleGetCourseContentsRequest, MoodleRawResponse>("moodle_core_course_get_contents", new MoodleGetCourseContentsRequest(courseId, options), ct);

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
