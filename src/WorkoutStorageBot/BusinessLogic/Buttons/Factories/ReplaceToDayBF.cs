using WorkoutStorageBot.BusinessLogic.Buttons.Abstraction;
using WorkoutStorageBot.BusinessLogic.Context.Session;
using WorkoutStorageBot.Model.DTO.BusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Buttons.Factories
{
    internal class ReplaceToDayBF : ButtonsFactory
    {
        internal ReplaceToDayBF(UserContext userContext) : base(userContext)
        {
        }

        protected override void AddBusinessButtons(Dictionary<string, string>? additionalParameters = null)
        {
            foreach (DTOCycle cycle in CurrentUserContext.UserInformation.Cycles.Where(c => !c.IsArchive))
            {
                foreach (DTODay day in cycle.Days.Where(c => !c.IsArchive))
                {
                    AddInlineButton($"{day.Name}[Id:{day.Id}, CycleId:{cycle.Id}]", $"2|ReplaceTo|Day|{day.Id}");
                }
            }
        }
    }
}