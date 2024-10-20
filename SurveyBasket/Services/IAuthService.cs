namespace SurveyBasket.Services
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> GetTokenAsync(string email, string password, CancellationToken cancellationToken);
        Task<Result<AuthResponse>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken);
        Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken);
        Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
        Task<Result> ConfirmEmailAsync(ConfirmEmailRequest request);
        Task<Result> ResendConfirmEmailAsync(ResendConfirmationEmailRequest request);
        Task<Result> SentResetPasswordCodeAsync(string email);
        Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
