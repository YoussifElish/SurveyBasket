namespace SurveyBasket.Authentication
{
    public interface IJwtProvider
    {
        (string token, int ExpiresIn) GenerateToken(ApplicationUser user, IEnumerable<string> roles, IEnumerable<string> pemissions);

        string? ValidateToken(string Token);
    }
}
