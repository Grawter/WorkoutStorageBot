using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Context.Session;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class ArchiveListBF : ButtonsFactory
    {
        internal ArchiveListBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Архивированные циклы", $"2|Archive|{CommonConsts.DomainsAndEntities.Cycles}");
            AddInlineButton("Архивированные дни", $"2|Archive|{CommonConsts.DomainsAndEntities.Days}");
            AddInlineButton("Архивированные упражнения", $"2|Archive|{CommonConsts.DomainsAndEntities.Exercises}");
            AddInlineButton("Вернуться к главному меню", "0|ToMain");
        }
    }
}