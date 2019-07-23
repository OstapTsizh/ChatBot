using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LoggerService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    public class LocationDialog : CancelAndRestartDialog
    {
        private readonly IDecisionMaker DecisionMaker;
        private readonly ThreadedLogger _myLogger;
        private DialogInfo _DialogInfo;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        private List<Country> _countries;
        private Country _country;
        private readonly bool _onlyInUkraine = true;
        

        public LocationDialog(IDecisionMaker decisionMaker, 
                             ThreadedLogger _myLogger, 
                             DialogInfo dialogInfo, 
                             ConcurrentDictionary<string, ConversationReference> conversationReferences)
            : base(nameof(LocationDialog))
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            _DialogInfo = dialogInfo;
            _conversationReferences = conversationReferences;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new MainMenuDialog(DecisionMaker, _myLogger, _DialogInfo, _conversationReferences));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GetCountryStepAsync,
                GetCityStepAsync,
                CallMenuDialogStepAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        /// <summary>
        /// Generates new QuestionAndAnswerModel and returns ChoicePrompt
        /// or passes on not modified QuestionAndAnswerModel to the next step.
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> GetCountryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _DialogInfo.DialogId = _myLogger.LogDialog(_DialogInfo.UserId).Result;

            _country =new Country();

            //if (_onlyInUkraine)
            //{
            //    _country.CountryName = "Ukraine";
            //    return await stepContext.NextAsync(cancellationToken:cancellationToken);
            //}

            {
                _countries = DecisionMaker.GetCountries(_DialogInfo.Language);
                var choices = new List<Choice>();

                foreach (var country in _countries)
                {
                    choices.Add(new Choice(country.CountryName));
                }

                var msg = "Будь ласка, оберіть необхідну країну:";// "Choose needed country, please.";
                var retryMsg = "Будь ласка, спробуйте ще раз:";// "Try one more time, please:";

                var options = new PromptOptions()
                {
                    Prompt = MessageFactory.Text(msg),
                    RetryPrompt = MessageFactory.Text(retryMsg),
                    Choices = choices,
                    Style = ListStyle.HeroCard
                };

                var message = options.Prompt.Text;
                var sender = "bot";
                var time = stepContext.Context.Activity.Timestamp.Value;

                _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

                return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> GetCityStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            //if (!_onlyInUkraine)
            {
                _country = _countries.FirstOrDefault(c=>c.CountryName==(stepContext.Result as FoundChoice).Value);
            }

            var choices = new List<Choice>();

            foreach (var city in _country.Cities)
            {
                choices.Add(new Choice(city));
            }
            var msg = "Будь ласка, оберіть необхідне місто:";// "Choose needed city, please.";
            var retryMsg = "Будь ласка, спробуйте ще раз:";// "Try one more time, please:";

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text(msg),
                RetryPrompt = MessageFactory.Text(retryMsg),
                Choices = choices,
                Style = ListStyle.HeroCard
            };

            var message = options.Prompt.Text;
            var sender = "bot";
            var time = stepContext.Context.Activity.Timestamp.Value;

            _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> CallMenuDialogStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {

            return await stepContext.BeginDialogAsync(nameof(MainMenuDialog), _country, cancellationToken);
        }


        // ToDo move next methods to the Subscription Dialog



        /// <summary>
        /// Checks if current ConversationReference is in
        /// the collection of ConversationReferences.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private bool CheckConversationReference(Activity activity)
        {
            return _conversationReferences.ContainsKey(activity.GetConversationReference().User.Id);
        }

        /// <summary>
        /// Adds the current ConversationReference into
        /// the collection of ConversationReferences.
        /// </summary>
        /// <param name="activity"></param>
        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        /// <summary>
        /// Deletes the current ConversationReference from
        /// the collection of ConversationReferences.
        /// </summary>
        /// <param name="activity"></param>
        private void DeleteConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.TryRemove(conversationReference.User.Id, out _);
        }

    }
}
