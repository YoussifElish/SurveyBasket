namespace SurveyBasket.Helpers
{
    public static class EmailBodyBuider
    {
        public static string GenerateEmailBody(string tempelate, Dictionary<string, string> tempelateModel)
        {
            var tempelatePath = $"{Directory.GetCurrentDirectory()}/Templates/{tempelate}.html";
            var streamReader = new StreamReader(tempelatePath);
            var body = streamReader.ReadToEnd();
            streamReader.Close();
            foreach (var item in tempelateModel)
            {
                body = body.Replace(item.Key, item.Value);
            }

            return body;

        }
    }
}
