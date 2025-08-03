#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class SettingExerciseBF : ButtonsFactory
    {
        public SettingExerciseBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton("Сменить название", $"2|ChangeName|{CommonConsts.DomainsAndEntities.Exercise}");
            AddInlineButton("Сменить тип", $"2|ChangeMode|{CommonConsts.DomainsAndEntities.Exercise}");
            AddInlineButton("Перенести упражнение", $"2|Replace|{CommonConsts.DomainsAndEntities.Exercise}");
            AddInlineButton("Добавить в архив", $"2|Archiving|{CommonConsts.DomainsAndEntities.Exercise}");
            AddInlineButton("Удалить", $"2|Delete|{CommonConsts.DomainsAndEntities.Exercise}");
            AddInlineButton("Удалить результаты упражнения", $"2|Delete|{CommonConsts.DomainsAndEntities.ResultsExercise}");
            AddInlineButton("Вернуться к главному меню", "0|ToMain");
        }
    }
}