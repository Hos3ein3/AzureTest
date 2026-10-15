using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class PredictClubMemberRepository : GenericRepository<PredictClubMember>, IPredictClubMemberRepository
{
    public PredictClubMemberRepository(DbContext dbContext) : base(dbContext) { }
}
