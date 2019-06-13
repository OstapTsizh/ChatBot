using System;
using Template.Core.Interfaces;

namespace QuestionConstructor
{
    public class QAConstructor : IQuestionCtor
    {
        // Where to save previous Keywords or state?
        // in the Model or in Some Context (in the Bot or in the QAConstructor)
        
        public QAConstructor() { }

        public string GetQuestionOrResult(string topic)
        {
            throw new NotImplementedException();
            // ToDo
            // reading Json from file/DBContext etc.
            //return (string) ...;
        }
    }
}
