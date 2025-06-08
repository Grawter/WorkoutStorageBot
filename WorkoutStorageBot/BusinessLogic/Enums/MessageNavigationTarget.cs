

namespace WorkoutStorageBot.BusinessLogic.Enums
{
    internal enum MessageNavigationTarget
    {
        Default,

        AddCycle,
        AddDays,
        AddExercises,
        AddResultForExercise, 

        ChangeNameCycle,
        ChangeNameDay,
        ChangeNameExercise,

        FindLogByID,
        FindLogByEventID,
        ChangeUserState,
        DeleteUser,
    }
}