﻿namespace SurveyBasket.Contracts.Polls
{

    public class LoginRequestValidator : AbstractValidator<PollRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().Length(3, 100);
            RuleFor(x => x.Summary)
                .NotEmpty().Length(3, 1500);


            RuleFor(x => x.StartsAt).NotEmpty().GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today));
            RuleFor(x => x.EndsAt).NotEmpty().GreaterThanOrEqualTo(x => x.StartsAt).WithMessage("{PropertyName} must be greater than or equal start date");
            //RuleFor(x => x).Must(HasValidDates).WithName(nameof(PollRequest.EndsAt)).WithMessage("{PropertyName} must be greater than or equal start date");
        }

        //private bool HasValidDates(PollRequest pollRequest)
        //{
        //    return pollRequest.EndsAt >= pollRequest.StartsAt;
        //}
    }
}
