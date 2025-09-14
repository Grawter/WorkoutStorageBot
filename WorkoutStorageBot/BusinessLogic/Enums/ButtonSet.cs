

namespace WorkoutStorageBot.BusinessLogic.Enums
{
    internal enum ButtonsSet
    {
        None,

        Main,
            DaysListWithLastWorkout,
            FoundResultsByDate,
            ExercisesListWithLastWorkoutForDay,
            EnableExerciseTimer,
            FixExerciseTimer,
            ResetResultsExercise,
            SaveResultsExercise,

        Settings,
            SettingCycles,
                AddCycle,
                CycleList, 
                    SettingCycle,
                        ArchiveCyclesList,
                        SettingDays,
                            AddDays,
                            DaysList,
                                SettingDay,
                                    ReplaceToCycle,
                                    ArchiveDaysList,
                                    SettingExercises,
                                        AddExercises,
                                            SaveExercises,
                                                 RedirectAfterSaveExercise,
                                        ExercisesList,
                                            SettingExercise,
                                                ChangeType,
                                                ReplaceToDay,
                                                ArchiveExercisesList,

            ResetTempDomains,
            ArchiveList,
            Export,
            ConfirmDelete,

        Period,
        
        Admin,
            AdminLogs,
    }
}