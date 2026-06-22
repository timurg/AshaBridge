using System.Text.Json.Nodes;
using AshaBridge.Sdk.Attributes;
using AshaBridge.Sdk.Contracts;

namespace AshaBridge.Extensions.Bitrix24.Contracts;

[McpMethod("bitrix_crm_item_get")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.item.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 60, Scope = CacheScope.Organization)]
[McpDescription("Get Bitrix24 CRM dynamic item by entity type id and item id.")]
[McpToolDescription("ru", "Получить элемент смарт-процесса Bitrix24 по идентификатору типа сущности и идентификатору элемента.")]
public sealed record BitrixCrmItemGetRequest(
    [property: CacheKey]
    [property: McpParameterDescription("Bitrix24 CRM dynamic entity type id.")]
    int EntityTypeId,

    [property: CacheKey]
    [property: McpParameterDescription("Bitrix24 CRM dynamic item id.")]
    long Id) : IMcpRequest<BitrixCrmItemGetResponse>;

public sealed record BitrixCrmItemGetResponse(JsonObject Item);

[McpMethod("bitrix_crm_item_list")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.item.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 60, Scope = CacheScope.Organization)]
[McpDescription("List Bitrix24 CRM dynamic items.")]
public sealed record BitrixCrmItemListRequest(
    [property: CacheKey]
    [property: McpParameterDescription("Bitrix24 CRM dynamic entity type id.")]
    int EntityTypeId,

    [property: McpParameterDescription("Optional Bitrix24 crm.item.list filter object.")]
    JsonObject? Filter) : IMcpRequest<BitrixCrmItemListResponse>;

public sealed record BitrixCrmItemListResponse(JsonArray Items);

[McpMethod("bitrix_crm_dynamic_items_list_all")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.item.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 60, Scope = CacheScope.Organization)]
[McpDescription("List all Bitrix24 CRM dynamic items for an entity type.")]
[McpToolDescription("ru", "Получить все элементы указанного смарт-процесса Bitrix24.")]
public sealed record BitrixCrmDynamicItemsListAllRequest(
    [property: CacheKey]
    [property: McpParameterDescription("Bitrix24 CRM dynamic entity type id.")]
    int EntityTypeId) : IMcpRequest<BitrixCrmItemListResponse>;

[McpMethod("bitrix_crm_item_update")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.item.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[InvalidatesCache("bitrix:crm.item:{EntityTypeId}:{Id}")]
[McpDescription("Update Bitrix24 CRM dynamic item fields.")]
public sealed record BitrixCrmItemUpdateRequest(
    [property: CacheKey]
    [property: McpParameterDescription("Bitrix24 CRM dynamic entity type id.")]
    int EntityTypeId,

    [property: CacheKey]
    [property: McpParameterDescription("Bitrix24 CRM dynamic item id to update.")]
    long Id,

    [property: McpParameterDescription("Bitrix24 CRM fields object to pass to crm.item.update.")]
    JsonObject Fields) : IMcpRequest<BitrixCrmItemUpdateResponse>;

public sealed record BitrixCrmItemUpdateResponse(bool Success);

[McpMethod("bitrix_crm_deal_get")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.deal.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 60, Scope = CacheScope.Organization)]
[McpDescription("Get Bitrix24 CRM deal by id.")]
[McpToolDescription("ru", "Получить сделку Bitrix24 по ее идентификатору.")]
public sealed record BitrixCrmDealGetRequest(
    [property: CacheKey]
    [property: McpParameterDescription("Bitrix24 CRM deal id.")]
    long Id) : IMcpRequest<BitrixCrmDealGetResponse>;

public sealed record BitrixCrmDealGetResponse(JsonObject Deal);

[McpMethod("bitrix_crm_deal_list")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.deal.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 60, Scope = CacheScope.Organization)]
[McpDescription("List Bitrix24 CRM deals.")]
public sealed record BitrixCrmDealListRequest(
    [property: McpParameterDescription("Optional Bitrix24 crm.deal.list filter object.")]
    JsonObject? Filter) : IMcpRequest<BitrixCrmDealListResponse>;

public sealed record BitrixCrmDealListResponse(JsonArray Deals);

