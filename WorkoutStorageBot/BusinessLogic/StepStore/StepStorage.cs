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
                new StepInformation(NavigationType.None, "Выберите интересующий вас раздел", ButtonsSet.Main, ButtonsSet.None),
                new StepInformation(NavigationType.None, "Выберите тренировочный день", ButtonsSet.WorkoutDays, ButtonsSet.Main),
                new StepInformation(NavigationType.None, "Выберите упраженение", ButtonsSet.WorkoutExercises, ButtonsSet.WorkoutDays),
                new StepInformation(NavigationType.SetResultForExercise, "Введите вес и количество повторений", ButtonsSet.None, ButtonsSet.WorkoutExercises),
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