using System;
using Template.Core.Interfaces;

namespace DecisionMakers
{
    public class DecisionMaker : IDecisionMaker
    {
        // Where to save previous Keywords or state?
        // in the Model or in Some Context (in the Bot or in the QAConstructor)
        
        public DecisionMaker() { }

        public string GetQuestionOrResult(string topic)
        {
            throw new NotImplementedException();
            // ToDo
            // reading Json from file/DBContext etc.
            //return (string) ...;
        }
    }
}
