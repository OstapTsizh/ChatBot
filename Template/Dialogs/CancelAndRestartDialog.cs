// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.3.0

using System;
using System.Threading;
using System.Threading.Tasks;
using LoggerService;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using StuddyBot.Core.DAL.Entities;

namespace StuddyBot.Dialogs
{
    public class CancelAndRestartDialog : ComponentDialog
    {
        //protected int _dialogId;
        //protected ThreadedLogger _myLogger;
        public CancelAndRestartDialog(string id)//, MyDialog myDialog
            : base(id)
        {
            //_dialogId = dialogId;
            //_myLogger = myLogger;
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
            //logDialog(innerDc.Context.Activity.Text, innerDc.Context.Activity.Timestamp.Value);
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

                switch (text)
                {
                    case "restart":
                    case "again":
                    case "new":
                    case "reload":
                        await innerDc.ReplaceDialogAsync(nameof(MainDialog), cancellationToken);
                        Thread.Sleep(500);
                        return new DialogTurnResult(DialogTurnStatus.Waiting);

                    case "cancel":
                    case "quit":
                    case "q":
                    case "exit":
                        await innerDc.Context.SendActivityAsync($"Cancelling", cancellationToken: cancellationToken);
                        return await innerDc.CancelAllDialogsAsync();
                }
            }

            return null;
        }

        //private void logDialog(string message, DateTimeOffset time)
        //{

        //    _myLogger.LogMessage(message, "user", time, _dialogId);
        //}

    }
}
