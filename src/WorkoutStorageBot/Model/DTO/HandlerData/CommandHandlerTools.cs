using WorkoutStorageBot.Core.Repositories.Store;
using WorkoutStorageBot.Model.DTO.HandlerData.Results;

namespace WorkoutStorageBot.Model.DTO.HandlerData
{
    internal class CommandHandlerTools
    {
        internal required CoreTools CoreTools { get; init; }
        internal required AuthorizedHandlerResult HandlerResult { get; init; }
        internal required RepositoriesStore RepositoriesStore { get; init; }
    }
}