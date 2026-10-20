using AzureTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations;

public class DebtConfiguration : IEntityTypeConfiguration<Debt>
{
    public void Configure(EntityTypeBuilder<Debt> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.TenantId);
    }
}
