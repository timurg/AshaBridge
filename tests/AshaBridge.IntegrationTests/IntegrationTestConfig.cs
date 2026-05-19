using System.Text.Json;

namespace AshaBridge.IntegrationTests;

public sealed class IntegrationTestConfig
{
    private readonly JsonElement root;

    private IntegrationTestConfig(string appSettingsPath, JsonElement root)
    {
        AppSettingsPath = appSettingsPath;
        this.root = root;
    }

    public static IntegrationTestConfig Load()
    {
        var path = FindAppSettings();
        using var stream = File.OpenRead(path);
        using var document = JsonDocument.Parse(stream, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        return new IntegrationTestConfig(path, document.RootElement.Clone());
    }

    public string AppSettingsPath { get; }

    public string BitrixWebhookUrl => RequireString("bitrix", "instances", BitrixDefaultInstance, "webhookUrl");

    public string BitrixDefaultInstance => RequireString("bitrix", "defaultInstance");

    public string MoodleBaseUrl => RequireString("moodle", "instances", MoodleDefaultInstance, "baseUrl");

    public string MoodleToken => RequireString("moodle", "instances", MoodleDefaultInstance, "token");

    public string MoodleDefaultInstance => RequireString("moodle", "defaultInstance");

    public bool AllowWrites => OptionalBool(true, "integrationTests", "allowWrites");

    public int BitrixEntityTypeId => OptionalPositiveInt(174, "integrationTests", "bitrix", "entityTypeId");

    public long BitrixLeadId => OptionalPositiveLong(0, "integrationTests", "bitrix", "leadId");

    public long BitrixItemId => OptionalPositiveLong(5729, "integrationTests", "bitrix", "itemId");

    public long BitrixDealId => OptionalPositiveLong(0, "integrationTests", "bitrix", "dealId");

    public long BitrixContactId => OptionalPositiveLong(0, "integrationTests", "bitrix", "contactId");

    public string BitrixTimelineEntityType => OptionalString("deal", "integrationTests", "bitrix", "timelineEntityType");

    public long BitrixTimelineEntityId => OptionalPositiveLong(1126, "integrationTests", "bitrix", "timelineEntityId");

    public long MoodleUserId => OptionalPositiveLong(0, "integrationTests", "moodle", "userId");

    public long MoodleCourseId => OptionalPositiveLong(0, "integrationTests", "moodle", "courseId");

    public long MoodleStudentRoleId => OptionalPositiveLong(5, "integrationTests", "moodle", "studentRoleId");

    public string MoodleUserLookupField => OptionalString("id", "integrationTests", "moodle", "userLookupField");

    public string MoodleUserLookupValue => OptionalString("", "integrationTests", "moodle", "userLookupValue");

    public IReadOnlyList<string> AllPermissions =>
        ReadStringArray("security", "user", "permissions")
            .Concat(ReadStringArray("security", "serviceTokens", "0", "permissions"))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static string FindAppSettings()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "src", "AshaBridge.Api", "appsettings.json");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException("Could not find src/AshaBridge.Api/appsettings.json.");
    }

    private string RequireString(params string[] path)
    {
        var value = OptionalString("", path);
        Assert.False(string.IsNullOrWhiteSpace(value), $"Missing appsettings.json value: {string.Join(':', path)}");
        return value;
    }

    private string OptionalString(string fallback, params string[] path)
    {
        return TryGet(path, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? fallback
            : fallback;
    }

    private int OptionalInt(int fallback, params string[] path)
    {
        return TryGet(path, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : fallback;
    }

    private int OptionalPositiveInt(int fallback, params string[] path)
    {
        var value = OptionalInt(fallback, path);
        return value > 0 ? value : fallback;
    }

    private long OptionalLong(long fallback, params string[] path)
    {
        return TryGet(path, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number)
            ? number
            : fallback;
    }

    private long OptionalPositiveLong(long fallback, params string[] path)
    {
        var value = OptionalLong(fallback, path);
        return value > 0 ? value : fallback;
    }

    private bool OptionalBool(bool fallback, params string[] path)
    {
        return TryGet(path, out var value) && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
            ? value.GetBoolean()
            : fallback;
    }

    private bool TryGet(IReadOnlyList<string> path, out JsonElement value)
    {
        value = root;
        foreach (var segment in path)
        {
            if (value.ValueKind == JsonValueKind.Array && int.TryParse(segment, out var index))
            {
                if (index < 0 || index >= value.GetArrayLength())
                {
                    return false;
                }

                value = value[index];
                continue;
            }

            if (value.ValueKind != JsonValueKind.Object || !value.TryGetProperty(segment, out value))
            {
                return false;
            }
        }

        return true;
    }

    private IReadOnlyList<string> ReadStringArray(params string[] path)
    {
        if (!TryGet(path, out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return value.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString())
            .OfType<string>()
            .ToArray();
    }
}
