namespace SurveyBasket.Errors
{
    public static class QuestionErrors
    {

        public static readonly Error QuestionNotFound = new("Question.QuestionNotFound", "no question was found with the given id", StatusCodes.Status404NotFound);

        public static readonly Error DuplicatedQuestionContent = new("Question.DuplicatedContent", "Another question with the same content is already exist ", StatusCodes.Status400BadRequest);

    }
}
