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

    [McpServerTool(Name = "bitrix_crm_item_list", ReadOnly = true)]
    public Task<BitrixCrmItemListResponse> BitrixCrmItemList(int entityTypeId, string filterJson = "", CancellationToken ct = default) =>
        InvokeAsync<BitrixCrmItemListRequest, BitrixCrmItemListResponse>("bitrix_crm_item_list", new BitrixCrmItemListRequest(entityTypeId, ParseOptionalJsonObject(filterJson, nameof(filterJson))), ct);

    [McpServerTool(Name = "bitrix_crm_item_update", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<BitrixCrmItemUpdateResponse> BitrixCrmItemUpdate(int entityTypeId, long id, string fieldsJson, CancellationToken ct) =>
        InvokeAsync<BitrixCrmItemUpdateRequest, BitrixCrmItemUpdateResponse>("bitrix_crm_item_update", new BitrixCrmItemUpdateRequest(entityTypeId, id, ParseRequiredJsonObject(fieldsJson, nameof(fieldsJson))), ct);

    [McpServerTool(Name = "bitrix_crm_deal_get", ReadOnly = true)]
    public Task<BitrixCrmDealGetResponse> BitrixCrmDealGet(long id, CancellationToken ct) =>
        InvokeAsync<BitrixCrmDealGetRequest, BitrixCrmDealGetResponse>("bitrix_crm_deal_get", new BitrixCrmDealGetRequest(id), ct);

    [McpServerTool(Name = "bitrix_crm_deal_list", ReadOnly = true)]
    public Task<BitrixCrmDealListResponse> BitrixCrmDealList(string filterJson = "", CancellationToken ct = default) =>
        InvokeAsync<BitrixCrmDealListRequest, BitrixCrmDealListResponse>("bitrix_crm_deal_list", new BitrixCrmDealListRequest(ParseOptionalJsonObject(filterJson, nameof(filterJson))), ct);

    [McpServerTool(Name = "bitrix_crm_contact_get", ReadOnly = true)]
    public Task<BitrixCrmContactGetResponse> BitrixCrmContactGet(long id, CancellationToken ct) =>
        InvokeAsync<BitrixCrmContactGetRequest, BitrixCrmContactGetResponse>("bitrix_crm_contact_get", new BitrixCrmContactGetRequest(id), ct);

    [McpServerTool(Name = "bitrix_crm_contact_list", ReadOnly = true)]
    public Task<BitrixCrmContactListResponse> BitrixCrmContactList(string filterJson = "", CancellationToken ct = default) =>
        InvokeAsync<BitrixCrmContactListRequest, BitrixCrmContactListResponse>("bitrix_crm_contact_list", new BitrixCrmContactListRequest(ParseOptionalJsonObject(filterJson, nameof(filterJson))), ct);

    [McpServerTool(Name = "bitrix_crm_contact_update", ReadOnly = false, Destructive = false, Idempotent = true)]
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

    [McpServerTool(Name = "moodle_core_user_get_users_by_field", ReadOnly = true)]
    public Task<MoodleGetUsersByFieldResponse> MoodleGetUsersByField(MoodleUserLookupField field, IReadOnlyList<string> values, CancellationToken ct) =>
        InvokeAsync<MoodleGetUsersByFieldRequest, MoodleGetUsersByFieldResponse>("moodle_core_user_get_users_by_field", new MoodleGetUsersByFieldRequest(field, values), ct);

    [McpServerTool(Name = "moodle_core_user_create_user", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<MoodleCreateUserResponse> MoodleCreateUser(string email, string password, string firstName, string lastName, CancellationToken ct) =>
        InvokeAsync<MoodleCreateUserRequest, MoodleCreateUserResponse>("moodle_core_user_create_user", new MoodleCreateUserRequest(email, password, firstName, lastName), ct);

    [McpServerTool(Name = "moodle_core_user_update_user", ReadOnly = false, Destructive = false, Idempotent = true)]
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

    [McpServerTool(Name = "moodle_core_user_get_user", ReadOnly = true)]
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
    public Task<MoodleGetUserResponse> MoodleFindUserByEmail(string email = "", string query = "", CancellationToken ct = default)
    {
        var lookup = FirstNotEmpty(email, query);
        return lookup is null
            ? MissingLookupAsync("moodle_user_find_by_email", "email|value|query")
            : InvokeAsync<MoodleGetUserRequest, MoodleGetUserResponse>("moodle_core_user_get_user", new MoodleGetUserRequest("email", lookup), ct);
    }

    [McpServerTool(Name = "moodle_user_find_by_id", ReadOnly = true)]
    public Task<MoodleGetUserResponse> MoodleFindUserById(long id = 0, string query = "", CancellationToken ct = default)
    {
        var lookup = id > 0
            ? id.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : FirstNotEmpty(query);
        return lookup is null
            ? MissingLookupAsync("moodle_user_find_by_id", "id|value|query")
            : InvokeAsync<MoodleGetUserRequest, MoodleGetUserResponse>("moodle_core_user_get_user", new MoodleGetUserRequest("id", lookup), ct);
    }

    [McpServerTool(Name = "moodle_user_find_by_username", ReadOnly = true)]
    public Task<MoodleGetUserResponse> MoodleFindUserByUsername(string username = "", string query = "", CancellationToken ct = default)
    {
        var lookup = FirstNotEmpty(username, query);
        return lookup is null
            ? MissingLookupAsync("moodle_user_find_by_username", "username|value|query")
            : InvokeAsync<MoodleGetUserRequest, MoodleGetUserResponse>("moodle_core_user_get_user", new MoodleGetUserRequest("username", lookup), ct);
    }

    [McpServerTool(Name = "moodle_core_auth_request_password_reset", ReadOnly = false, Destructive = false)]
    public Task<MoodleRawResponse> MoodleRequestPasswordReset(string username = "", string email = "", CancellationToken ct = default) =>
        InvokeAsync<MoodleRequestPasswordResetRequest, MoodleRawResponse>("moodle_core_auth_request_password_reset", new MoodleRequestPasswordResetRequest(EmptyToNull(username), EmptyToNull(email)), ct);

    [McpServerTool(Name = "moodle_core_enrol_get_users_courses", ReadOnly = true)]
    public Task<MoodleGetUsersCoursesResponse> MoodleGetUsersCourses(long userId, CancellationToken ct) =>
        InvokeAsync<MoodleGetUsersCoursesRequest, MoodleGetUsersCoursesResponse>("moodle_core_enrol_get_users_courses", new MoodleGetUsersCoursesRequest(userId), ct);

    [McpServerTool(Name = "moodle_enrol_manual_enrol_user", ReadOnly = false, Destructive = false, Idempotent = true)]
    public Task<MoodleRawResponse> MoodleManualEnrolUser(long roleId, long userId, long courseId, long timeStart = 0, long timeEnd = 0, int suspend = -1, CancellationToken ct = default) =>
        InvokeAsync<MoodleManualEnrolUserRequest, MoodleRawResponse>("moodle_enrol_manual_enrol_user", new MoodleManualEnrolUserRequest(roleId, userId, courseId, PositiveToNullable(timeStart), PositiveToNullable(timeEnd), suspend >= 0 ? suspend : null), ct);

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
    public Task<MoodleRawResponse> MoodleGetCourses(string idsCsv = "", CancellationToken ct = default) =>
        InvokeAsync<MoodleGetCoursesRequest, MoodleRawResponse>("moodle_core_course_get_courses", new MoodleGetCoursesRequest(ParseLongCsv(idsCsv)), ct);

    [McpServerTool(Name = "moodle_core_course_get_courses_by_field", ReadOnly = true)]
    public Task<MoodleRawResponse> MoodleGetCoursesByField(string field = "", string value = "", CancellationToken ct = default) =>
        InvokeAsync<MoodleGetCoursesByFieldRequest, MoodleRawResponse>("moodle_core_course_get_courses_by_field", new MoodleGetCoursesByFieldRequest(EmptyToNull(field), EmptyToNull(value)), ct);

    [McpServerTool(Name = "moodle_core_course_get_contents", ReadOnly = true)]
    public Task<MoodleRawResponse> MoodleGetCourseContents(long courseId, string optionsJson = "", CancellationToken ct = default) =>
        InvokeAsync<MoodleGetCourseContentsRequest, MoodleRawResponse>("moodle_core_course_get_contents", new MoodleGetCourseContentsRequest(courseId, ParseMoodleOptions(optionsJson)), ct);

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

    private Task<MoodleGetUserResponse> MissingLookupAsync(string toolName, string expectedArguments)
    {
        logger.LogWarning(
            "Moodle lookup tool {ToolName} was called without a lookup value. Expected one of: {ExpectedArguments}",
            toolName,
            expectedArguments);
        return Task.FromResult(new MoodleGetUserResponse(null));
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
