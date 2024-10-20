namespace SurveyBasket.Services
{
    public interface INotificationService
    {
        Task SendNewPollsNotification(int? id = null);
    }
}
