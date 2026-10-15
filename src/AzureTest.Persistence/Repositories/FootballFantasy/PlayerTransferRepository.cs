using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class PlayerTransferRepository : GenericRepository<PlayerTransfer>, IPlayerTransferRepository
{
    public PlayerTransferRepository(DbContext dbContext) : base(dbContext) { }
}
