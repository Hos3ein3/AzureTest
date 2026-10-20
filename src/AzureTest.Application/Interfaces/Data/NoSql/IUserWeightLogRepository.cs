using AzureTest.Domain.Documents.Gym;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IUserWeightLogRepository : IDocumentRepository<UserWeightLog>
{
}
