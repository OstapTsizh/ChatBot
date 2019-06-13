using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Template.Bot
{
    public class Bot : IBot
    {
        
        public Bot()
        {
            
        }

        public async Task OnTurnAsync(ITurnContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Welcome user if it is new,
            // start MainDialog.


            // If current user continues dialog

            string validatedUserText = GetUserText(context, context.Activity).ToString();
            
            //
            // Dialog.Run(validatedUserText)

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
