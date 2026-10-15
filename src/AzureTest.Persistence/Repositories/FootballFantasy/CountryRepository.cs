using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories.FootballFantasy;

public class CountryRepository : GenericRepository<Country>, ICountryRepository
{
    public CountryRepository(DbContext context) : base(context) { }
}
