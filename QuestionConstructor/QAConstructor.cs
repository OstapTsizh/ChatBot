using System;
using Newtonsoft.Json;
using Template.Core.Interfaces;

namespace QuestionConstructor
{
    public class QAConstructor : IQuestionCtor
    {


        public QAConstructor() { }

        public string GetQuestionOrResult()
        {
            throw new NotImplementedException();
            // ToDo
            // reading Json from file/DBContext etc.
            //return (string) ...;
        }
    }
}
