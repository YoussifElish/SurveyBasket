namespace SurveyBasket.Contracts.Results
{
    public record PollVotesResponse(
        string PollTitle,
        IEnumerable<VoteResponse> Votes
        );

}
