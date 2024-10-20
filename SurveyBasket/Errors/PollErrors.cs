namespace SurveyBasket.Errors
{
    public static class PollErrors
    {
        public static readonly Error PollNotFound = new("Poll.NotFound", "No Poll Was Found With the given id", StatusCodes.Status404NotFound);
        public static readonly Error DuplicatePoll = new("Poll.Duplicate", "Another Poll With this title already exist", StatusCodes.Status409Conflict);
    }
}
