namespace SurveyBasket.Contracts.Users
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().Length(3, 100);
            RuleFor(x => x.LastName).NotEmpty().Length(3, 100);
            RuleFor(x => x.Roles).NotNull().NotEmpty();
            RuleFor(x => x.Roles).Must(x => x.Distinct().Count() == x.Count).When(x => x.Roles != null);
        }
    }
}
