namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// One setting the operator changed from its default.
/// </summary>
public sealed class SettingEntity
{

    public required string Key { get; set; }

    public required string Value { get; set; }

}