[McpMethod("bitrix_crm_deals_list_all")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.deal.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 60, Scope = CacheScope.Organization)]
[McpDescription("List all Bitrix24 CRM deals.")]
[McpToolDescription("ru", "Получить список всех сделок Bitrix24.")]
public sealed record BitrixCrmDealsListAllRequest() : IMcpRequest<BitrixCrmDealListResponse>;

[McpMethod("bitrix_crm_deals_find_by_contact_id")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.deal.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 60, Scope = CacheScope.Organization)]
[McpDescription("Find Bitrix24 CRM deals by contact id.")]
[McpToolDescription("ru", "Найти сделки Bitrix24, связанные с указанным контактом.")]
public sealed record BitrixCrmDealsFindByContactIdRequest(
    [property: CacheKey]
    [property: McpParameterDescription("Bitrix24 CRM contact id.")]
    long ContactId) : IMcpRequest<BitrixCrmDealListResponse>;

[McpMethod("bitrix_crm_contact_get")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.contact.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 60, Scope = CacheScope.Organization)]
[McpDescription("Get Bitrix24 CRM contact by id.")]
[McpToolDescription("ru", "Получить контакт Bitrix24 по его идентификатору.")]
public sealed record BitrixCrmContactGetRequest(
    [property: CacheKey]
    [property: McpParameterDescription("Bitrix24 CRM contact id.")]
    long Id) : IMcpRequest<BitrixCrmContactGetResponse>;

public sealed record BitrixCrmContactGetResponse(JsonObject Contact);

[McpMethod("bitrix_crm_contact_list")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.contact.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 60, Scope = CacheScope.Organization)]
[McpDescription("List Bitrix24 CRM contacts.")]
public sealed record BitrixCrmContactListRequest(
    [property: McpParameterDescription("Optional Bitrix24 crm.contact.list filter object.")]
    JsonObject? Filter) : IMcpRequest<BitrixCrmContactListResponse>;

public sealed record BitrixCrmContactListResponse(JsonArray Contacts);

[McpMethod("bitrix_crm_contacts_list_all")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.contact.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 60, Scope = CacheScope.Organization)]
[McpDescription("List all Bitrix24 CRM contacts.")]
[McpToolDescription("ru", "Получить список всех контактов Bitrix24.")]
public sealed record BitrixCrmContactsListAllRequest() : IMcpRequest<BitrixCrmContactListResponse>;

