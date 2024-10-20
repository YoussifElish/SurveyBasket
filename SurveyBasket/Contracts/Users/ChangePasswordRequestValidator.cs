namespace SurveyBasket.Contracts.Users
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().Matches(RegexPatterns.Password).WithMessage("Password Should be at least 8 digits and should contains Lower Case And Upper Case").NotEqual(x => x.CurrentPassword).WithMessage("New Password Can't be same as current password");
        }
    }
}
