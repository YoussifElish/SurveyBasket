using Microsoft.Extensions.Caching.Hybrid;
using SurveyBasket.Abstractions;
using SurveyBasket.Contracts.Answers;
using SurveyBasket.Contracts.Common;
using SurveyBasket.Contracts.Questions;
using System.Linq.Dynamic.Core;


namespace SurveyBasket.Services
{
    public class QuestionService(ApplicationDbContext context, HybridCache hybridCache) : IQuestionService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly HybridCache _hybridCache = hybridCache;
        private const string _cachePrefix = "availableQuestions";

        public async Task<Result<PaginatedList<QuestionResponse>>> GetAllAsync(int pollId, RequestFilters filters, CancellationToken cancellationToken = default)
        {
            var pollIsExists = await _context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken: cancellationToken);

            if (!pollIsExists)
                return Result.Failure<PaginatedList<QuestionResponse>>(PollErrors.PollNotFound);

            var query = _context.Questions
                .Where(x => x.PollId == pollId);

            if (!string.IsNullOrEmpty(filters.SearchValue))
            {
                query = query.Where(x => x.Content.Contains(filters.SearchValue));
            }

            if (!string.IsNullOrEmpty(filters.SortColumn))
            {
                query = query.OrderBy($"{filters.SortColumn} {filters.SortDirection}");
            }

            var source = query
                            .Include(x => x.Answers)
                            .ProjectToType<QuestionResponse>()
                            .AsNoTracking();

            var questions = await PaginatedList<QuestionResponse>.CreateAsync(source, filters.PageNumber, filters.PageSize, cancellationToken);

            return Result.Success(questions);
        }



        public async Task<Result<IEnumerable<QuestionResponse>>> GetAvailableAsync(int pollId, string userId, CancellationToken cancellationToken = default)
        {


            var hasVote = await _context.Votes.AnyAsync(x => x.PollId == pollId && x.UserId == userId, cancellationToken);
            if (hasVote)
                return Result.Failure<IEnumerable<QuestionResponse>>(VoteErrors.DuplicatedVote);

            var pollIsExist = await _context.Polls.AnyAsync(x => x.Id == pollId && x.IsPublished && x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
            if (!pollIsExist)
                return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);
            var cahcheKey = $"{_cachePrefix}-{pollId}";

            var questions = await _hybridCache.GetOrCreateAsync<IEnumerable<QuestionResponse>>(cahcheKey, async cashEntry => await _context.Questions.Where(x => x.PollId == pollId && x.IsActive).Include(x => x.Answers).Select(q => new QuestionResponse(q.id,
            q.Content,
            q.Answers.Where(a => a.IsActive).Select(a => new AnswerResponse(a.Id, a.Content))
            )).AsNoTracking().ToListAsync(),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5)
            }
            );

            return Result.Success<IEnumerable<QuestionResponse>>(questions);


        }


        public async Task<Result<QuestionResponse>> GetAsync(int pollId, int id, CancellationToken cancellationToken = default)
        {

            var question = await _context.Questions.Where(x => x.PollId == pollId && x.id == id).Include(x => x.Answers).ProjectToType<QuestionResponse>().AsNoTracking().SingleOrDefaultAsync(cancellationToken);

            if (question is null)
                return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);

            return Result.Success<QuestionResponse>(question);
        }



        public async Task<Result<QuestionResponse>> AddAsync(int pollId, QuestionRequest request, CancellationToken cancellationToken = default)
        {
            var pollIsExist = await _context.Polls.AnyAsync(x => x.Id == pollId, cancellationToken: cancellationToken);
            if (!pollIsExist)
                return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);
            var questionIsExist = await _context.Questions.AnyAsync(x => x.Content == request.Content && x.PollId == pollId, cancellationToken: cancellationToken);
            if (questionIsExist)
                return Result.Failure<QuestionResponse>(QuestionErrors.DuplicatedQuestionContent);

            var question = request.Adapt<Question>();
            question.PollId = pollId;

            await _context.AddAsync(question, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            //await _cacheService.RemoveAsync($"{_cachePrefix}-{pollId}");
            await _hybridCache.RemoveAsync($"{_cachePrefix}-{pollId}");
            return Result.Success(question.Adapt<QuestionResponse>());

        }

        public async Task<Result> UpdateAsync(int pollId, int id, QuestionRequest request, CancellationToken cancellationToken = default)
        {
            var questionIsExist = await _context.Questions.AnyAsync(x => x.PollId == pollId && x.id != id && x.Content == request.Content, cancellationToken);

            if (questionIsExist)
                return Result.Failure(QuestionErrors.DuplicatedQuestionContent);

            var question = await _context.Questions.Include(x => x.Answers).SingleOrDefaultAsync(x => x.PollId == pollId && x.id == id, cancellationToken);
            if (question is null)
                return Result.Failure(QuestionErrors.QuestionNotFound);
            question.Content = request.Content;

            //curent answers
            var currentAnswers = question.Answers.Select(x => x.Content).ToList();

            //add new answers
            var newAnswers = request.Answers.Except(currentAnswers).ToList();
            newAnswers.ForEach(answer =>
            {
                question.Answers.Add(new Answer { Content = answer });
            });
            // remove - deactivate
            question.Answers.ToList().ForEach(answer =>
            {
                answer.IsActive = request.Answers.Contains(answer.Content);
            });
            //await _cacheService.RemoveAsync($"{_cachePrefix}-{pollId}");

            await _context.SaveChangesAsync(cancellationToken);
            await _hybridCache.RemoveAsync($"{_cachePrefix}-{pollId}");
            return Result.Success();



        }

        public async Task<Result> ToggleStatusAsync(int pollId, int id, CancellationToken cancellationToken = default)
        {
            var question = await _context.Questions.Where(x => x.PollId == pollId && x.id == id).Include(x => x.Answers).SingleOrDefaultAsync(cancellationToken);

            if (question is null)
                return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);

            question.IsActive = !question.IsActive;
            foreach (var answer in question.Answers)
                answer.IsActive = !answer.IsActive;
            await _context.SaveChangesAsync(cancellationToken);
            await _hybridCache.RemoveAsync($"{_cachePrefix}-{pollId}");

            return Result.Success();

        }


    }
}
