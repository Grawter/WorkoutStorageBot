#region using
using WorkoutStorageBot.Model;
#endregion

namespace WorkoutStorageBot.BusinessLogic.SessionContext
{
    internal class DataManager
    {
        internal Cycle? CurrentCycle { get; private set; }
        internal Day? CurrentDay { get; private set; }
        internal Exercise? CurrentExercise { get; private set; }

        internal List<Exercise>? Exercises { get; private set; }
        internal List<ResultExercise>? ResultExercises { get; private set; }

        internal void SetCycle(string nameCycle, bool isActive, int userInformationId)
        {
            ResetCycle();

            CurrentCycle = new () { NameCycle = nameCycle, IsActive = isActive, UserInformationId = userInformationId };
        }

        internal void SetCycle(Cycle cycle)
        {
            CurrentCycle = cycle;
        }

        internal void SetDay(string nameDay)
        {
            ResetDay();

            CurrentDay = new() { NameDay = nameDay, CycleId = CurrentCycle.Id };
        }

        internal void SetDay(Day day)
        {
            CurrentDay = day;
        }

        internal void SetExercise(Exercise exercise)
        {
            CurrentExercise = exercise;
        }

        internal bool TryAddExercise(string nameExercise)
        {
            if (Exercises == null)
            {
                Exercises = new () { new Exercise { NameExercise = nameExercise, DayId = CurrentDay.Id } };
                return true;
            }

            if (Exercises.Any(e => e.NameExercise == nameExercise))
                return false;

            Exercises.Add(new Exercise { NameExercise = nameExercise, DayId = CurrentDay.Id });
            return true;
        }

        internal void AddResultExercise(ResultExercise? resultExercise)
        {
            if (resultExercise == null)
                throw new FormatException();

            resultExercise.ExerciseId = CurrentExercise.Id;

            if (ResultExercises == null)
               ResultExercises = new() { resultExercise };
            else
                ResultExercises.Add(resultExercise);
        }

        internal void ResetCycle()
        {
            CurrentCycle = null;
        }

        internal void ResetDay()
        {
            CurrentDay = null;
        }

        internal void ResetExercise()
        {
            CurrentExercise = null;
        }

        internal void ResetExercises()
        {
            Exercises = null;
        }

        internal void ResetResultExercises()
        {
            ResultExercises = null;
        }

        internal void ResetAll()
        {
            ResetCycle();
            ResetDay();
            ResetExercise();
            ResetExercises();
            ResetResultExercises();
        }
    }
}