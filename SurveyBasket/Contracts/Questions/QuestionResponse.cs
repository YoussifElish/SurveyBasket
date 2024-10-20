using SurveyBasket.Contracts.Answers;

namespace SurveyBasket.Contracts.Questions
{
    public record QuestionResponse(
        int id,
        string Content,
        IEnumerable<AnswerResponse> Answers);

}
