using System.Security.Claims;

namespace Z0.Infrastructure.Identity;

public interface ICognitoTokenValidator
{
    bool ValidateScopes(ClaimsPrincipal user, params string[] requiredScopes);
    string? GetUserId(ClaimsPrincipal user);
    string? GetUserEmail(ClaimsPrincipal user);
    IEnumerable<string> GetUserScopes(ClaimsPrincipal user);
}

public class CognitoTokenValidator : ICognitoTokenValidator
{
    public bool ValidateScopes(ClaimsPrincipal user, params string[] requiredScopes)
    {
        var userScopes = GetUserScopes(user).ToList();

        return requiredScopes.All(required =>
            userScopes.Contains(required) ||
            userScopes.Any(s => s.EndsWith(":*") && required.StartsWith(s.Replace(":*", ":"))));
    }

    public string? GetUserId(ClaimsPrincipal user)
    {
        // Cognito uses "sub" claim for user ID
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;
    }

    public string? GetUserEmail(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value
            ?? user.FindFirst("email")?.Value;
    }

    public IEnumerable<string> GetUserScopes(ClaimsPrincipal user)
    {
        var scopeClaim = user.FindFirst("scope");
        if (scopeClaim != null)
        {
            return scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        return Enumerable.Empty<string>();
    }
}
