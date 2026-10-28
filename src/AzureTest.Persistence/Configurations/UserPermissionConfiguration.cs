using AzureTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations;

public sealed class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("UserPermissions", "Identity");
        builder.HasKey(up => up.Id);

        builder.Property(up => up.UserId)
            .IsRequired();

        builder.Property(up => up.PermissionId)
            .IsRequired();

        builder.Property(up => up.IsGranted)
            .HasDefaultValue(UserPermission.Rules.DefaultIsGranted)
            .IsRequired();

        builder.HasIndex(up => new { up.UserId, up.PermissionId })
            .IsUnique();

        builder.HasOne(up => up.Permission)
            .WithMany()
            .HasForeignKey(up => up.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
