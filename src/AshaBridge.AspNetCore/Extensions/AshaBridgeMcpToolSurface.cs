using System.Reflection;
using System.Text.Json.Nodes;
using AshaBridge.Core.Runtime;
using AshaBridge.Extensions.Bitrix24.Contracts;
using AshaBridge.Extensions.Moodle.Contracts;
using AshaBridge.Sdk.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace AshaBridge.AspNetCore.Extensions;

[McpServerToolType]
public sealed class AshaBridgeMcpToolSurface(
    StreamingInvocationRuntime runtime,
    IHttpContextAccessor httpContextAccessor,
    IServiceProvider services,
    ILogger<AshaBridgeMcpToolSurface> logger)
{
    [McpServerTool(Name = "bitrix_crm_item_get", ReadOnly = true)]
    public Task<BitrixCrmItemGetResponse> BitrixCrmItemGet(int entityTypeId, long id, CancellationToken ct) =>
        InvokeAsync<BitrixCrmItemGetRequest, BitrixCrmItemGetResponse>("bitrix_crm_item_get", new BitrixCrmItemGetRequest(entityTypeId, id), ct);

    [McpServerTool(Name = "bitrix_crm_dynamic_items_list_all", ReadOnly = true)]
    public Task<BitrixCrmItemListResponse> BitrixCrmDynamicItemsListAll(int entityTypeId, CancellationToken ct) =>
        InvokeAsync<BitrixCrmDynamicItemsListAllRequest, BitrixCrmItemListResponse>("bitrix_crm_dynamic_items_list_all", new BitrixCrmDynamicItemsListAllRequest(entityTypeId), ct);

    public Task<BitrixCrmItemListResponse> BitrixCrmItemList(int entityTypeId, string filterJson = "", CancellationToken ct = default) =>
        InvokeAsync<BitrixCrmItemListRequest, BitrixCrmItemListResponse>("bitrix_crm_item_list", new BitrixCrmItemListRequest(entityTypeId, ParseOptionalJsonObject(filterJson, nameof(filterJson))), ct);

    public Task<BitrixCrmItemUpdateResponse> BitrixCrmItemUpdate(int entityTypeId, long id, string fieldsJson, CancellationToken ct) =>
        InvokeAsync<BitrixCrmItemUpdateRequest, BitrixCrmItemUpdateResponse>("bitrix_crm_item_update", new BitrixCrmItemUpdateRequest(entityTypeId, id, ParseRequiredJsonObject(fieldsJson, nameof(fieldsJson))), ct);

    [McpServerTool(Name = "bitrix_crm_deal_get", ReadOnly = true)]
    public Task<BitrixCrmDealGetResponse> BitrixCrmDealGet(long id, CancellationToken ct) =>
        InvokeAsync<BitrixCrmDealGetRequest, BitrixCrmDealGetResponse>("bitrix_crm_deal_get", new BitrixCrmDealGetRequest(id), ct);

    [McpServerTool(Name = "bitrix_crm_deals_list_all", ReadOnly = true)]
    public Task<BitrixCrmDealListResponse> BitrixCrmDealsListAll(CancellationToken ct) =>
        InvokeAsync<BitrixCrmDealsListAllRequest, BitrixCrmDealListResponse>("bitrix_crm_deals_list_all", new BitrixCrmDealsListAllRequest(), ct);

    [McpServerTool(Name = "bitrix_crm_deals_find_by_contact_id", ReadOnly = true)]
    public Task<BitrixCrmDealListResponse> BitrixCrmDealsFindByContactId(long contactId, CancellationToken ct) =>
        InvokeAsync<BitrixCrmDealsFindByContactIdRequest, BitrixCrmDealListResponse>("bitrix_crm_deals_find_by_contact_id", new BitrixCrmDealsFindByContactIdRequest(contactId), ct);

    public Task<BitrixCrmDealListResponse> BitrixCrmDealList(string filterJson = "", CancellationToken ct = default) =>
        InvokeAsync<BitrixCrmDealListRequest, BitrixCrmDealListResponse>("bitrix_crm_deal_list", new BitrixCrmDealListRequest(ParseOptionalJsonObject(filterJson, nameof(filterJson))), ct);

    [McpServerTool(Name = "bitrix_crm_contact_get", ReadOnly = true)]
    public Task<BitrixCrmContactGetResponse> BitrixCrmContactGet(long id, CancellationToken ct) =>
        InvokeAsync<BitrixCrmContactGetRequest, BitrixCrmContactGetResponse>("bitrix_crm_contact_get", new BitrixCrmContactGetRequest(id), ct);

    [McpServerTool(Name = "bitrix_crm_contacts_list_all", ReadOnly = true)]
    public Task<BitrixCrmContactListResponse> BitrixCrmContactsListAll(CancellationToken ct) =>
        InvokeAsync<BitrixCrmContactsListAllRequest, BitrixCrmContactListResponse>("bitrix_crm_contacts_list_all", new BitrixCrmContactsListAllRequest(), ct);

    [McpServerTool(Name = "bitrix_crm_contacts_find_by_email", ReadOnly = true)]
    public Task<BitrixCrmContactListResponse> BitrixCrmContactsFindByEmail(string email, CancellationToken ct) =>
        InvokeAsync<BitrixCrmContactsFindByEmailRequest, BitrixCrmContactListResponse>("bitrix_crm_contacts_find_by_email", new BitrixCrmContactsFindByEmailRequest(email), ct);

    public Task<BitrixCrmContactListResponse> BitrixCrmContactList(string filterJson = "", CancellationToken ct = default) =>
        InvokeAsync<BitrixCrmContactListRequest, BitrixCrmContactListResponse>("bitrix_crm_contact_list", new BitrixCrmContactListRequest(ParseOptionalJsonObject(filterJson, nameof(filterJson))), ct);

    [McpServerTool(Name = "bitrix_crm_contact_update_name", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<BitrixCrmContactUpdateResponse> BitrixCrmContactUpdateName(long id, string name, string lastName = "", string middleName = "", CancellationToken ct = default) =>
        InvokeAsync<BitrixCrmContactUpdateNameRequest, BitrixCrmContactUpdateResponse>("bitrix_crm_contact_update_name", new BitrixCrmContactUpdateNameRequest(id, name, EmptyToNull(lastName), EmptyToNull(middleName)), ct);

    [McpServerTool(Name = "bitrix_crm_contact_update_email", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<BitrixCrmContactUpdateResponse> BitrixCrmContactUpdateEmail(long id, string email, CancellationToken ct) =>
        InvokeAsync<BitrixCrmContactUpdateEmailRequest, BitrixCrmContactUpdateResponse>("bitrix_crm_contact_update_email", new BitrixCrmContactUpdateEmailRequest(id, email), ct);

    public Task<BitrixCrmContactUpdateResponse> BitrixCrmContactUpdate(long id, string name = "", string lastName = "", string middleName = "", string email = "", CancellationToken ct = default) =>
        InvokeAsync<BitrixCrmContactUpdateRequest, BitrixCrmContactUpdateResponse>("bitrix_crm_contact_update", new BitrixCrmContactUpdateRequest(id, EmptyToNull(name), EmptyToNull(lastName), EmptyToNull(middleName), EmptyToNull(email)), ct);

    [McpServerTool(Name = "bitrix_crm_deal_training_direction_update", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<BitrixCrmDealTrainingDirectionUpdateResponse> BitrixCrmDealTrainingDirectionUpdate(long id, string direction, CancellationToken ct) =>
        InvokeAsync<BitrixCrmDealTrainingDirectionUpdateRequest, BitrixCrmDealTrainingDirectionUpdateResponse>("bitrix_crm_deal_training_direction_update", new BitrixCrmDealTrainingDirectionUpdateRequest(id, direction), ct);

    [McpServerTool(Name = "bitrix_crm_deal_party_email_add", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<BitrixCrmDealPartyEmailAddResponse> BitrixCrmDealPartyEmailAdd(long dealId, string recipient, string subject, string body, bool isHtml = true, bool disableCopyToSelf = false, CancellationToken ct = default) =>
        InvokeAsync<BitrixCrmDealPartyEmailAddRequest, BitrixCrmDealPartyEmailAddResponse>("bitrix_crm_deal_party_email_add", new BitrixCrmDealPartyEmailAddRequest(dealId, recipient, subject, body, isHtml, disableCopyToSelf), ct);

    [McpServerTool(Name = "bitrix_crm_timeline_comment_add", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<BitrixCrmTimelineCommentAddResponse> BitrixCrmTimelineCommentAdd(string entityType, long entityId, string comment, CancellationToken ct) =>
        InvokeAsync<BitrixCrmTimelineCommentAddRequest, BitrixCrmTimelineCommentAddResponse>("bitrix_crm_timeline_comment_add", new BitrixCrmTimelineCommentAddRequest(entityType, entityId, comment), ct);

    public Task<MoodleGetUsersByFieldResponse> MoodleGetUsersByField(MoodleUserLookupField field, IReadOnlyList<string> values, CancellationToken ct) =>
        InvokeAsync<MoodleGetUsersByFieldRequest, MoodleGetUsersByFieldResponse>("moodle_core_user_get_users_by_field", new MoodleGetUsersByFieldRequest(field, values), ct);

    [McpServerTool(Name = "moodle_core_user_create_user", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<MoodleCreateUserResponse> MoodleCreateUser(string email, string password, string firstName, string lastName, CancellationToken ct) =>
        InvokeAsync<MoodleCreateUserRequest, MoodleCreateUserResponse>("moodle_core_user_create_user", new MoodleCreateUserRequest(email, password, firstName, lastName), ct);

    public Task<MoodleUpdateUserResponse> MoodleUpdateUser(
        long id,
        string username = "",
        string auth = "",
        bool suspended = false,
        bool updateSuspended = false,
        string password = "",
        string firstName = "",
        string lastName = "",
        string email = "",
        int mailDisplay = -1,
        string city = "",
        string country = "",
        string timezone = "",
        string description = "",
        string idNumber = "",
        string institution = "",
        string department = "",
        string phone1 = "",
        string phone2 = "",
        string address = "",
        string lang = "",
        CancellationToken ct = default) =>
        InvokeAsync<MoodleUpdateUserRequest, MoodleUpdateUserResponse>(
            "moodle_core_user_update_user",
            new MoodleUpdateUserRequest(id, EmptyToNull(username), EmptyToNull(auth), updateSuspended ? suspended : null, EmptyToNull(password), EmptyToNull(firstName), EmptyToNull(lastName), EmptyToNull(email), mailDisplay >= 0 ? mailDisplay : null, EmptyToNull(city), EmptyToNull(country), EmptyToNull(timezone), EmptyToNull(description), EmptyToNull(idNumber), EmptyToNull(institution), EmptyToNull(department), EmptyToNull(phone1), EmptyToNull(phone2), EmptyToNull(address), EmptyToNull(lang)),
            ct);

    [McpServerTool(Name = "moodle_user_update_name", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<MoodleUpdateUserResponse> MoodleUserUpdateName(long id, string firstName, string lastName, CancellationToken ct) =>
        InvokeAsync<MoodleUserUpdateNameRequest, MoodleUpdateUserResponse>("moodle_user_update_name", new MoodleUserUpdateNameRequest(id, firstName, lastName), ct);

    [McpServerTool(Name = "moodle_user_update_email", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<MoodleUpdateUserResponse> MoodleUserUpdateEmail(long id, string email, CancellationToken ct) =>
        InvokeAsync<MoodleUserUpdateEmailRequest, MoodleUpdateUserResponse>("moodle_user_update_email", new MoodleUserUpdateEmailRequest(id, email), ct);

    [McpServerTool(Name = "moodle_user_update_username", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<MoodleUpdateUserResponse> MoodleUserUpdateUsername(long id, string username, CancellationToken ct) =>
        InvokeAsync<MoodleUserUpdateUsernameRequest, MoodleUpdateUserResponse>("moodle_user_update_username", new MoodleUserUpdateUsernameRequest(id, username), ct);

    [McpServerTool(Name = "moodle_user_update_password", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<MoodleUpdateUserResponse> MoodleUserUpdatePassword(long id, string password, CancellationToken ct) =>
        InvokeAsync<MoodleUserUpdatePasswordRequest, MoodleUpdateUserResponse>("moodle_user_update_password", new MoodleUserUpdatePasswordRequest(id, password), ct);

    [McpServerTool(Name = "moodle_user_suspend", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<MoodleUpdateUserResponse> MoodleUserSuspend(long id, CancellationToken ct) =>
        InvokeAsync<MoodleUserSuspendRequest, MoodleUpdateUserResponse>("moodle_user_suspend", new MoodleUserSuspendRequest(id), ct);

    [McpServerTool(Name = "moodle_user_unsuspend", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<MoodleUpdateUserResponse> MoodleUserUnsuspend(long id, CancellationToken ct) =>
        InvokeAsync<MoodleUserUnsuspendRequest, MoodleUpdateUserResponse>("moodle_user_unsuspend", new MoodleUserUnsuspendRequest(id), ct);

    public Task<MoodleGetUserResponse> MoodleGetUser(string key, string value, CancellationToken ct)
    {
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

    [McpServerTool(Name = "moodle_user_find_by_email", ReadOnly = true)]
    public Task<MoodleFindUserResponse> MoodleFindUserByEmail(string email = "", string query = "", CancellationToken ct = default)
    {
        var lookup = FirstNotEmpty(email, query);
        return lookup is null
            ? MissingLookupAsync("moodle_user_find_by_email", "email|value|query")
            : InvokeAsync<MoodleFindUserByEmailRequest, MoodleFindUserResponse>("moodle_user_find_by_email", new MoodleFindUserByEmailRequest(lookup), ct);
    }

    [McpServerTool(Name = "moodle_user_find_by_id", ReadOnly = true)]
    public Task<MoodleFindUserResponse> MoodleFindUserById(long id = 0, string query = "", CancellationToken ct = default)
    {
        var lookup = id > 0
            ? id.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : FirstNotEmpty(query);
        return lookup is null
            ? MissingLookupAsync("moodle_user_find_by_id", "id|value|query")
            : InvokeAsync<MoodleFindUserByIdRequest, MoodleFindUserResponse>("moodle_user_find_by_id", new MoodleFindUserByIdRequest(long.Parse(lookup, System.Globalization.CultureInfo.InvariantCulture)), ct);
    }

    [McpServerTool(Name = "moodle_user_find_by_username", ReadOnly = true)]
    public Task<MoodleFindUserResponse> MoodleFindUserByUsername(string username = "", string query = "", CancellationToken ct = default)
    {
        var lookup = FirstNotEmpty(username, query);
        return lookup is null
            ? MissingLookupAsync("moodle_user_find_by_username", "username|value|query")
            : InvokeAsync<MoodleFindUserByUsernameRequest, MoodleFindUserResponse>("moodle_user_find_by_username", new MoodleFindUserByUsernameRequest(lookup), ct);
    }

    public Task<MoodleRawResponse> MoodleRequestPasswordReset(string username = "", string email = "", CancellationToken ct = default) =>
        InvokeAsync<MoodleRequestPasswordResetRequest, MoodleRawResponse>("moodle_core_auth_request_password_reset", new MoodleRequestPasswordResetRequest(EmptyToNull(username), EmptyToNull(email)), ct);

    [McpServerTool(Name = "moodle_user_request_password_reset_by_email", ReadOnly = false, Destructive = false)]
    public Task<MoodleRawResponse> MoodleRequestPasswordResetByEmail(string email, CancellationToken ct) =>
        InvokeAsync<MoodleRequestPasswordResetByEmailRequest, MoodleRawResponse>("moodle_user_request_password_reset_by_email", new MoodleRequestPasswordResetByEmailRequest(email), ct);

    [McpServerTool(Name = "moodle_user_request_password_reset_by_username", ReadOnly = false, Destructive = false)]
    public Task<MoodleRawResponse> MoodleRequestPasswordResetByUsername(string username, CancellationToken ct) =>
        InvokeAsync<MoodleRequestPasswordResetByUsernameRequest, MoodleRawResponse>("moodle_user_request_password_reset_by_username", new MoodleRequestPasswordResetByUsernameRequest(username), ct);

    [McpServerTool(Name = "moodle_core_enrol_get_users_courses", ReadOnly = true)]
    public Task<MoodleGetUsersCoursesResponse> MoodleGetUsersCourses(long userId, CancellationToken ct) =>
        InvokeAsync<MoodleGetUsersCoursesRequest, MoodleGetUsersCoursesResponse>("moodle_core_enrol_get_users_courses", new MoodleGetUsersCoursesRequest(userId), ct);

    public Task<MoodleRawResponse> MoodleManualEnrolUser(long roleId, long userId, long courseId, long timeStart = 0, long timeEnd = 0, int suspend = -1, CancellationToken ct = default) =>
        InvokeAsync<MoodleManualEnrolUserRequest, MoodleRawResponse>("moodle_enrol_manual_enrol_user", new MoodleManualEnrolUserRequest(roleId, userId, courseId, PositiveToNullable(timeStart), PositiveToNullable(timeEnd), suspend >= 0 ? suspend : null), ct);

    [McpServerTool(Name = "moodle_user_enrol", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<MoodleRawResponse> MoodleUserEnrol(long roleId, long userId, long courseId, CancellationToken ct) =>
        InvokeAsync<MoodleUserEnrolRequest, MoodleRawResponse>("moodle_user_enrol", new MoodleUserEnrolRequest(roleId, userId, courseId), ct);

    [McpServerTool(Name = "moodle_user_enrol_as_student", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<MoodleRawResponse> MoodleUserEnrolAsStudent(long userId, long courseId, CancellationToken ct) =>
        InvokeAsync<MoodleUserEnrolAsStudentRequest, MoodleRawResponse>("moodle_user_enrol_as_student", new MoodleUserEnrolAsStudentRequest(userId, courseId), ct);

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

    public Task<MoodleRawResponse> MoodleGetCourses(string idsCsv = "", CancellationToken ct = default) =>
        InvokeAsync<MoodleGetCoursesRequest, MoodleRawResponse>("moodle_core_course_get_courses", new MoodleGetCoursesRequest(ParseLongCsv(idsCsv)), ct);

    public Task<MoodleRawResponse> MoodleGetCoursesByField(string field = "", string value = "", CancellationToken ct = default) =>
        InvokeAsync<MoodleGetCoursesByFieldRequest, MoodleRawResponse>("moodle_core_course_get_courses_by_field", new MoodleGetCoursesByFieldRequest(EmptyToNull(field), EmptyToNull(value)), ct);

    public Task<MoodleRawResponse> MoodleGetCourseContents(long courseId, string optionsJson = "", CancellationToken ct = default) =>
        InvokeAsync<MoodleGetCourseContentsRequest, MoodleRawResponse>("moodle_core_course_get_contents", new MoodleGetCourseContentsRequest(courseId, ParseMoodleOptions(optionsJson)), ct);

    [McpServerTool(Name = "moodle_course_get_by_id", ReadOnly = true)]
    public Task<MoodleRawResponse> MoodleCourseGetById(long id, CancellationToken ct) =>
        InvokeAsync<MoodleCourseGetByIdRequest, MoodleRawResponse>("moodle_course_get_by_id", new MoodleCourseGetByIdRequest(id), ct);

    [McpServerTool(Name = "moodle_course_find_by_shortname", ReadOnly = true)]
    public Task<MoodleRawResponse> MoodleCourseFindByShortName(string shortName, CancellationToken ct) =>
        InvokeAsync<MoodleCourseFindByShortNameRequest, MoodleRawResponse>("moodle_course_find_by_shortname", new MoodleCourseFindByShortNameRequest(shortName), ct);

    [McpServerTool(Name = "moodle_course_find_by_idnumber", ReadOnly = true)]
    public Task<MoodleRawResponse> MoodleCourseFindByIdNumber(string idNumber, CancellationToken ct) =>
        InvokeAsync<MoodleCourseFindByIdNumberRequest, MoodleRawResponse>("moodle_course_find_by_idnumber", new MoodleCourseFindByIdNumberRequest(idNumber), ct);

    [McpServerTool(Name = "moodle_courses_find_by_category", ReadOnly = true)]
    public Task<MoodleRawResponse> MoodleCoursesFindByCategory(string category, CancellationToken ct) =>
        InvokeAsync<MoodleCoursesFindByCategoryRequest, MoodleRawResponse>("moodle_courses_find_by_category", new MoodleCoursesFindByCategoryRequest(category), ct);

    [McpServerTool(Name = "moodle_course_get_contents", ReadOnly = true)]
    public Task<MoodleRawResponse> MoodleCourseGetContents(long courseId, CancellationToken ct) =>
        InvokeAsync<MoodleCourseGetContentsRequest, MoodleRawResponse>("moodle_course_get_contents", new MoodleCourseGetContentsRequest(courseId), ct);

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

        logger.LogInformation(
            "Invoking AshaBridge MCP method {MethodName}. CorrelationId={CorrelationId}; Request={RequestSummary}",
            methodName,
            execution.CorrelationId,
            SummarizeRequest(request));

        await foreach (var @event in runtime.InvokeAsync(methodName, request, execution, ct).ConfigureAwait(false))
        {
            if (@event is MethodCompletedEvent<TResponse> completed)
            {
                logger.LogInformation(
                    "Completed AshaBridge MCP method {MethodName}. CorrelationId={CorrelationId}",
                    methodName,
                    execution.CorrelationId);
                return completed.Response;
            }

            if (@event is MethodFailedEvent failed)
            {
                logger.LogWarning(
                    "Failed AshaBridge MCP method {MethodName}. CorrelationId={CorrelationId}; ErrorCode={ErrorCode}; ErrorMessage={ErrorMessage}",
                    methodName,
                    execution.CorrelationId,
                    failed.Error.Code,
                    failed.Error.Message);
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

    private static string? EmptyToNull(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private Task<MoodleFindUserResponse> MissingLookupAsync(string toolName, string expectedArguments)
    {
        logger.LogWarning(
            "Moodle lookup tool {ToolName} was called without a lookup value. Expected one of: {ExpectedArguments}",
            toolName,
            expectedArguments);
        return Task.FromResult(new MoodleFindUserResponse(false, "", "", null));
    }

    private static string? FirstNotEmpty(params string[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private static string SummarizeRequest<TRequest>(TRequest request)
    {
        var values = request?.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => $"{property.Name}={FormatLogValue(property.Name, property.GetValue(request))}");

        return values is null ? "<null>" : string.Join(", ", values);
    }

    private static string FormatLogValue(string name, object? value)
    {
        if (value is null)
        {
            return "<null>";
        }

        if (name.Contains("password", StringComparison.OrdinalIgnoreCase)
            || name.Contains("token", StringComparison.OrdinalIgnoreCase)
            || name.Contains("body", StringComparison.OrdinalIgnoreCase)
            || name.Contains("comment", StringComparison.OrdinalIgnoreCase))
        {
            return "<redacted>";
        }

        var text = value.ToString() ?? "";
        return text.Length <= 160 ? text : string.Concat(text.AsSpan(0, 160), "...");
    }

    private static long? PositiveToNullable(long value) =>
        value > 0 ? value : null;

    private static IReadOnlyList<long>? ParseLongCsv(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => long.Parse(part, System.Globalization.CultureInfo.InvariantCulture))
            .ToArray();
    }

    private static JsonObject? ParseOptionalJsonObject(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value) ? null : ParseRequiredJsonObject(value, parameterName);

    private static JsonObject ParseRequiredJsonObject(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A JSON object string is required.", parameterName);
        }

        return JsonNode.Parse(value) as JsonObject
            ?? throw new ArgumentException("The value must be a JSON object.", parameterName);
    }

    private static IReadOnlyList<MoodleNameValueOption>? ParseMoodleOptions(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var array = JsonNode.Parse(value) as JsonArray
            ?? throw new ArgumentException("The optionsJson value must be a JSON array.", nameof(value));

        return array.Select(item => new MoodleNameValueOption(
                item?["name"]?.GetValue<string>() ?? throw new ArgumentException("Each option must include a name.", nameof(value)),
                item?["value"]?.GetValue<string>() ?? throw new ArgumentException("Each option must include a value.", nameof(value))))
            .ToArray();
    }
}
