namespace SurveyBasket.Contracts.Results
{
    public record VotesPerQuestionResponse(
        string Question,
        int Votes,
        IEnumerable<AnsweCountResponse> Answers);

}
