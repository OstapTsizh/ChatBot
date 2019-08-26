// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using StuddyBot.Core.Models;

namespace StuddyBot.Dialogs
{
    /// <summary>
    /// A dialog to catch specified keywords on each user's activity.
    /// </summary>
    public class CancelAndRestartDialog : ComponentDialog
    {
        private IStatePropertyAccessor<DialogInfo> _dialogInfoStateProperty;

        public CancelAndRestartDialog(string id, IStatePropertyAccessor<DialogInfo> dialogInfoStateProperty)
            : base(id)
        {
            _dialogInfoStateProperty = dialogInfoStateProperty;
        }

        protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;  
            }

            return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text.ToLower();
                var _DialogInfo = await _dialogInfoStateProperty.GetAsync(innerDc.Context);
                switch (text)
                {
                    case "lang":
                        var activeDialogId = _DialogInfo.LastDialogName;

                        await innerDc.ReplaceDialogAsync(nameof(LanguageDialog),
                            cancellationToken: cancellationToken);
                        
                        return new DialogTurnResult(DialogTurnStatus.Waiting);
                    case "restart":
                    case "again":
                    case "new":
                    case "reload":
                        innerDc.Context.Activity.Text = "begin";
                        await innerDc.ReplaceDialogAsync(nameof(LocationDialog), "begin", cancellationToken);
                        //await innerDc.EndDialogAsync(nameof(LoopingDialog), cancellationToken);
                        //await innerDc.BeginDialogAsync(nameof(LoopingDialog), "begin", cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);

                    case "cancel":
                    case "quit":
                    case "q":
                    case "exit":
                        await innerDc.Context.SendActivityAsync($"Cancelling", cancellationToken: cancellationToken);
                        return await innerDc.CancelAllDialogsAsync();

                    case "sub":
                    case "subscription":
                        await innerDc.ReplaceDialogAsync(nameof(SubscriptionDialog),
                            cancellationToken: cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);

                    case "email":
                        await innerDc.ReplaceDialogAsync(nameof(EmailDialog),
                            cancellationToken: cancellationToken);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);
                }
            }

            return null;
        }
    }
}
