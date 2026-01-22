namespace WorkoutStorageBot.BusinessLogic.Enums
{
    internal enum MessageNavigationTarget
    {
        Default,

        AddCycle,
        AddDays,
        AddExercises,
        AddResultForExercise,
        AddCommentForExerciseTimer,

        ChangeNameCycle,
        ChangeNameDay,
        ChangeNameExercise,
        DeleteResultsExercise,

        FindResultsByDate,
        FindResultsByDateInDay,

        FindLogByID,
        FindLogByEventID,
        SendMessageToUser,
        SendMessagesToActiveUsers,
        SendMessagesToAllUsers,
        ChangeUserState,
        DeleteUser,
    }
}