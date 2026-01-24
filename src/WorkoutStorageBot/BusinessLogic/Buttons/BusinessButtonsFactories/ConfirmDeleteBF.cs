using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Core.Extensions;

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class ConfirmDeleteBF : ButtonsFactory
    {
        public ConfirmDeleteBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            string domainName = (additionalParameters?["Name"]).ThrowIfNullOrWhiteSpace();
            string domainType = (additionalParameters?["DomainType"]).ThrowIfNullOrWhiteSpace();

            AddInlineButton($"Да, удалить {domainName}", $"2|ConfirmDelete|{domainType}");
        }
    }
}