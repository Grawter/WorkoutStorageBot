﻿

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
        DeleteResultsExercises,

        FindLogByID,
        FindLogByEventID,
        ChangeUserState,
        DeleteUser,
    }
}