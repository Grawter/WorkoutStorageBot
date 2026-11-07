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
            string domainType = (additionalParameters?["DomainType"]).ThrowIfNullOrWhiteSpace();

            AddInlineButton($"Да, удалить", $"2|ConfirmDelete|{domainType}");
        }
    }
}