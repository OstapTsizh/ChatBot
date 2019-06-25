using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Template.Core.Interfaces
{
    public interface IUserAnswerValidator
    {
        Task<bool> ValidateUserPrompt<T>(PromptValidatorContext<IList<T>> promptContext,
            CancellationToken cancellationToken);
    }
}
