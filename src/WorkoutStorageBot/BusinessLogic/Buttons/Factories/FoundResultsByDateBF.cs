using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Core.Extensions;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class FoundResultsByDateBF : ButtonsFactory
    {
        internal FoundResultsByDateBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            if (additionalParameters != null)
            {
                string domainTypeForFind = (additionalParameters?["domainTypeForFind"]).ThrowIfNullOrWhiteSpace();

                if (additionalParameters.TryGetValue("trainingDateLessThanFinderDate", out string? dateLessThanFindedDate))
                    AddInlineButton($"Показать результаты тренировки за {dateLessThanFindedDate}", $"1|FindResultsByDate|{domainTypeForFind}|{dateLessThanFindedDate}");

                if (additionalParameters.TryGetValue("trainingDateGreaterThanFinderDate", out string? dateGreaterThanFindedDate))
                    AddInlineButton($"Показать результаты тренировки за {dateGreaterThanFindedDate}", $"1|FindResultsByDate|{domainTypeForFind}|{dateGreaterThanFindedDate}");
            }
        }
    }
}