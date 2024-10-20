using Microsoft.AspNetCore.Identity.UI.Services;
using SurveyBasket.Helpers;

namespace SurveyBasket.Services
{
    public class NotificationService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, IEmailSender emailSender) : INotificationService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IEmailSender _emailSender = emailSender;

        public async Task SendNewPollsNotification(int? id = null)
        {
            IEnumerable<Poll> polls = [];
            if (id.HasValue)
            {
                var poll = await _context.Polls.
                    SingleOrDefaultAsync(x => x.Id == id && x.IsPublished
                    && x.StartsAt == DateOnly.FromDateTime(DateTime.UtcNow));
                polls = [poll!];

            }
            else
            {
                polls = await _context.Polls.
                     Where(x => x.IsPublished
                     && x.StartsAt == DateOnly.FromDateTime(DateTime.UtcNow))
                     .AsNoTracking()
                     .ToListAsync();


            }

            //TODO : Select Memberes Only
            var users = await _userManager.Users.ToListAsync();
            var origin = _httpContextAccessor.HttpContext?.Request.Headers.Origin;


            foreach (var poll in polls)
            {
                foreach (var user in users)
                {
                    var placeholders = new Dictionary<string, string>
                    {
                        {"{name}",user.FirstName},
                        {"{pollTill}",poll.Title},
                        {"{endDate}",poll.EndsAt.ToString()},
                        {"{url}",$"{origin}/polls/start/{poll.Id}"}
                    };
                    var emailBody = EmailBodyBuider.GenerateEmailBody("PollNotification", placeholders);
                    await _emailSender.SendEmailAsync(user.Email!, $"Survey Basket : New Poll - {poll.Title}", emailBody);

                }
            }

        }
    }
}
