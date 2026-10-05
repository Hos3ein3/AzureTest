import re

with open('./src/AzureTest.Persistence/Context/AzureTestDbContext.cs', 'r') as f:
    content = f.read()

# Remove Identity Schema Mapping block
block_to_remove = r"""\s*// Identity Schema Mapping
\s*modelBuilder\.Entity<ApplicationUser>\(\)\.ToTable\("Users", "Identity"\);
\s*modelBuilder\.Entity<ApplicationRole>\(\)\.ToTable\("Roles", "Identity"\);
\s*modelBuilder\.Entity<IdentityUserRole<Guid>>\(\)\.ToTable\("UserRoles", "Identity"\);
\s*modelBuilder\.Entity<IdentityUserClaim<Guid>>\(\)\.ToTable\("UserClaims", "Identity"\);
\s*modelBuilder\.Entity<IdentityUserLogin<Guid>>\(\)\.ToTable\("UserLogins", "Identity"\);
\s*modelBuilder\.Entity<IdentityRoleClaim<Guid>>\(\)\.ToTable\("RoleClaims", "Identity"\);
\s*modelBuilder\.Entity<IdentityUserToken<Guid>>\(\)\.ToTable\("UserTokens", "Identity"\);
\s*modelBuilder\.Entity<RefreshToken>\(\)\.ToTable\("RefreshTokens", "Identity"\);"""

content = re.sub(block_to_remove, "", content)

with open('./src/AzureTest.Persistence/Context/AzureTestDbContext.cs', 'w') as f:
    f.write(content)

