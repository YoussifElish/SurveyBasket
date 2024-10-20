namespace SurveyBasket.Contracts.Authentication
{
    public record AuthResponse(
        string Id,
        string? Email,
        string FirstName,
        String LastName,
        string Token,
        int ExpiresIn,
        string RefreshToken,
        DateTime RefreshTokenExpiration
        );

}
