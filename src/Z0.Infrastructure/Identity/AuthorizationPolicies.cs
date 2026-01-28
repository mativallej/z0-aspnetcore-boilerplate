using Microsoft.AspNetCore.Authorization;

namespace Z0.Infrastructure.Identity;

public static class AuthorizationPolicies
{
    public const string ReadItems = "items:read";
    public const string WriteItems = "items:write";
    public const string DeleteItems = "items:delete";

    public static void AddPolicies(AuthorizationOptions options)
    {
        options.AddPolicy(ReadItems, policy =>
            policy.RequireAssertion(context =>
                HasScope(context, ReadItems) || HasScope(context, "items:*")));

        options.AddPolicy(WriteItems, policy =>
            policy.RequireAssertion(context =>
                HasScope(context, WriteItems) || HasScope(context, "items:*")));

        options.AddPolicy(DeleteItems, policy =>
            policy.RequireAssertion(context =>
                HasScope(context, DeleteItems) || HasScope(context, "items:*")));
    }

    private static bool HasScope(AuthorizationHandlerContext context, string scope)
    {
        // Cognito can send scopes as a space-separated string in "scope" claim
        var scopeClaim = context.User.FindFirst("scope");
        if (scopeClaim != null)
        {
            var scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return scopes.Contains(scope);
        }

        // Also check for individual scope claims (some JWT configurations)
        return context.User.HasClaim("scope", scope);
    }
}
