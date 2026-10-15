using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class UserAchievementRepository : GenericRepository<UserAchievement>, IUserAchievementRepository
{
    public UserAchievementRepository(DbContext dbContext) : base(dbContext) { }
}
