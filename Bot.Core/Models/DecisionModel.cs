
namespace StuddyBot.Core.Models
{
    /// <summary>
    /// Contains bot's final response to user.
    /// </summary>
    public class DecisionModel
    {
        /// <summary>
        /// Keywords from user's answers for bot's questions.
        /// </summary>
        public string[] Meta { get; set; }

        /// <summary>
        /// Bot's final answer.
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        /// Resource that bot gives to user. 
        /// </summary>
        public string Resources { get; set; }
    }
}
