﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Template.Core;
using Template.Core.Interfaces;

namespace Template.Bot
{
    public class Bot : IBot
    {
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
            await _questionCtor.GetQuestionOrResult();

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


    }
}
