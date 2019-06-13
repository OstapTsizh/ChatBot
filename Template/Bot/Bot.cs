using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Template.Core;
using Template.Core.Interfaces;
using Template.QAModels;
using Microsoft.Bot.Builder.Dialogs;

namespace Template.Bot
{
    public class Bot : IBot
    {
        // Where to save previous Key words or state?
        // in the Model or in Some Context (in the Bot or in the QAConstructor)

        private IQuestionCtor _questionCtor;
        
        public Bot(IQuestionCtor questionCtor)
        {
            _questionCtor = questionCtor;
        }

        public async Task OnTurnAsync(ITurnContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Welcome user if it is new,
            // start MainDialog.


            // If current user continues dialog

            string validatedUserText = GetUserText(context, context.Activity).ToString();

            //
            // Dialog.Run(validatedUserText)
            // or
            // await QuestionConstructor.GetQuestionOrResult(validatedUserText, previousKeyWords)

            
            // return next Question or Answer to user
            // call some method that chooses correct Prompt (message)
            await SendMessageToUser(context, validatedUserText);

        }

        private async Task<IMessageActivity> GetUserText(ITurnContext context, Activity activity)
        {
            // Validation
            // Errors
            // Restart dialog
            // ...

            IMessageActivity validatedUserText = null;
            return validatedUserText;
        }


        private async Task SendMessageToUser(ITurnContext context, string validatedUserText)
        {
            // Getting next question or result from Question Constructor
            // received Json as string
            var QCResult =_questionCtor.GetQuestionOrResult(validatedUserText);

            // Deserialize QCResult into the model to send next question or
            // final result
            var qcResponse = JsonConvert.DeserializeObject<QCResponse>(QCResult);

            // ToDo
            // Some logic here to choose correct Prompt or simple message etc.


            IMessageActivity nextMessage = null;
            await context.SendActivityAsync(nextMessage);

            //return nextMessage;
        }
    }
}
