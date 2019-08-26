using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LoggerService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using StuddyBot.Core.Interfaces;
using System.Linq;
using Newtonsoft.Json;

namespace StuddyBot.Bots
{
    public class DialogAndWelcomeBot<T> : DialogBot<T> where T : Dialog
    {
        // protected MyDialog _myDialog;
        protected ThreadedLogger _Logger;
        protected IDecisionMaker _decisionMaker;

        public DialogAndWelcomeBot(ConversationState conversationState, UserState userState, T dialog, 
            ILogger<DialogBot<T>> logger, IDecisionMaker decisionMaker, ThreadedLogger Logger)
            : base(conversationState, userState, dialog, logger, decisionMaker)
        {
            _Logger = Logger;
            _decisionMaker = decisionMaker;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            //foreach (var member in membersAdded)
            //{                
            //    if (member.Id != turnContext.Activity.Recipient.Id)
            //    {
            //        //_decisionMaker
            //        var msg = //"Привіт, друже! Мене звати RDBot, Ваш персональний помічник!";
            //            "Hello Friend! My name is RDBot, your personal assistant!";
                    
            //        await turnContext.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            //        msg = "Send me something to start conversation.";
            //        await turnContext.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            //    }
            //}
        }


        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            var jsonModel = JsonConvert.SerializeObject(conversationReference).ToString();
            try
            {
                _Logger.AddConversationReference(conversationReference.User.Id, jsonModel);
                //var userFound = _db.User.First(user => user.Id == conversationReference.User.Id);
                //userFound.ConversationReference = jsonModel;
                //_db.SaveChanges();
            }
            catch {
            }
            //.First(user => user.Id == conversationReference.User.Id).ConversationReference = jsonModel;
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }
    }
}

