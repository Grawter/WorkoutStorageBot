

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

        FindLogByID,
        FindLogByEventID,
        ChangeUserState,
        DeleteUser,
    }
}