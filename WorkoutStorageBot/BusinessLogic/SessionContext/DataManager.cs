#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Model.Domain;
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

        internal Cycle SetCycle(string nameCycle, bool isActive, int userInformationId)
        {
            ResetCycle();

            return CurrentCycle = new() { Name = nameCycle, IsActive = isActive, UserInformationId = userInformationId };
        }

        private void SetCycle(Cycle cycle)
        {
            CurrentCycle = cycle;
        }

        internal Day SetDay(string nameDay)
        {
            ResetDay();

            return CurrentDay = new() { Name = nameDay, CycleId = CurrentCycle.Id };
        }

        private void SetDay(Day day)
        {
            CurrentDay = day;
        }

        private void SetExercise(Exercise exercise)
        {
            CurrentExercise = exercise;
        }

        internal bool TryAddExercise(string[] nameExercises)
        {
            if (Exercises == null)
            {
                Exercises = new();
            }

            foreach (string name in nameExercises)
            {
                if (Exercises.Any(e => e.Name == name))
                    return false;

                Exercises.Add(new Exercise { Name = name, DayId = CurrentDay.Id });
            }

            return true;
        }

        internal void AddResultsExercise(IEnumerable<ResultExercise> resultsExercise)
        {
            if (!resultsExercise.Any())
                throw new FormatException();

            if (ResultExercises == null)
                ResultExercises = new();

            foreach (ResultExercise result in resultsExercise)
            {
                result.ExerciseId = CurrentExercise.Id;
                ResultExercises.Add(result);
            }
        }

        private void ResetCycle()
        {
            CurrentCycle = null;
        }

        private void ResetDay()
        {
            CurrentDay = null;
        }

        private void ResetExercise()
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

        internal void SetDomain(IDomain domain)
        {
            switch (domain)
            {
                case Cycle cycle:
                    SetCycle(cycle);
                    break;

                case Day day:
                    SetDay(day);
                    break;

                case Exercise exercise:
                    SetExercise(exercise);
                    break;
            }
        }

        internal void ResetDomain(IDomain domain)
        {
            switch (domain)
            {
                case Cycle:
                    ResetCycle();
                    break;

                case Day:
                    ResetDay();
                    break;

                case Exercise:
                    ResetExercise();
                    break;
            }
        }

        internal DomainType GetDomainType (IDomain domain)
        {
            return domain switch
            {
                Cycle
                    => DomainType.Cycle,
                Day
                    => DomainType.Day,
                Exercise
                     => DomainType.Exercise,
                _ => throw new NotImplementedException($"Неожиданный domain: {domain.GetType().FullName}")
            };
        }
    }
}