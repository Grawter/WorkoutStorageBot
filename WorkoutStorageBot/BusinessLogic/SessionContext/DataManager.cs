#region using
using WorkoutStorageBot.Model;

#endregion

namespace WorkoutStorageBot.BusinessLogic.SessionContext
{
    internal class DataManager
    {
        internal DataManager()
        {
            CurrentCycle = new();
            Exercises = new();
            ResultExercises = new();

            NumberDay = 1;
        }

        private List<Exercise> Exercises { get; set; }
        internal List<ResultExercise> ResultExercises { get; private set; }


        private Cycle CurrentCycle { get; set; }
        internal Day CurrentDay { get; private set; }
        internal Exercise CurrentExercise { get; private set; }

        internal uint NumberDay { get; private set; }

        internal void AddExercise(string nameExercise)
        {
            Exercises.Add(new Exercise { NameExercise = nameExercise });
        }

        internal void AddDay()
        {
            var day = new Day { NameDay = NumberDay.ToString() };
            day.Exercises.AddRange(Exercises);

            CurrentCycle.Days.Add(day);

            ++NumberDay;

            Exercises.Clear();
        }

        internal void AddResultExercise(ResultExercise resultExercise)
        {
            resultExercise.ExerciseId = CurrentExercise.Id;
            ResultExercises.Add(resultExercise);
        }

        internal void SetCycle(string name, int id)
        {
            CurrentCycle.NameCycle = name;
            CurrentCycle.UserInformationId = id;
        }

        internal void SetDay(string name, string id)
        {
            CurrentDay = new Day { NameDay = name, Id = int.Parse(id) };
        }

        internal void SetExercise(string name, string id)
        {
            CurrentExercise = new Exercise { NameExercise = name, Id = int.Parse(id) };
        }

        internal Cycle GetCycleWithUiId(int userInformationId)
        {
            if (Exercises.Count > 0)
                Exercises.Clear();

            NumberDay = 1;

            CurrentCycle.UserInformationId = userInformationId;
            CurrentCycle.IsActive = true;

            return CurrentCycle;
        }

        internal void ResetCycle()
        {
            CurrentCycle = new();
        }

        internal void ResetResultExercises()
        {
            ResultExercises.Clear();
        }
    }
}