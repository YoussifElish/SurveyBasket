﻿namespace SurveyBasket.Contracts.Users
{
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().Length(3, 100);
            RuleFor(x => x.LastName).NotEmpty().Length(3, 100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().Matches(RegexPatterns.Password).WithMessage("Password should be at least 8 digits and should contain Lowercase , UpperCase, special character ");
            RuleFor(x => x.Roles).NotNull().NotEmpty();
            RuleFor(x => x.Roles).Must(x => x.Distinct().Count() == x.Count).When(x => x.Roles != null);
        }
    }
}
