using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class CoachRepository : GenericRepository<Coach>, ICoachRepository
{
    public CoachRepository(DbContext context) : base(context) { }
}
