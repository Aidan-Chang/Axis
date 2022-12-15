using Axis.Identity.Abstraction.Models;
using Axis.Identity.Common.Managers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Axis.Identity.Common.DbContexts;

public static class IdentityDbContextInitializer {

  public static void UseSeedIdentityData(this IApplicationBuilder app) {

    using var scope = app.ApplicationServices.CreateScope();
    IdentityDbContext? ctx = scope.ServiceProvider.GetService<IdentityDbContext>();
    ctx?.Database.EnsureCreated();

    UserManager userManager = scope.ServiceProvider.GetService<UserManager>()!;
    RoleManager roleManager = scope.ServiceProvider.GetService<RoleManager>()!;

    if (userManager.Users.Any() && roleManager.Roles.Any())
      return;

    // defined roles
    (Guid, string, bool)[] roles = {
      (new Guid("8368a8e4-efdf-4251-9daf-d89fa9e34f0b"), "Admins", true),
      (new Guid("f6e9b926-87bc-472f-8257-c7e3cb2d306e"), "Users", false),
    };

    // defined users
    (Guid, string, string, string, string, bool)[] users = {
      (new Guid("0aabc58d-40ee-42e0-9e47-9435fc8630b7"),"Admin", "1q2w3e4r5t", "Administrator", "Admins", true),
      (new Guid("3fb02738-88fe-44b0-9d45-e5a7ab50becb"),"Guest", "12345678", "Guest", "Users", false),
      (new Guid("6d993a50-5245-4561-b6c5-687f3841ac17"),"User01", "12345678", "Demo User 01", "Users", false),
      (new Guid("dd59f01d-4dc6-4f30-b59c-dda64dc94f40"),"User02", "12345678", "Demo User 02", "Users", false),
      (new Guid("4538cf8a-bc00-4805-9108-aa2710285fb5"),"User03", "12345678", "Demo User 03", "Users", false),
    };

    Task.Run(async () => {
      // seed roles
      foreach (var row in roles)
        if (await roleManager.RoleExistsAsync(row.Item2) == false)
          await roleManager.CreateAsync(new Role { Id = row.Item1, Name = row.Item2, GroupName = "App", GroupNum = 1, Admin = row.Item3, Embedbed = true, Enabled = true });

      // seed users
      foreach (var row in users) {
        User? user = await userManager.FindByIdAsync(row.Item1.ToString("D"));
        if (user == null) {
          user = new User { Id = row.Item1, UserName = row.Item2, Title = row.Item4, Admin = row.Item6, Embedbed = true, Enabled = true };
          await userManager.CreateAsync(user, row.Item3);
        }

        if (await roleManager.RoleExistsAsync(row.Item5))
          if (await userManager.IsInRoleAsync(user, row.Item5) == false)
            await userManager.AddToRoleAsync(user, row.Item5);
      }
    }).Wait();
  }

}
