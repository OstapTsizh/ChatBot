using System;
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
using Microsoft.Extensions.Configuration;
using StuddyBot.Core.DAL.Data;
using StuddyBot.Core.Interfaces;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    public class LanguageDialog : ComponentDialog
    {
        private readonly IDecisionMaker DecisionMaker;
        private readonly ThreadedLogger _Logger;
        private IStatePropertyAccessor<DialogInfo> _dialogInfoStateProperty;
        private IConfiguration _Configuration;


        public LanguageDialog(IStatePropertyAccessor<DialogInfo> dialogInfoStateProperty, IDecisionMaker decisionMaker, ISubscriptionManager SubscriptionManager,
                             ThreadedLogger _myLogger, 
                             IEmailSender emailSender,
                             StuddyBotContext db,
                             IConfiguration configuration)
            : base(nameof(LanguageDialog))//, dialogInfoStateProperty)
             
        {
            
            _Logger = _myLogger;
            DecisionMaker = decisionMaker;
            
            _dialogInfoStateProperty = dialogInfoStateProperty;
            _Configuration = configuration;

            //AddDialog(new LocationDialog(_dialogInfoStateProperty, DecisionMaker, SubscriptionManager, _Logger,

            //    emailSender));

            //AddDialog(new MainMenuDialog(_dialogInfoStateProperty, DecisionMaker, SubscriptionManager, _Logger,
                
            //    emailSender));


            //AddDialog(new CoursesDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager,
            //    _Logger
            //    ));
            //AddDialog(new ChooseOptionDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager,
            //    _Logger
            //    ));
            //AddDialog(new PlannedEventsDialog(dialogInfoStateProperty, DecisionMaker, _Logger
            //    ));
            //AddDialog(new QAsDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager,
            //    _Logger
            //    ));
            //AddDialog(new SubscriptionDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager,
            //    _Logger
            //    ));
            //AddDialog(new MailingDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager,
            //    _myLogger
            //    ));
            //AddDialog(new FinishDialog(dialogInfoStateProperty, DecisionMaker, emailSender, SubscriptionManager,
            //    _Logger
            //    ));




            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CheckLanguageStepAsync,
                ChangeLanguageStepAsync,
                SetUserLanguageAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> CheckLanguageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);//new DialogState()

            try
            {
                var languageTest = await _Logger.UserLanguageInDb(stepContext.Context.Activity.From.Id);
                if (string.IsNullOrEmpty(languageTest))
                {
                    languageTest = await Task.Run(() => stepContext.Context.Activity.Locale.ToLower());
                }

                  // stepContext.Context.Activity.Locale.ToLower(); "en-us";
                //await stepContext.Context.SendActivityAsync(MessageFactory.Text(languageTest + " - dynamic"),
                //    cancellationToken: cancellationToken);
                _DialogInfo.Language = languageTest;
            }
            catch
            {
                // Get default language for new user
                _DialogInfo.Language = _Configuration.GetValue<string>("DefaultLanguageForNewUser").ToLower();
                //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Error on getting user's language from activity"),
                //    cancellationToken: cancellationToken);
            }

            try
            {
                _DialogInfo.UserId = //stepContext.Context.Activity.From.Id;
                     await _Logger.LogUser(stepContext.Context.Activity.From.Id, _DialogInfo.Language);
            }
            catch (Exception e)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Error on creating user in DB\n" +
                    $"{e.InnerException}"),
                    cancellationToken: cancellationToken);
            }


            try
            {
                //_DialogInfo.DialogId = await  _Logger.LogDialog(_DialogInfo.UserId, _DialogInfo.Language);
                _DialogInfo.DialogId = await _Logger.LogDialog(_DialogInfo.UserId);
            }
            catch (Exception e)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Error on getting user's ID from DB while logging dialog\n" +
                    $"{e.InnerException}"),
                    cancellationToken: cancellationToken);
            }

            string language;

            switch (_DialogInfo.Language)
            {
                case "ru-ru":
                    language = "Русский";
                    break;
                case "uk-ua":
                    language = "Українська";
                    break;
                default:
                    language = "English";
                    break;
            }

            var choices = new List<Choice>();
            {
                choices.Add(new Choice("Yes"));
                choices.Add(new Choice("No"));
            }

            var msg = $"Is your language {language}?";

            var options = new PromptOptions()
            {
                Prompt = MessageFactory.Text(msg),
                Choices = choices,
                Style = ListStyle.SuggestedAction
            };

            var message = options.Prompt.Text;
            var sender = "bot";
            var time = stepContext.Context.Activity.LocalTimestamp.Value;

            _Logger.LogMessage(message, sender, time, _DialogInfo.DialogId);

            await _dialogInfoStateProperty.SetAsync(stepContext.Context, _DialogInfo);

            return await stepContext.PromptAsync(nameof(ChoicePrompt), options,
                cancellationToken: cancellationToken);
        }


        private async Task<DialogTurnResult> ChangeLanguageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);

            var choiceValue = (string)(stepContext.Result as FoundChoice).Value;

            if (choiceValue == "No")
            {
                var choices = new List<Choice>();
                {
                    choices.Add(new Choice("Ukrainian"));
                    choices.Add(new Choice("Russian"));
                    choices.Add(new Choice("English"));
                }

                var msg = "Choose your language:";

                var promptOptions = new PromptOptions()
                {
                    Prompt = MessageFactory.Text(msg),
                    Choices = choices,
                    Style = ListStyle.SuggestedAction
                };

                var promptMessage = promptOptions.Prompt.Text;
                var promptSender = "bot";
                var promptTime = stepContext.Context.Activity.LocalTimestamp.Value;

                _Logger.LogMessage(promptMessage, promptSender, promptTime, _DialogInfo.DialogId);


                await _dialogInfoStateProperty.SetAsync(stepContext.Context, _DialogInfo);

                return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions,
                    cancellationToken: cancellationToken);
            }

            //await _dialogInfoStateProperty.SetAsync(stepContext.Context, _DialogInfo);

            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }


        /// <summary>
        /// Starts child (Location) dialog after retrieving UserId
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> SetUserLanguageAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var _DialogInfo = await _dialogInfoStateProperty.GetAsync(stepContext.Context);

            var choiceValue = (stepContext.Result as FoundChoice)?.Value;

            if (!string.IsNullOrEmpty(choiceValue))
            {
                switch (choiceValue)
                {
                    case "Russian":
                        _DialogInfo.Language = "ru-ru";
                        break;
                    case "Ukrainian":
                        _DialogInfo.Language = "uk-ua";
                        break;
                    default:
                        _DialogInfo.Language = "en-us";
                        break;
                }
            }

            try
            {
                _Logger.ChangeUserLanguage(_DialogInfo.UserId, _DialogInfo.Language);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Error on saving user's Language to DB" +
                    $"\nuserID: {_DialogInfo.UserId}\nlang: {_DialogInfo.Language}"),
                    cancellationToken: cancellationToken);
            }

            await _dialogInfoStateProperty.SetAsync(stepContext.Context, _DialogInfo);

            //if (_DialogInfo.LastDialogName == string.Empty)
            //{
            //    return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);// .BeginDialogAsync(nameof(LocationDialog), cancellationToken: cancellationToken);
            //}

            return await stepContext.ReplaceDialogAsync(nameof(LocationDialog), cancellationToken: cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);// .BeginDialogAsync(nameof(LocationDialog), cancellationToken: cancellationToken);
        }

    }
}
