using AshaBridge.Sdk.Attributes;
using AshaBridge.Sdk.Contracts;

namespace AshaBridge.Extensions.Moodle.Contracts;

public sealed record MoodleFindUserResponse(bool Found, string Field, string Value, MoodleUser? User);

[McpMethod("moodle_user_find_by_email")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Find one Moodle user by email.")]
public sealed record MoodleFindUserByEmailRequest(string Email) : IMcpRequest<MoodleFindUserResponse>;

[McpMethod("moodle_user_find_by_id")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Find one Moodle user by id.")]
public sealed record MoodleFindUserByIdRequest(long Id) : IMcpRequest<MoodleFindUserResponse>;

[McpMethod("moodle_user_find_by_username")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Find one Moodle user by username.")]
public sealed record MoodleFindUserByUsernameRequest(string Username) : IMcpRequest<MoodleFindUserResponse>;

[McpMethod("moodle_user_update_name")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Update a Moodle user's first and last name.")]
public sealed record MoodleUserUpdateNameRequest(long Id, string FirstName, string LastName) : IMcpRequest<MoodleUpdateUserResponse>;

[McpMethod("moodle_user_update_email")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Update a Moodle user's email.")]
public sealed record MoodleUserUpdateEmailRequest(long Id, string Email) : IMcpRequest<MoodleUpdateUserResponse>;

[McpMethod("moodle_user_update_username")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Update a Moodle user's username.")]
public sealed record MoodleUserUpdateUsernameRequest(long Id, string Username) : IMcpRequest<MoodleUpdateUserResponse>;

[McpMethod("moodle_user_update_password")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Update a Moodle user's password.")]
public sealed record MoodleUserUpdatePasswordRequest(long Id, string Password) : IMcpRequest<MoodleUpdateUserResponse>;

[McpMethod("moodle_user_suspend")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Suspend a Moodle user.")]
public sealed record MoodleUserSuspendRequest(long Id) : IMcpRequest<MoodleUpdateUserResponse>;

[McpMethod("moodle_user_unsuspend")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Unsuspend a Moodle user.")]
public sealed record MoodleUserUnsuspendRequest(long Id) : IMcpRequest<MoodleUpdateUserResponse>;

[McpMethod("moodle_user_request_password_reset_by_email")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteLow)]
[DoNotCache]
[McpDescription("Request a Moodle password reset by email.")]
public sealed record MoodleRequestPasswordResetByEmailRequest(string Email) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_user_request_password_reset_by_username")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteLow)]
[DoNotCache]
[McpDescription("Request a Moodle password reset by username.")]
public sealed record MoodleRequestPasswordResetByUsernameRequest(string Username) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_user_enrol")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.enrol.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Enrol one Moodle user into one course with an explicit role id.")]
public sealed record MoodleUserEnrolRequest(long RoleId, long UserId, long CourseId) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_user_enrol_as_student")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.enrol.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Enrol one Moodle user into one course as a student.")]
public sealed record MoodleUserEnrolAsStudentRequest(long UserId, long CourseId) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_course_get_by_id")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.course.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Get one Moodle course by id.")]
public sealed record MoodleCourseGetByIdRequest(long Id) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_course_find_by_shortname")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.course.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Find Moodle courses by shortname.")]
public sealed record MoodleCourseFindByShortNameRequest(string ShortName) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_course_find_by_idnumber")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.course.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Find Moodle courses by idnumber.")]
public sealed record MoodleCourseFindByIdNumberRequest(string IdNumber) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_courses_find_by_category")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.course.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Find Moodle courses by category id.")]
public sealed record MoodleCoursesFindByCategoryRequest(string Category) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_course_get_contents")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.course.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Get Moodle course contents without extra options.")]
public sealed record MoodleCourseGetContentsRequest(long CourseId) : IMcpRequest<MoodleRawResponse>;
