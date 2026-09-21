using AzureTest.Domain.Primitives;

namespace AzureTest.Application.Interfaces.Data;

public interface IRepositoryFactory
{
    IGenericRepository<TEntity> Instantiate<TEntity>() where TEntity : BaseEntity;
}
