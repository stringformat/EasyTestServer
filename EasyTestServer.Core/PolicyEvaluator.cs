using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace EasyTestServer.Core;

public class PolicyEvaluator : IPolicyEvaluator
{
    public Task<PolicyAuthorizationResult> AuthorizeAsync(
        AuthorizationPolicy policy,
        AuthenticateResult authenticationResult,
        HttpContext context,
        object? resource)
    {
        return Task.FromResult(PolicyAuthorizationResult.Success());
    }

    public Task<AuthenticateResult> AuthenticateAsync(
        AuthorizationPolicy policy,
        HttpContext context)
    {
        var claimsPrincipal = new ClaimsPrincipal(
            new[]
            {
                new ClaimsIdentity(new[]
                {
                    new Claim("TestClaim", "TestClaimValue")
                })
            });
        var authenticationTicket = new AuthenticationTicket(claimsPrincipal, new AuthenticationProperties(), "TestScheme");
        var authenticateResult = AuthenticateResult.Success(authenticationTicket);
        
        return Task.FromResult(authenticateResult);
    }
}