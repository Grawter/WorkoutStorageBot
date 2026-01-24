namespace WorkoutStorageBot.Model.DTO.HandlerData.Results
{
    internal class PrimaryHandlerResult : AuthorizedHandlerResult
    {
        internal required bool IsNewContext { get; set; }
    }
}