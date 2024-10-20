namespace SurveyBasket.Contracts.Authentication
{
    public record LoginRequest(
        string email,
        string password
        );

}