[McpMethod("bitrix_crm_contacts_find_by_email")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.contact.read")]
[OperationRisk(OperationRisk.Read)]
[Cacheable(TtlSeconds = 60, Scope = CacheScope.Organization)]
[McpDescription("Find Bitrix24 CRM contacts by email.")]
[McpToolDescription("ru", "Найти контакты Bitrix24 по адресу электронной почты.")]
public sealed record BitrixCrmContactsFindByEmailRequest(
    [property: CacheKey]
    [property: McpParameterDescription("Contact email address.")]
    string Email) : IMcpRequest<BitrixCrmContactListResponse>;

[McpMethod("bitrix_crm_contact_update")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.contact.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[InvalidatesCache("bitrix:crm.contact:{Id}")]
[McpDescription("Update Bitrix24 CRM contact name fields and email. Only provided fields are changed.")]
public sealed record BitrixCrmContactUpdateRequest(
    [property: CacheKey]
    [property: McpParameterDescription("Bitrix24 CRM contact id to update.")]
    long Id,

    [property: McpParameterDescription("Optional contact first name.")]
    string? Name,

    [property: McpParameterDescription("Optional contact last name.")]
    string? LastName,

    [property: McpParameterDescription("Optional contact middle name.")]
    string? MiddleName,

    [property: McpParameterDescription("Optional contact email.")]
    string? Email) : IMcpRequest<BitrixCrmContactUpdateResponse>;

public sealed record BitrixCrmContactUpdateResponse(bool Success, string Message);

[McpMethod("bitrix_crm_contact_update_name")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.contact.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[InvalidatesCache("bitrix:crm.contact:{Id}")]
[McpDescription("Update Bitrix24 CRM contact name fields.")]
[McpToolDescription("ru", "Изменить имя, фамилию и отчество контакта Bitrix24.")]
public sealed record BitrixCrmContactUpdateNameRequest(
    [property: CacheKey]
    [property: McpParameterDescription("Bitrix24 CRM contact id to update.")]
    long Id,

    [property: McpParameterDescription("Contact first name.")]
    string Name,

    [property: McpParameterDescription("Optional contact last name.")]
    string? LastName,

    [property: McpParameterDescription("Optional contact middle name.")]
    string? MiddleName) : IMcpRequest<BitrixCrmContactUpdateResponse>;

[McpMethod("bitrix_crm_contact_update_email")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.contact.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[InvalidatesCache("bitrix:crm.contact:{Id}")]
[McpDescription("Update Bitrix24 CRM contact email.")]
[McpToolDescription("ru", "Изменить адрес электронной почты контакта Bitrix24.")]
public sealed record BitrixCrmContactUpdateEmailRequest(
    [property: CacheKey]
    [property: McpParameterDescription("Bitrix24 CRM contact id to update.")]
    long Id,

    [property: McpParameterDescription("Contact email address.")]
    string Email) : IMcpRequest<BitrixCrmContactUpdateResponse>;

[McpMethod("bitrix_crm_deal_training_direction_update")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.deal.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[InvalidatesCache("bitrix:crm.deal:{Id}")]
[McpDescription("Update Bitrix24 CRM deal training direction field UF_CRM_6283BEE95507A.")]
[McpToolDescription("ru", "Изменить направление обучения в сделке Bitrix24.")]
public sealed record BitrixCrmDealTrainingDirectionUpdateRequest(
    [property: CacheKey]
    [property: McpParameterDescription("Bitrix24 CRM deal id to update.")]
    long Id,

    [property: McpParameterDescription("Training direction value.")]
    string Direction) : IMcpRequest<BitrixCrmDealTrainingDirectionUpdateResponse>;

public sealed record BitrixCrmDealTrainingDirectionUpdateResponse(bool Success, string Message);

[McpMethod("bitrix_crm_deal_party_email_add")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.crm.deal.read")]
[RequiresPermission("bitrix.crm.contact.read")]
[RequiresPermission("bitrix.crm.activity.write")]
[RequiresPermission("bitrix.user.read")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Add an outgoing email CRM activity on a Bitrix24 deal for the student, student curator, or manager.")]
[McpToolDescription("ru", "Добавить в сделку Bitrix24 исходящее письмо студенту, куратору студента или менеджеру.")]
public sealed record BitrixCrmDealPartyEmailAddRequest(
    [property: McpParameterDescription("Bitrix24 CRM deal id.")]
    long DealId,

    [property: McpParameterDescription("Recipient: student, student_curator, or manager. The tool resolves the email address.")]
    string Recipient,

    [property: McpParameterDescription("Email subject.")]
    string Subject,

    [property: McpParameterDescription("Email body.")]
    string Body,

    [property: McpParameterDescription("Whether the body is HTML. Defaults to true.")]
    bool IsHtml = true,

    [property: McpParameterDescription("Do not copy the sender. Defaults to false.")]
    bool DisableCopyToSelf = false) : IMcpRequest<BitrixCrmDealPartyEmailAddResponse>;

public sealed record BitrixCrmDealPartyEmailAddResponse(bool Success, long ActivityId, string Recipient);

[McpMethod("bitrix_crm_timeline_comment_add")]
[ContractVersion("1.0.0")]
[RequiresPermission("bitrix.timeline.write")]
[OperationRisk(OperationRisk.WriteMedium)]
[RequiresIdempotency]
[DoNotCache]
[McpDescription("Add a Bitrix24 CRM timeline comment.")]
[McpToolDescription("ru", "Добавить комментарий в таймлайн сущности Bitrix24, например сделки или контакта.")]
public sealed record BitrixCrmTimelineCommentAddRequest(
    [property: McpParameterDescription("Bitrix24 CRM timeline entity type, for example deal or contact.")]
    string EntityType,

    [property: McpParameterDescription("Bitrix24 CRM entity id to attach the timeline comment to.")]
    long EntityId,

    [property: McpParameterDescription("Timeline comment text.")]
    string Comment) : IMcpRequest<BitrixCrmTimelineCommentAddResponse>;

public sealed record BitrixCrmTimelineCommentAddResponse(long Id);
