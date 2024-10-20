namespace SurveyBasket.Contracts.Authentication
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Code).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().Matches(RegexPatterns.Password).WithMessage("Password Should be at least 8 digits and should contains Lower Case And Upper Case");
        }
    }
}
