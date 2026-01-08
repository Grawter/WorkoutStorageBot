using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class ConfirmDeleteBF : ButtonsFactory
    {
        public ConfirmDeleteBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            string domainName = additionalParameters["Name"];
            string domainType = additionalParameters["DomainType"];

            AddInlineButton($"Да, удалить {domainName}", $"2|ConfirmDelete|{domainType}");
        }
    }
}