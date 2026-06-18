using AshaBridge.AspNetCore.Extensions;
using AshaBridge.Core.Registry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AshaBridge.IntegrationTests;

public sealed class MoodleContractRegistrationTests
{
    [Fact]
    public void MoodleExtension_RegistersOnlyAgentTools()
    {
        var config = IntegrationTestConfig.Load();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(config.AppSettingsPath, optional: false, reloadOnChange: false)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddAshaBridge(configuration);

        using var provider = services.BuildServiceProvider(validateScopes: true);
        var methods = provider.GetRequiredService<MethodRegistry>()
            .Methods
            .Where(method => method.ExtensionId == "ashabridge.extensions.moodle")
            .Select(method => method.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        var expected = new[]
        {
            "moodle_course_find_by_idnumber",
            "moodle_course_find_by_shortname",
            "moodle_course_get_by_id",
            "moodle_course_get_contents",
            "moodle_courses_find_by_category",
            "moodle_user_enrol",
            "moodle_user_enrol_as_student",
            "moodle_user_find_by_email",
            "moodle_user_find_by_id",
            "moodle_user_find_by_username",
            "moodle_user_request_password_reset_by_email",
            "moodle_user_request_password_reset_by_username",
            "moodle_user_suspend",
            "moodle_user_unsuspend",
            "moodle_user_update_email",
            "moodle_user_update_name",
            "moodle_user_update_password",
            "moodle_user_update_username"
        };

        Assert.Equal(expected.OrderBy(name => name, StringComparer.Ordinal), methods);
    }
}
