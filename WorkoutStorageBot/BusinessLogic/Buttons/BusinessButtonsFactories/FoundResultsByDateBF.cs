using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Helpers.Common;

namespace WorkoutStorageBot.BusinessLogic.Buttons.BusinessButtonsFactories
{
    internal class FoundResultsByDateBF : ButtonsFactory
    {
        public FoundResultsByDateBF(UserContext userContext) : base(userContext)
        {
        }

        internal override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            if (additionalParameters != null)
            {
                string domainTypeForFind = CommonHelper.GetIfNotNullOrWhiteSpace(additionalParameters.GetValueOrDefault("domainTypeForFind"));

                if (additionalParameters.TryGetValue("trainingDateLessThanFindedDate", out string dateLessThanFindedDate))
                    AddInlineButton($"Показать результаты тренировки за {dateLessThanFindedDate}", $"1|FindResultsByDate|{domainTypeForFind}|{dateLessThanFindedDate}");

                if (additionalParameters.TryGetValue("trainingDateGreaterThanFindedDate", out string dateGreaterThanFindedDate))
                    AddInlineButton($"Показать результаты тренировки за {dateGreaterThanFindedDate}", $"1|FindResultsByDate|{domainTypeForFind}|{dateGreaterThanFindedDate}");
            }
        }
    }
}