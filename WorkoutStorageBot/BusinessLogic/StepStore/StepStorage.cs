#region using
using WorkoutStorageBot.BusinessLogic.Enums;
#endregion

namespace WorkoutStorageBot.BusinessLogic.StepStore
{
    internal static class StepStorage
    {
        private static List<StepInformation> stepsInfrormation;

        static StepStorage()
        {
            stepsInfrormation = new() {
                new StepInformation(QueryFrom.NoMatter, "Выберите интересующий раздел", ButtonsSet.Main, ButtonsSet.None),
                new StepInformation(QueryFrom.NoMatter, "Выберите тренировочный день", ButtonsSet.DaysListWithLastWorkout, ButtonsSet.Main),
                new StepInformation(QueryFrom.NoMatter, "Выберите упраженение", ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout),


                new StepInformation(QueryFrom.Settings, "Выберите интересующие настройки", ButtonsSet.Settings, ButtonsSet.Main),

                new StepInformation(QueryFrom.Settings, "Выберите интересующие настройки для циклов", ButtonsSet.SettingCycles, ButtonsSet.Settings),
                new StepInformation(QueryFrom.Settings, "Выберите интересующий цикл", ButtonsSet.CycleList, ButtonsSet.SettingCycles),
                new StepInformation(QueryFrom.Settings, "Выберите интересующую настройку для указанного цикла", ButtonsSet.SettingCycle, ButtonsSet.CycleList),

                new StepInformation(QueryFrom.Settings, "Выберите интересующие настройки для дней", ButtonsSet.SettingDays, ButtonsSet.SettingCycle),
                new StepInformation(QueryFrom.Settings, "Выберите интересующий день", ButtonsSet.DaysList, ButtonsSet.SettingDays),
                new StepInformation(QueryFrom.Settings, "Выберите интересующую настройку для указанного дня", ButtonsSet.SettingDay, ButtonsSet.DaysList),

                new StepInformation(QueryFrom.Settings, "Выберите интересующие настройки для упражнений", ButtonsSet.SettingExercises, ButtonsSet.SettingDay),
                new StepInformation(QueryFrom.Settings, "Выберите интересующее упражнение", ButtonsSet.ExercisesList, ButtonsSet.SettingExercises),
                new StepInformation(QueryFrom.Settings, "Выберите интересующую настройку для указанного упражнения", ButtonsSet.SettingExercise, ButtonsSet.ExercisesList),
            };
        }

        internal static StepInformation GetStep(string buttonsSet)
        {
            return stepsInfrormation.FirstOrDefault(SI => SI.ButtonsSet.ToString() == buttonsSet) ?? GetMainStep();
        }

        internal static StepInformation GetMainStep()
        {
            return stepsInfrormation.First();
        }
    }
}