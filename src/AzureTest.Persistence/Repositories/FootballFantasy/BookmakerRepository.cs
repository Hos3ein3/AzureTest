using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class BookmakerRepository : GenericRepository<Bookmaker>, IBookmakerRepository
{
    public BookmakerRepository(DbContext dbContext) : base(dbContext) { }
}
