using AzureTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions", "Identity");
        builder.HasKey(rp => rp.Id);
        
        builder.HasOne(rp => rp.Permission)
               .WithMany()
               .HasForeignKey(rp => rp.PermissionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
