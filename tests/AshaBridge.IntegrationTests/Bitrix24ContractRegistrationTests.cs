using AshaBridge.AspNetCore.Extensions;
using AshaBridge.Core.Registry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AshaBridge.IntegrationTests;

public sealed class Bitrix24ContractRegistrationTests
{
    [Fact]
    public void BitrixExtension_RegistersDealPartyContactAndTrainingDirectionTools()
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
            .Select(method => method.Name)
            .ToArray();

        Assert.Contains("bitrix_crm_contact_update", methods);
        Assert.Contains("bitrix_crm_deal_training_direction_update", methods);
        Assert.Contains("bitrix_crm_deal_party_email_add", methods);
    }
}
