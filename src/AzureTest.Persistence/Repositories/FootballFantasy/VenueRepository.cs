using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class VenueRepository : GenericRepository<Venue>, IVenueRepository
{
    public VenueRepository(DbContext dbContext) : base(dbContext)
    {
    }
}
