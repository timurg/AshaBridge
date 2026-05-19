using System.Text.Json.Nodes;
using AshaBridge.Extensions.Bitrix24.Contracts;
using Xunit.Abstractions;

namespace AshaBridge.IntegrationTests;

public sealed class Bitrix24RealRequestTests : IClassFixture<AshaBridgeRuntimeFixture>
{
    private readonly AshaBridgeRuntimeFixture fixture;

    public Bitrix24RealRequestTests(AshaBridgeRuntimeFixture fixture, ITestOutputHelper output)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task CrmDealList_UsesConfiguredWebhook()
    {
        var response = await fixture.InvokeAsync(
            new BitrixCrmDealListRequest(null));

        Assert.NotNull(response.Deals);
    }

    [Fact]
    public async Task CrmContactList_UsesConfiguredWebhook()
    {
        var response = await fixture.InvokeAsync(
            new BitrixCrmContactListRequest(null));

        Assert.NotNull(response.Contacts);
    }

    [Fact]
    public async Task CrmItemList_UsesConfiguredWebhook_WhenEntityTypeIdIsConfigured()
    {
        var entityTypeId = fixture.Config.BitrixEntityTypeId;
        Assert.True(entityTypeId > 0, "Set integrationTests:bitrix:entityTypeId to test bitrix_crm_item_list through AshaBridge contract.");

        var response = await fixture.InvokeAsync(
            new BitrixCrmItemListRequest(entityTypeId, null));

        Assert.NotNull(response.Items);
    }

    [Fact]
    public async Task CrmDealGet_UsesConfiguredWebhook_WhenDealIdIsConfigured()
    {
        var dealId = fixture.Config.BitrixDealId;
        Assert.True(dealId > 0, "Set integrationTests:bitrix:dealId to test bitrix_crm_deal_get through AshaBridge contract.");

        var response = await fixture.InvokeAsync(
            new BitrixCrmDealGetRequest(dealId));

        Assert.NotNull(response.Deal);
    }

    [Fact]
    public async Task CrmContactGet_UsesConfiguredWebhook_WhenContactIdIsConfigured()
    {
        var contactId = await ResolveContactIdAsync();
        Assert.True(contactId > 0, "Set integrationTests:bitrix:contactId to test bitrix_crm_contact_get through AshaBridge contract.");

        var response = await fixture.InvokeAsync(
            new BitrixCrmContactGetRequest(contactId));

        Assert.NotNull(response.Contact);
    }

    [Fact]
    public async Task CrmItemGet_UsesConfiguredWebhook_WhenItemIdIsConfigured()
    {
        var entityTypeId = fixture.Config.BitrixEntityTypeId;
        var itemId = fixture.Config.BitrixItemId;
        Assert.True(entityTypeId > 0 && itemId > 0, "Set integrationTests:bitrix:entityTypeId and itemId to test bitrix_crm_item_get through AshaBridge contract.");

        var response = await fixture.InvokeAsync(
            new BitrixCrmItemGetRequest(entityTypeId, itemId));

        Assert.NotNull(response.Item);
    }

    [Fact]
    public async Task CrmTimelineCommentAdd_UsesConfiguredWebhook_WhenWritesAreEnabled()
    {
        if (!fixture.Config.AllowWrites || fixture.Config.BitrixTimelineEntityId <= 0)
        {
            Assert.Fail("Set integrationTests:allowWrites=true and bitrix:timelineEntityId to test bitrix_crm_timeline_comment_add.");
        }

        var response = await fixture.InvokeAsync(
            new BitrixCrmTimelineCommentAddRequest(
                fixture.Config.BitrixTimelineEntityType,
                fixture.Config.BitrixTimelineEntityId,
                $"AshaBridge integration test {DateTimeOffset.UtcNow:O}"));

        Assert.True(response.Id > 0);
    }

    [Fact]
    public async Task CrmItemUpdate_UsesConfiguredWebhook_WhenWritesAreEnabled()
    {
        if (!fixture.Config.AllowWrites || fixture.Config.BitrixEntityTypeId <= 0 || fixture.Config.BitrixItemId <= 0)
        {
            Assert.Fail("Set integrationTests:allowWrites=true, bitrix:entityTypeId, and bitrix:itemId to test bitrix_crm_item_update.");
        }

        var response = await fixture.InvokeAsync(
            new BitrixCrmItemUpdateRequest(fixture.Config.BitrixEntityTypeId, fixture.Config.BitrixItemId, new JsonObject()));

        Assert.True(response.Success);
    }

    private async Task<long> ResolveContactIdAsync()
    {
        if (fixture.Config.BitrixContactId > 0)
        {
            return fixture.Config.BitrixContactId;
        }

        var contacts = await fixture.InvokeAsync(new BitrixCrmContactListRequest(null));
        return contacts.Contacts
            .Select(contact => contact?["ID"]?.GetValue<string>())
            .Select(id => long.TryParse(id, out var parsed) ? parsed : 0)
            .FirstOrDefault(id => id > 0);
    }
}

