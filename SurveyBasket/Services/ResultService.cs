using SurveyBasket.Contracts.Results;

namespace SurveyBasket.Services
{
    public class ResultService(ApplicationDbContext context) : IResultService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<Result<PollVotesResponse>> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default)
        {
            //var pollVotes = await _context.Polls.Where(x=>x.Id ==  pollId).Include(x=>x.Votes)
            var pollVotes = await _context.Polls.Where(x => x.Id == pollId).Select(x => new PollVotesResponse(
                x.Title,
                x.Votes.Select(x => new VoteResponse($"{x.User.FirstName} {x.User.LastName}", x.SubmitedOn, x.VoteAnswers.Select(x => new QuestionAnswerResponse(x.Question.Content, x.Answer.Content)))))).SingleOrDefaultAsync(cancellationToken);

            return pollVotes is null ? Result.Failure<PollVotesResponse>(PollErrors.PollNotFound) : Result.Success<PollVotesResponse>(pollVotes);
        }


        public async Task<Result<IEnumerable<VotesPerDayResponse>>> GetVotesPerDayAsync(int pollId, CancellationToken cancellationToken = default)
        {
            var pollIsExist = await _context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken: cancellationToken);
            if (!pollIsExist)
                return Result.Failure<IEnumerable<VotesPerDayResponse>>(PollErrors.PollNotFound);

            var votesPerDay = await _context.Votes.Where(X => X.PollId == pollId).GroupBy(x => new { Date = DateOnly.FromDateTime(x.SubmitedOn) }).Select(g => new VotesPerDayResponse(g.Key.Date, g.Count())).ToListAsync(cancellationToken);

            return Result.Success<IEnumerable<VotesPerDayResponse>>(votesPerDay);
        }



        public async Task<Result<IEnumerable<VotesPerQuestionResponse>>> GetVotesPerQuestionAsync(int pollId, CancellationToken cancellationToken = default)
        {
            var pollIsExist = await _context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken: cancellationToken);
            if (!pollIsExist)
                return Result.Failure<IEnumerable<VotesPerQuestionResponse>>(PollErrors.PollNotFound);

            var votesPerQuestion = await _context.Votes.Where(X => X.PollId == pollId)
                .SelectMany(vote => vote.VoteAnswers)
                .GroupBy(x => new { Question = x.Question.Content })
                .Select(x => new VotesPerQuestionResponse
                (x.Key.Question,
                x.Count(),
                x.GroupBy(x => new { Answer = x.Answer.Content })
                .Select(answer => new AnsweCountResponse(
                    answer.Key.Answer,
                    answer.Count()))
                .ToList()))
                .ToListAsync(cancellationToken);

            return Result.Success<IEnumerable<VotesPerQuestionResponse>>(votesPerQuestion);
        }
    }
}
