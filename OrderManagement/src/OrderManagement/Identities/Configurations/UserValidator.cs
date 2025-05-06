using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Identity;

namespace OrderManagement.Identities.Configurations
{
    public class UserValidator : IResourceOwnerPasswordValidator
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public UserValidator(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = await _userManager.FindByNameAsync(context.UserName);

            if (user == null)
            {
                context.Result = new GrantValidationResult(
                    TokenRequestErrors.InvalidGrant,
                    "Invalid username or password"
                );
                return;
            }

            var signInResult = await _signInManager.PasswordSignInAsync(
                user,
                context.Password,
                isPersistent: true,
                lockoutOnFailure: true);

            if (signInResult.Succeeded)
            {
                var userId = user.Id;

                context.Result = new GrantValidationResult(
                    subject: userId,
                    authenticationMethod: "custom",
                    claims: new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim(ClaimTypes.Name, user.UserName)
                    });
                return;
            }

            context.Result = new GrantValidationResult(
                TokenRequestErrors.InvalidGrant,
                "Invalid username or password"
            );
        }
    }
}
