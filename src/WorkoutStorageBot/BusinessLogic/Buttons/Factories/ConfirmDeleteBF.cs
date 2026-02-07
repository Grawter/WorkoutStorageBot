using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Core.Extensions;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class ConfirmDeleteBF : ButtonsFactory
    {
        internal ConfirmDeleteBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            string domainName = (additionalParameters?["Name"]).ThrowIfNullOrWhiteSpace();
            string domainType = (additionalParameters?["DomainType"]).ThrowIfNullOrWhiteSpace();

            AddInlineButton($"Да, удалить {domainName}", $"2|ConfirmDelete|{domainType}");
        }
    }
}