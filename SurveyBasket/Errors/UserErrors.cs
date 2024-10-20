namespace SurveyBasket.Errors
{
    public static class UserErrors
    {
        public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Invalid Email Or Password", StatusCodes.Status400BadRequest);
        public static readonly Error InvalidToken = new("User.InvalidToken", "Invalid Email Or Password", StatusCodes.Status400BadRequest);
        public static readonly Error DuplicatedEmail = new("User.DuplicatedEmail", "Another User With The Same Email Is Exist", StatusCodes.Status409Conflict);
        public static readonly Error DisabledUser = new("User.DisabledUser", "Disabled User Please Contacy Your Administrator", StatusCodes.Status409Conflict);
        public static readonly Error InavlidUser = new("User.InvalidUser", "Invalid Email Or Password", StatusCodes.Status400BadRequest);
        public static readonly Error NotFound = new("User.NotFound", "User Not Found", StatusCodes.Status404NotFound);
        public static readonly Error InavlidPassword = new("User.InavlidPassword", "Invalid Password", StatusCodes.Status400BadRequest);
        public static readonly Error OperationFailed = new("User.OperatinFailed", "Operation Failed", StatusCodes.Status400BadRequest);
        public static readonly Error EmailNotConfirmed = new("User.EmailNotConfirmed", "Email Is Not Confirmed", StatusCodes.Status401Unauthorized);
        public static readonly Error LockedUser = new("User.LockedUser", "Please Contact Your Administrator", StatusCodes.Status401Unauthorized);
        public static readonly Error InvalidCode = new("User.InvalidCode", "Invalid Code", StatusCodes.Status401Unauthorized);
        public static readonly Error DuplicatedConfirmation = new("User.DuplicatedConfirmation", "Email already Confirmed", StatusCodes.Status400BadRequest);

    }
}
