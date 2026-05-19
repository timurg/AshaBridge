using System.Text.Json.Nodes;
using AshaBridge.Sdk.Attributes;
using AshaBridge.Sdk.Contracts;

namespace AshaBridge.Extensions.Moodle.Contracts;

[McpMethod("moodle_core_user_get_users_by_field")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Get Moodle users by id, idnumber, username, or email.")]
public sealed record MoodleGetUsersByFieldRequest(
    [property: McpParameterDescription("Moodle user lookup field: Id, IdNumber, Username, or Email.")]
    MoodleUserLookupField Field,

    [property: McpParameterDescription("One or more lookup values for the selected Moodle user field.")]
    IReadOnlyList<string> Values) : IMcpRequest<MoodleGetUsersByFieldResponse>;

public enum MoodleUserLookupField
{
    Id,
    IdNumber,
    Username,
    Email
}

public sealed record MoodleUser(long Id, string? Username, string? Email, string? FullName);

public sealed record MoodleGetUsersByFieldResponse(IReadOnlyList<MoodleUser> Users);

[McpMethod("moodle_core_user_create_user")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Create one Moodle user. Email is always used as both username and email.")]
public sealed record MoodleCreateUserRequest(
    [property: McpParameterDescription("Moodle user email address. This is also used as username.")]
    string Email,

    [property: McpParameterDescription("Temporary random Moodle password.")]
    string Password,

    [property: McpParameterDescription("Moodle first name.")]
    string FirstName,

    [property: McpParameterDescription("Moodle last name.")]
    string LastName) : IMcpRequest<MoodleCreateUserResponse>;

public sealed record MoodleCreateUserResponse(long Id, string Username);

[McpMethod("moodle_core_user_update_user")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Update one Moodle user. This is a single-user wrapper around Moodle core_user_update_users.")]
public sealed record MoodleUpdateUserRequest(
    [property: McpParameterDescription("Moodle user id.")]
    long Id,

    string? Username,
    string? Auth,
    bool? Suspended,
    string? Password,
    string? FirstName,
    string? LastName,
    string? Email,
    int? MailDisplay,
    string? City,
    string? Country,
    string? Timezone,
    string? Description,
    string? IdNumber,
    string? Institution,
    string? Department,
    string? Phone1,
    string? Phone2,
    string? Address,
    string? Lang) : IMcpRequest<MoodleUpdateUserResponse>;

public sealed record MoodleUpdateUserResponse(bool Success);

[McpMethod("moodle_core_user_get_user")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Find one Moodle user by one criterion such as id, username, email, firstname, lastname, idnumber, phone1, or phone2.")]
public sealed record MoodleGetUserRequest(
    [property: McpParameterDescription("Moodle user search field.")]
    string Key,

    [property: McpParameterDescription("Moodle user search value.")]
    string Value) : IMcpRequest<MoodleGetUserResponse>;

public sealed record MoodleGetUserResponse(MoodleUser? User);

[McpMethod("moodle_core_auth_request_password_reset")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteLow)]
[DoNotCache]
[McpDescription("Request a Moodle password reset by username or email.")]
public sealed record MoodleRequestPasswordResetRequest(string? Username, string? Email) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_core_enrol_get_users_courses")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.course.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Get Moodle courses for a user.")]
public sealed record MoodleGetUsersCoursesRequest(
    [property: CacheKey]
    [property: McpParameterDescription("Moodle user id.")]
    long UserId) : IMcpRequest<MoodleGetUsersCoursesResponse>;

public sealed record MoodleCourse(long Id, string? ShortName, string? FullName);

public sealed record MoodleGetUsersCoursesResponse(IReadOnlyList<MoodleCourse> Courses);

[McpMethod("moodle_enrol_manual_enrol_user")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.enrol.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Manually enrol one Moodle user into one course.")]
public sealed record MoodleManualEnrolUserRequest(
    long RoleId,
    long UserId,
    long CourseId,
    long? TimeStart,
    long? TimeEnd,
    int? Suspend) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_core_completion_get_activities_completion_status")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.progress.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 120, Scope = CacheScope.Organization)]
[McpDescription("Get Moodle activity completion status for a course and user.")]
public sealed record MoodleGetActivitiesCompletionStatusRequest(
    [property: CacheKey]
    [property: McpParameterDescription("Moodle course id.")]
    long CourseId,

    [property: CacheKey]
    [property: McpParameterDescription("Moodle user id.")]
    long UserId) : IMcpRequest<MoodleGetActivitiesCompletionStatusResponse>;

public sealed record MoodleActivityCompletion(long Cmid, int State, string? TimeCompleted);

public sealed record MoodleGetActivitiesCompletionStatusResponse(IReadOnlyList<MoodleActivityCompletion> Statuses);

[McpMethod("moodle_core_completion_get_course_completion_status")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.progress.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 120, Scope = CacheScope.Organization)]
[McpDescription("Get Moodle course completion status for a course and user.")]
public sealed record MoodleGetCourseCompletionStatusRequest(long CourseId, long UserId) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_core_competency_list_user_plans")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.progress.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 120, Scope = CacheScope.User)]
[McpDescription("List a Moodle user's competency learning plans.")]
public sealed record MoodleListUserPlansRequest(long UserId) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_gradereport_user_get_grade_items")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.grade.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 120, Scope = CacheScope.Organization)]
[McpDescription("Get Moodle user grade items.")]
public sealed record MoodleGetGradeItemsRequest(
    [property: CacheKey]
    [property: McpParameterDescription("Moodle course id.")]
    long CourseId,

    [property: CacheKey]
    [property: McpParameterDescription("Moodle user id.")]
    long UserId) : IMcpRequest<MoodleGetGradeItemsResponse>;

public sealed record MoodleGradeItem(string? ItemName, string? GradeRaw, string? GradeFormatted);

public sealed record MoodleGetGradeItemsResponse(IReadOnlyList<MoodleGradeItem> Items);

[McpMethod("moodle_core_course_get_courses")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.course.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Get Moodle course details, optionally by ids.")]
public sealed record MoodleGetCoursesRequest(IReadOnlyList<long>? Ids) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_core_course_get_courses_by_field")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.course.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Get Moodle courses matching a field such as id, ids, shortname, idnumber, or category.")]
public sealed record MoodleGetCoursesByFieldRequest(string? Field, string? Value) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_core_course_get_contents")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.course.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Get Moodle course contents.")]
public sealed record MoodleGetCourseContentsRequest(
    long CourseId,
    IReadOnlyList<MoodleNameValueOption>? Options) : IMcpRequest<MoodleRawResponse>;

public sealed record MoodleNameValueOption(string Name, string Value);

public sealed record MoodleRawResponse(JsonNode? Data);
