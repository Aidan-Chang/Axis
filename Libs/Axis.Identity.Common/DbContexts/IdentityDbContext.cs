using Axis.Identity.Abstraction.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Axis.Identity.Common.DbContexts;

public class IdentityDbContext : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken> {

  public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

  protected override void OnModelCreating(ModelBuilder builder) {

    base.OnModelCreating(builder);

    builder.HasDefaultSchema("identity");

    builder.Entity<User>(entity => {
      entity.ToTable("users");
      entity.HasKey(key => new { key.Id });
      entity.Property(x => x.Id).HasColumnOrder(0);
      entity.Property(x => x.UserName).HasMaxLength(200).HasColumnOrder(1);
      entity.Property(x => x.NormalizedUserName).HasMaxLength(200).HasColumnOrder(2);
      entity.Property(x => x.PasswordHash).HasMaxLength(200).HasColumnOrder(3);
      entity.Property(x => x.Title).HasMaxLength(200).HasColumnOrder(4);
      entity.Property(x => x.Email).HasMaxLength(200).HasColumnOrder(5);
      entity.Property(x => x.NormalizedEmail).HasMaxLength(200).HasColumnOrder(6);
      entity.Property(x => x.EmailConfirmed).HasColumnOrder(7);
      entity.Property(x => x.PhoneNumber).HasMaxLength(200).HasColumnOrder(8);
      entity.Property(x => x.PhoneNumberConfirmed).HasColumnOrder(9);
      entity.Property(x => x.TwoFactorEnabled).HasColumnOrder(10);
      entity.Property(x => x.ConcurrencyStamp).HasMaxLength(200).HasColumnOrder(11);
      entity.Property(x => x.SecurityStamp).HasMaxLength(200).HasColumnOrder(12);
      entity.Property(x => x.LastLogOn).HasColumnOrder(13);
      entity.Property(x => x.TokenRefreshOn).HasColumnOrder(14);
      entity.Property(x => x.AccessFailedCount).HasColumnOrder(15);
      entity.Property(x => x.LockoutEnabled).HasColumnOrder(16);
      entity.Property(x => x.LockoutEnd).HasColumnOrder(17);
      entity.Property(x => x.Admin).HasColumnOrder(87);
      entity.Property(x => x.Embedbed).HasColumnOrder(88);
      entity.Property(x => x.Enabled).HasColumnOrder(89);
      entity.Property(x => x.CreatedBy).HasMaxLength(50).HasColumnOrder(95);
      entity.Property(x => x.CreatedOn).HasColumnOrder(96);
      entity.Property(x => x.UpdatedBy).HasMaxLength(50).HasColumnOrder(97);
      entity.Property(x => x.UpdatedOn).HasColumnOrder(98);

      entity.HasIndex(x => new { x.NormalizedUserName }).Metadata.SetDatabaseName("uix_users_normalizedname");
      entity.HasIndex(x => new { x.NormalizedEmail }).Metadata.SetDatabaseName("uix_users_normalizedemail");
    });

    builder.Entity<Role>(entity => {
      entity.ToTable("roles");
      entity.HasKey(key => new { key.Id });
      entity.Property(x => x.Id).HasColumnOrder(0);
      entity.Property(x => x.Name).HasMaxLength(200).HasColumnOrder(1);
      entity.Property(x => x.NormalizedName).HasMaxLength(200).HasColumnOrder(2);
      entity.Property(x => x.GroupName).HasMaxLength(200).HasColumnOrder(3);
      entity.Property(x => x.GroupNum).HasMaxLength(200).HasColumnOrder(4);
      entity.Property(x => x.ConcurrencyStamp).HasMaxLength(200).HasColumnOrder(5);
      entity.Property(x => x.Admin).HasColumnOrder(87);
      entity.Property(x => x.Embedbed).HasColumnOrder(88);
      entity.Property(x => x.Enabled).HasColumnOrder(89);
      entity.Property(x => x.CreatedBy).HasMaxLength(50).HasColumnOrder(95);
      entity.Property(x => x.CreatedOn).HasColumnOrder(96);
      entity.Property(x => x.UpdatedBy).HasMaxLength(50).HasColumnOrder(97);
      entity.Property(x => x.UpdatedOn).HasColumnOrder(98);

      entity.HasIndex(x => new { x.NormalizedName }).Metadata.SetDatabaseName("uix_roles_normalizedname");
    });

    builder.Entity<RoleClaim>(entity => {
      entity.ToTable("roleclaims");
      entity.HasKey(key => new { key.Id });
      entity.Property(x => x.ClaimType).HasMaxLength(50);
      entity.Property(x => x.ClaimValue).HasMaxLength(200);
    });

    builder.Entity<UserRole>(entity => {
      entity.ToTable("userroles");
      entity.HasKey(key => new { key.UserId, key.RoleId });
    });

    builder.Entity<UserToken>(entity => {
      entity.ToTable("usertokens");
      entity.HasKey(key => new { key.UserId, key.LoginProvider, key.Name });
      entity.Property(x => x.LoginProvider).HasMaxLength(50);
      entity.Property(x => x.Name).HasMaxLength(50);
      entity.Property(x => x.Value).HasMaxLength(200);
    });

    builder.Entity<UserLogin>(entity => {
      entity.ToTable("userlogins");
      entity.HasKey(key => new { key.LoginProvider, key.ProviderKey });
      entity.Property(x => x.LoginProvider).HasMaxLength(50);
      entity.Property(x => x.ProviderKey).HasMaxLength(200);
      entity.Property(x => x.ProviderDisplayName).HasMaxLength(200);
    });

    builder.Entity<UserClaim>(entity => {
      entity.ToTable("userclaims");
      entity.HasKey(key => new { key.Id });
      entity.Property(x => x.ClaimType).HasMaxLength(50);
      entity.Property(x => x.ClaimValue).HasMaxLength(200);
    });

    builder.Entity<UserDefault>(entity => {
      entity.ToTable("userdefaults");
      entity.HasKey(key => new { key.UserId, key.Scope, key.Name });
      entity.Property(x => x.Scope).HasMaxLength(50);
      entity.Property(x => x.Name).HasMaxLength(50);
      entity.Property(x => x.Value).HasMaxLength(200);

      entity.HasOne<User>()
        .WithMany()
        .HasForeignKey(x => x.UserId)
        .OnDelete(DeleteBehavior.Cascade)
        .IsRequired();
    });
  }

  public DbSet<UserDefault> UserDefaults { get; set; } = default!;

}
