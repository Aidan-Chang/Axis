using Axis.Identity.Abstraction.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Axis.Identity.Common.Managers;

public class RoleManager : RoleManager<Role> {

  public RoleManager(
    IRoleStore<Role> store,
    IEnumerable<IRoleValidator<Role>> roleValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    ILogger<RoleManager> logger) : base(store, roleValidators, keyNormalizer, errors, logger) { }

  public override async Task<Role?> FindByIdAsync(string roleId) {
    roleId = roleId.ToLower();
    return await base.FindByIdAsync(roleId);
  }

}
