using System.Collections.Generic;
using StuddyBot.Core.Models;

namespace StuddyBot.Core.Interfaces
{
    /// <summary>
    /// An interface to implement in a class that will be
    /// getting a question/answer; making decision based on keywords
    /// from the user.
    /// </summary>
    public interface IDecisionMaker
    {
        /// <summary>
        /// Return a string with topic(s), question(s)/answer(s), next keywords.
        /// </summary>
        /// <param name="name">A topic to look for question(s)/answer(s),
        /// decision(s).</param>
        /// <returns>Model with name, keywords, questions, decisions" </returns>
        QuestionModel GetQuestionOrResult(string name);

        /// <summary>
        /// Finds right decision according to user's answers
        /// </summary>
        /// <param name="answers"> Stored user's answers</param>
        /// <param name="questionModel">Topic Model</param>
        /// <returns>DecisionModel object of <see cref="DecisionModel"/> class</returns>
        DecisionModel GetDecision(List<string> answers, QuestionModel questionModel);

        /// <summary>
        /// Gets the first (starting) topics from the dbContext.
        /// </summary>
        /// <returns></returns>
        List<string> GetStartTopics();

        /// <summary>
        /// Gets all available countries from the dbContext.
        /// </summary>
        /// <returns></returns>
        List<Country> GetCountries(string lang);
        
        /// <summary>
        /// Gets all localized main menu items (e.g. navigation bar)
        /// from the dbContext.
        /// </summary>
        /// <returns></returns>
        List<MainMenuItem> GetMainMenuItems(string lang);

        /// <summary>
        /// Gets all main menu items (e.g. navigation bar) from the dbContext
        /// in neutral language.
        /// </summary>
        /// <returns></returns>
        List<string> GetMainMenuItemsNeutral();

        /// <summary>
        /// Gets all courses in selected city from the dbContext.
        /// </summary>
        /// <returns></returns>
        List<Course> GetCourses(string lang);

        ///// <summary>
        ///// Gets about info from the dbContext.
        ///// </summary>
        ///// <returns></returns>
        //List<string> GetAbout();

        /// <summary>
        /// Gets questions/answers from the dbContext.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, List<string>> GetQAs(string lang);

        /// <summary>
        /// Gets planned events from the dbContext.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, List<string>> GetPlannedEvents(string lang);

        /// <summary>
        /// Gets a ChooseOption list from the dbContext.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetChooseOptions(string lang);
    }
}
