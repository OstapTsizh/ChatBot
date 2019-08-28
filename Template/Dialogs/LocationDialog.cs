using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmailSender.Interfaces;
using LoggerService;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    public class LocationDialog : ComponentDialog
    {
        private readonly IDecisionMaker DecisionMaker;
        private readonly ThreadedLogger _myLogger;
        
        private IStatePropertyAccessor<DialogInfo> _dialogInfoStateProperty;


        public LocationDialog(IStatePropertyAccessor<DialogInfo> dialogInfoStateProperty, IDecisionMaker decisionMaker, ISubscriptionManager SubscriptionManager,
                             ThreadedLogger _myLogger, 
                             //DialogInfo dialogInfo, 
                             //ConcurrentDictionary<string, ConversationReference> conversationReferences, 
                             IEmailSender emailSender,
                             StuddyBotContext db)
            : base(nameof(LocationDialog))//, dialogInfoStateProperty)
             
        {
            
            this._myLogger = _myLogger;
            DecisionMaker = decisionMaker;
            //_DialogInfo = dialogInfo;
            //_conversationReferences = conversationReferences;
            _dialogInfoStateProperty = dialogInfoStateProperty;

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new MainMenuDialog(dialogInfoStateProperty, DecisionMaker, SubscriptionManager, _myLogger,
                //_DialogInfo,
                //_conversationReferences, 
                db, 
                emailSender));
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
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);
            _DialogInfo.LastDialogName = this.Id;
            var _country =new Country();

            

            {
                var _countries = DecisionMaker.GetCountries(_DialogInfo.Language);
                var choices = new List<Choice>();

                foreach (var country in _countries)
                {
                    choices.Add(new Choice(country.CountryName));
                }

                var dialogsMUI = DecisionMaker.GetDialogsMui(_DialogInfo.Language);

                var prompt = dialogsMUI.LocationDictionary["promptCountry"]; // "Choose needed country, please.";
                var reprompt = dialogsMUI.MainDictionary["reprompt"]; // "Try one more time, please:";
                
                var options = new PromptOptions()
                {
                    Prompt = MessageFactory.Text(prompt),
                    RetryPrompt = MessageFactory.Text(reprompt),
                    Choices = choices,
                    Style = ListStyle.SuggestedAction
                };

                var message = options.Prompt.Text;
                var sender = "bot";
                var time = stepContext.Context.Activity.LocalTimestamp.Value;

                _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

                //await _dialogInfoStateProperty.SetAsync(stepContext.Context, _DialogInfo);

                return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> GetCityStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);

            //if (!_onlyInUkraine)
            
            var _countries = DecisionMaker.GetCountries(_DialogInfo.Language);
            var choice = (stepContext.Result as FoundChoice).Value;
            var _country = _countries.FirstOrDefault(c=>c.CountryName == choice);
            

            _DialogInfo.Country = _country.CountryName;

            var choices = new List<Choice>();

            foreach (var city in _country.Cities)
            {
                choices.Add(new Choice(city));
            }

            var dialogsMUI = DecisionMaker.GetDialogsMui(_DialogInfo.Language);

            var prompt = dialogsMUI.LocationDictionary["promptCity"];// "Choose needed city, please.";
            var reprompt = dialogsMUI.MainDictionary["reprompt"];// "Try one more time, please:";

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text(prompt),
                RetryPrompt = MessageFactory.Text(reprompt),
                Choices = choices,
                Style = ListStyle.SuggestedAction
            };

            var message = options.Prompt.Text;
            var sender = "bot";
            var time = stepContext.Context.Activity.LocalTimestamp.Value;

            _myLogger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            await _dialogInfoStateProperty.SetAsync(stepContext.Context, _DialogInfo);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options, cancellationToken);
        }

        private async Task<DialogTurnResult> CallMenuDialogStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);

            var _countries = DecisionMaker.GetCountries(_DialogInfo.Language);

            var _country = _countries.FirstOrDefault(c => c.CountryName == _DialogInfo.Country);

            {
                _DialogInfo.City = _country.Cities.FirstOrDefault(c => c == (stepContext.Result as FoundChoice).Value);
            }
                       

            //await stepContext.Context.SendActivityAsync(MessageFactory.Text("City successfully selected, starting MainMenuDialog"),
            //       cancellationToken: cancellationToken);



            await _dialogInfoStateProperty.SetAsync(stepContext.Context, _DialogInfo);

            return await stepContext.BeginDialogAsync(nameof(MainMenuDialog), _country, cancellationToken);
        }

    }
}
