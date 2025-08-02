#region using

using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.SessionContext;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class SaveExercisesBF : ButtonsFactory
    {
        public SaveExercisesBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            AddInlineButton($"Сохранить упражнения", "2|SaveExercises");
            AddInlineButton($"Сбросить добавленные упражнения", $"2|ResetTempDomains|{CommonConsts.Domain.Exercise}");
        }
    }
}