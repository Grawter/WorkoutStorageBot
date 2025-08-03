#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class ArchiveListBF : ButtonsFactory
    {
        public ArchiveListBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Архивированные циклы", $"2|Archive|{CommonConsts.DomainsAndEntities.Cycles}");
            AddInlineButton("Архивированные дни", $"2|Archive|{CommonConsts.DomainsAndEntities.Days}");
            AddInlineButton("Архивированные упражнения", $"2|Archive|{CommonConsts.DomainsAndEntities.Exercises}");
        }
    }
}