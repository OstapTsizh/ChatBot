
namespace StuddyBot.Core.Models
{
    /// <summary>
    /// Bot's question to user.
    /// </summary>
    public class Question
    {
        /// <summary>
        /// Bot's question.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// indicates if user answered the question.
        /// </summary>
        public string IsAnswered { get; set; }

        /// <summary>
        /// Answers to the question, which user can choose.
        /// </summary>
        public string[] Keywords { get; set; }
    }
}