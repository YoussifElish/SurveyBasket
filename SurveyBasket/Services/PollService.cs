using Hangfire;
using SurveyBasket.Contracts.Polls;

namespace SurveyBasket.Services
{
    public class PollService(ApplicationDbContext context, INotificationService notificationService) : IPollService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly INotificationService _notificationService = notificationService;

        public async Task<IEnumerable<PollResponse>> GetAllAsync(CancellationToken cancellationToken = default) => await _context.Polls.AsNoTracking().ProjectToType<PollResponse>().ToListAsync(cancellationToken);

        public async Task<IEnumerable<PollResponse>> GetCurrentAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Polls.Where(x => x.IsPublished && x.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && x.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow)).AsNoTracking().ProjectToType<PollResponse>().ToListAsync();
        }
        public async Task<Result<PollResponse>> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            var poll = await _context.Polls.FindAsync(id, cancellationToken);

            return poll is not null ? Result.Success(poll.Adapt<PollResponse>()) : Result.Failure<PollResponse>(PollErrors.PollNotFound);
        }

        public async Task<Result<PollResponse>> AddAsync(PollRequest poll, CancellationToken cancellationToken = default)
        {
            var isExistingTitle = await _context.Polls.AnyAsync(x => x.Title == poll.Title, cancellationToken: cancellationToken);
            if (isExistingTitle)
                return Result.Failure<PollResponse>(PollErrors.DuplicatePoll);
            var result = poll.Adapt<Poll>();

            await _context.Polls.AddAsync(result, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success(result.Adapt<PollResponse>());
        }

        public async Task<Result> UpdateAsync(int id, PollRequest poll, CancellationToken cancellationToken = default)
        {
            var currentPoll = await _context.Polls.FindAsync(id, cancellationToken);
            if (currentPoll is null)
                return Result.Failure(PollErrors.PollNotFound);
            var isExistingTitle = await _context.Polls.AnyAsync(x => x.Title == poll.Title && x.Id != id);
            if (isExistingTitle)
                return Result.Failure(PollErrors.DuplicatePoll);

            currentPoll.Title = poll.Title;
            currentPoll.Summary = poll.Summary;
            currentPoll.StartsAt = poll.StartsAt;
            currentPoll.EndsAt = poll.EndsAt;
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();

        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var poll = await _context.Polls.FindAsync(id, cancellationToken);
            if (poll is null)
                return Result.Failure(PollErrors.PollNotFound);
            _context.Polls.Remove(poll);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        public async Task<Result> TogglePublishStatusAsync(int id, CancellationToken cancellationToken = default)
        {
            var poll = await _context.Polls.FindAsync(id, cancellationToken);
            if (poll is null)
                return Result.Failure(PollErrors.PollNotFound);
            poll.IsPublished = !poll.IsPublished;
            await _context.SaveChangesAsync(cancellationToken);
            if (poll.IsPublished && poll.StartsAt == DateOnly.FromDateTime(DateTime.UtcNow))
                BackgroundJob.Enqueue(() => _notificationService.SendNewPollsNotification(poll.Id));

            return Result.Success();
        }


    }
}
