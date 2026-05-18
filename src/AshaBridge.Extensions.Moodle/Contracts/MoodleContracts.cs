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
