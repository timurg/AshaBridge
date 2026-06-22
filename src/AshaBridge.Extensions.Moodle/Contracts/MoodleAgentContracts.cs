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
[McpToolDescription("ru", "Найти пользователя Moodle по адресу электронной почты.")]
public sealed record MoodleFindUserByEmailRequest(
    [property: McpParameterDescription("Moodle user email address.")]
    string Email) : IMcpRequest<MoodleFindUserResponse>;

[McpMethod("moodle_user_find_by_id")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Find one Moodle user by id.")]
[McpToolDescription("ru", "Найти пользователя Moodle по внутреннему числовому идентификатору.")]
public sealed record MoodleFindUserByIdRequest(
    [property: McpParameterDescription("Internal numeric Moodle user id.")]
    long Id) : IMcpRequest<MoodleFindUserResponse>;

[McpMethod("moodle_user_find_by_username")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Find one Moodle user by username.")]
[McpToolDescription("ru", "Найти пользователя Moodle по имени пользователя (логину).")]
public sealed record MoodleFindUserByUsernameRequest(
    [property: McpParameterDescription("Moodle username (login).")]
    string Username) : IMcpRequest<MoodleFindUserResponse>;

[McpMethod("moodle_user_update_name")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Update a Moodle user's first and last name.")]
[McpToolDescription("ru", "Изменить имя и фамилию пользователя Moodle.")]
public sealed record MoodleUserUpdateNameRequest(
    [property: McpParameterDescription("Internal numeric Moodle user id.")]
    long Id,
    [property: McpParameterDescription("New first name.")]
    string FirstName,
    [property: McpParameterDescription("New last name.")]
    string LastName) : IMcpRequest<MoodleUpdateUserResponse>;

[McpMethod("moodle_user_update_email")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Update a Moodle user's email.")]
[McpToolDescription("ru", "Изменить адрес электронной почты пользователя Moodle.")]
public sealed record MoodleUserUpdateEmailRequest(
    [property: McpParameterDescription("Internal numeric Moodle user id.")]
    long Id,
    [property: McpParameterDescription("New email address.")]
    string Email) : IMcpRequest<MoodleUpdateUserResponse>;

[McpMethod("moodle_user_update_username")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Update a Moodle user's username.")]
[McpToolDescription("ru", "Изменить имя пользователя (логин) в Moodle.")]
public sealed record MoodleUserUpdateUsernameRequest(
    [property: McpParameterDescription("Internal numeric Moodle user id.")]
    long Id,
    [property: McpParameterDescription("New Moodle username (login).")]
    string Username) : IMcpRequest<MoodleUpdateUserResponse>;

[McpMethod("moodle_user_update_password")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Update a Moodle user's password.")]
[McpToolDescription("ru", "Установить новый пароль пользователя Moodle.")]
public sealed record MoodleUserUpdatePasswordRequest(
    [property: McpParameterDescription("Internal numeric Moodle user id.")]
    long Id,
    [property: McpParameterDescription("New Moodle password.")]
    string Password) : IMcpRequest<MoodleUpdateUserResponse>;

[McpMethod("moodle_user_suspend")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Suspend a Moodle user.")]
[McpToolDescription("ru", "Заблокировать учетную запись пользователя Moodle.")]
public sealed record MoodleUserSuspendRequest(
    [property: McpParameterDescription("Internal numeric Moodle user id.")]
    long Id) : IMcpRequest<MoodleUpdateUserResponse>;

[McpMethod("moodle_user_unsuspend")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Unsuspend a Moodle user.")]
[McpToolDescription("ru", "Разблокировать учетную запись пользователя Moodle.")]
public sealed record MoodleUserUnsuspendRequest(
    [property: McpParameterDescription("Internal numeric Moodle user id.")]
    long Id) : IMcpRequest<MoodleUpdateUserResponse>;

[McpMethod("moodle_user_request_password_reset_by_email")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteLow)]
[DoNotCache]
[McpDescription("Request a Moodle password reset by email.")]
[McpToolDescription("ru", "Запросить сброс пароля Moodle по адресу электронной почты пользователя.")]
public sealed record MoodleRequestPasswordResetByEmailRequest(
    [property: McpParameterDescription("Moodle user email address.")]
    string Email) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_user_request_password_reset_by_username")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.user.write")]
[OperationRisk(OperationRisk.WriteLow)]
[DoNotCache]
[McpDescription("Request a Moodle password reset by username.")]
[McpToolDescription("ru", "Запросить сброс пароля Moodle по имени пользователя (логину).")]
public sealed record MoodleRequestPasswordResetByUsernameRequest(
    [property: McpParameterDescription("Moodle username (login).")]
    string Username) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_user_enrol")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.enrol.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Enrol one Moodle user into one course with an explicit role id.")]
[McpToolDescription("ru", "Зачислить пользователя Moodle на курс с явно указанной ролью.")]
public sealed record MoodleUserEnrolRequest(
    [property: McpParameterDescription("Moodle role id for the enrolment.")]
    long RoleId,
    [property: McpParameterDescription("Internal numeric Moodle user id.")]
    long UserId,
    [property: McpParameterDescription("Internal numeric Moodle course id.")]
    long CourseId) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_user_enrol_as_student")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.enrol.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Enrol one Moodle user into one course as a student.")]
[McpToolDescription("ru", "Зачислить пользователя Moodle на курс в роли студента.")]
public sealed record MoodleUserEnrolAsStudentRequest(
    [property: McpParameterDescription("Internal numeric Moodle user id.")]
    long UserId,
    [property: McpParameterDescription("Internal numeric Moodle course id.")]
    long CourseId) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_course_get_by_id")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.course.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Get one Moodle course by its internal numeric course id.")]
[McpToolDescription("ru", "Получить курс Moodle по внутреннему числовому идентификатору курса.")]
public sealed record MoodleCourseGetByIdRequest(
    [property: McpParameterDescription("Internal numeric Moodle course id, not the course idnumber field.")]
    long Id) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_course_find_by_shortname")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.course.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Find Moodle courses by shortname.")]
[McpToolDescription("ru", "Найти курсы Moodle по краткому названию.")]
public sealed record MoodleCourseFindByShortNameRequest(
    [property: McpParameterDescription("Moodle course short name.")]
    string ShortName) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_course_find_by_idnumber")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.course.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Find Moodle courses by the optional course idnumber field, not by internal course id.")]
[McpToolDescription("ru", "Найти курсы Moodle по значению поля idnumber, а не по внутреннему идентификатору.")]
public sealed record MoodleCourseFindByIdNumberRequest(
    [property: McpParameterDescription("Value stored in the Moodle course idnumber field.")]
    string IdNumber) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_courses_find_by_category")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.course.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Find Moodle courses by category id.")]
[McpToolDescription("ru", "Найти курсы Moodle в указанной категории.")]
public sealed record MoodleCoursesFindByCategoryRequest(
    [property: McpParameterDescription("Moodle course category id.")]
    string Category) : IMcpRequest<MoodleRawResponse>;

[McpMethod("moodle_course_get_contents")]
[ContractVersion("1.0.0")]
[RequiresPermission("moodle.course.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 300, Scope = CacheScope.Organization)]
[McpDescription("Get Moodle course contents without extra options.")]
[McpToolDescription("ru", "Получить содержимое курса Moodle: разделы, модули и учебные материалы.")]
public sealed record MoodleCourseGetContentsRequest(
    [property: McpParameterDescription("Internal numeric Moodle course id.")]
    long CourseId) : IMcpRequest<MoodleRawResponse>;
