namespace WorkoutStorageBot.BusinessLogic.Consts
{
    internal class CommonConsts
    {
        public class Domain
        {
            internal const string Cycle = "Cycle";
            internal const string Day = "Day";
            internal const string Exercise = "Exercise";
        }

        public class EventNames
        {
            internal const string StartingBot = "StartingBot";
            internal const string NotSupportedUpdateType = "NotSupportedUpdateType";
            internal const string ExpectedUpdateType = "ExpectedUpdateType";
            internal const string RuntimeError = "RuntimeError";
            internal const string Critical = "CRITICAL";
        }

        public class Common
        {
            internal const string DateTimeFormatDateFirst = "dd.MM.yyyy HH:mm:ss";

            internal const string DateTimeFormatHoursFirst = "HH:mm:ss dd.MM.yyyy";
        }
    }
}