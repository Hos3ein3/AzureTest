namespace AzureTest.Domain.Primitives;

/// <summary>
/// Marker interface for entities or MongoDB documents that represent singleton/single-record settings.
/// Prevents inserting a new record if a record already exists for the entity or tenant workspace.
/// </summary>
public interface ISingleRecordSetting
{
}
