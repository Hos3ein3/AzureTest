import os

configs_dir = "src/AzureTest.Persistence/Configurations"
if not os.path.exists(configs_dir):
    os.makedirs(configs_dir)

entities_info = {
    "TaskItem": "builder.HasKey(t => t.Id);\n        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);\n        builder.Property(t => t.Priority).HasMaxLength(50);\n        builder.HasIndex(t => t.UserId);",
    "Ticket": "builder.HasKey(t => t.Id);\n        builder.Property(t => t.Subject).IsRequired().HasMaxLength(255);\n        builder.HasIndex(t => t.UserId);\n\n        builder.HasMany(t => t.Messages)\n               .WithOne(m => m.Ticket)\n               .HasForeignKey(m => m.TicketId)\n               .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);",
    "Expense": "builder.HasKey(x => x.Id);",
    "Event": "builder.HasKey(e => e.Id);\n        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);\n        builder.Property(e => e.Color).HasMaxLength(20);\n        builder.HasIndex(e => e.UserId);",
    "TicketMessage": "builder.HasKey(m => m.Id);\n        builder.HasIndex(m => m.TicketId);",
    "Debt": "builder.HasKey(x => x.Id);",
    "HabitTracker": "builder.HasKey(h => h.Id);\n        builder.Property(h => h.Name).IsRequired().HasMaxLength(200);\n        builder.HasIndex(h => h.UserId);\n\n        builder.HasMany(h => h.Logs)\n               .WithOne(l => l.HabitTracker)\n               .HasForeignKey(l => l.HabitTrackerId)\n               .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);",
    "HabitLog": "builder.HasKey(l => l.Id);\n        builder.HasIndex(l => new { l.HabitTrackerId, l.LogDate }).IsUnique();"
}

# Add individual files for those in entities_info
for entity, config_code in entities_info.items():
    file_content = f"""using AzureTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations;

public class {entity}Configuration : IEntityTypeConfiguration<{entity}>
{{
    public void Configure(EntityTypeBuilder<{entity}> builder)
    {{
        {config_code}
    }}
}}
"""
    with open(os.path.join(configs_dir, f"{entity}Configuration.cs"), "w") as f:
        f.write(file_content)

# For Permission and RolePermission (already exists in PermissionConfiguration.cs, we will split them)
with open(os.path.join(configs_dir, "PermissionConfiguration.cs"), "w") as f:
    f.write("""using AzureTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions", "Identity");
        builder.HasKey(p => p.Id);
    }
}
""")

with open(os.path.join(configs_dir, "RolePermissionConfiguration.cs"), "w") as f:
    f.write("""using AzureTest.Domain.Entities;
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
""")

# For Identity entities
identity_configs = {
    "ApplicationUser": 'builder.ToTable("Users", "Identity");',
    "ApplicationRole": 'builder.ToTable("Roles", "Identity");',
    "RefreshToken": 'builder.ToTable("RefreshTokens", "Identity");'
}

for entity, config_code in identity_configs.items():
    file_content = f"""using AzureTest.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations;

public class {entity}Configuration : IEntityTypeConfiguration<{entity}>
{{
    public void Configure(EntityTypeBuilder<{entity}> builder)
    {{
        {config_code}
    }}
}}
"""
    with open(os.path.join(configs_dir, f"{entity}Configuration.cs"), "w") as f:
        f.write(file_content)


# Create a file for the standard Identity entities (IdentityUserRole, etc)
with open(os.path.join(configs_dir, "IdentityFrameworkConfiguration.cs"), "w") as f:
    f.write("""using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations;

public class IdentityUserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<Guid>> builder)
    {
        builder.ToTable("UserRoles", "Identity");
    }
}

public class IdentityUserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<Guid>> builder)
    {
        builder.ToTable("UserClaims", "Identity");
    }
}

public class IdentityUserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> builder)
    {
        builder.ToTable("UserLogins", "Identity");
    }
}

public class IdentityRoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
    {
        builder.ToTable("RoleClaims", "Identity");
    }
}

public class IdentityUserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder)
    {
        builder.ToTable("UserTokens", "Identity");
    }
}
""")

