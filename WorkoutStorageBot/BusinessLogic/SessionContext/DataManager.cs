#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Model.Entities.BusinessLogic;
using WorkoutStorageBot.Model.Interfaces;
#endregion

namespace WorkoutStorageBot.BusinessLogic.SessionContext
{
    internal class DataManager
    {
        internal Cycle? CurrentCycle { get; private set; }
        internal Day? CurrentDay { get; private set; }
        internal Exercise? CurrentExercise { get; private set; }

        internal List<Exercise>? Exercises { get; private set; }
        internal List<ResultExercise>? ResultsExercise { get; private set; }

        internal DateTime ExerciseTimer { get; private set; }

        internal Cycle SetCurrentCycle(string nameCycle, bool isActive, int userInformationId)
        {
            CurrentCycle = new() { Name = nameCycle, IsActive = isActive, UserInformationId = userInformationId };

            return CurrentCycle;
        }

        private void SetCurrentCycle(Cycle cycle)
        {
            CurrentCycle = cycle;
        }

        internal Day SetCurrentDay(string nameDay)
        {
            CurrentDay = new() { Name = nameDay, CycleId = CurrentCycle.Id };

            return CurrentDay;
        }

        internal void StartExerciseTimer()
        {
            ExerciseTimer = DateTime.Now;
        }

        private void SetCurrentDay(Day day)
        {
            CurrentDay = day;
        }

        private void SetCurrentExercise(Exercise exercise)
        {
            CurrentExercise = exercise;
        }

        internal bool TryAddExercise(List<Exercise> exercises, out string existingExerciseName)
        {
            if (!exercises.HasItemsInCollection())
                throw new InvalidOperationException($"Получена пустая коллеция {nameof(exercises)}");

            if (Exercises == null)
                Exercises = new();

            existingExerciseName = string.Empty;    

            foreach (Exercise exercise in exercises)
            {
                if (Exercises.Any(e => e.Name == exercise.Name))
                {
                    existingExerciseName = exercise.Name;

                    return false;
                }

                exercise.DayId = CurrentDay.Id;

                Exercises.Add(exercise);
            }

            return true;
        }

        internal void AddResultsExercise(List<ResultExercise> resultsExercise)
        {
            if (!resultsExercise.HasItemsInCollection())
                throw new InvalidOperationException($"Получена пустая коллеция {nameof(resultsExercise)}");

            if (ResultsExercise == null)
                ResultsExercise = new();

            foreach (ResultExercise result in resultsExercise)
            {
                result.ExerciseId = CurrentExercise.Id;
                ResultsExercise.Add(result);
            }
        }

        private void ResetCurrentCycle()
        {
            CurrentCycle = null;
        }

        private void ResetCurrentDay()
        {
            CurrentDay = null;
        }

        private void ResetCurrentExercise()
        {
            CurrentExercise = null;
        }

        internal void ResetExercises()
        {
            Exercises = null;
        }

        internal void ResetResultsExercise()
        {
            ResultsExercise = null;
        }

        internal void ResetExerciseTimer()
        {
            ExerciseTimer = DateTime.MinValue;
        }

        private void ResetChildDomains(IDomain domain)
        {
            switch (domain)
            {
                case Cycle:
                    ResetCurrentDay();
                    ResetCurrentExercise();
                    ResetResultsExercise();
                    break;

                case Day:
                    ResetCurrentExercise();
                    ResetResultsExercise();
                    break;

                case Exercise:
                    ResetResultsExercise();
                    break;

                default:
                    throw new InvalidOperationException($"Неизвестный тип домена: {domain.GetType().Name}");
            }
        }

        internal void ResetAll()
        {
            ResetCurrentCycle();
            ResetCurrentDay();
            ResetCurrentExercise();
            ResetExercises();
            ResetResultsExercise();
            ResetExerciseTimer();
        }

        internal void ResetCurrentDomain(IDomain domain)
        {
            switch (domain)
            {
                case Cycle:
                    ResetCurrentCycle();
                    break;

                case Day:
                    ResetCurrentDay();
                    break;

                case Exercise:
                    ResetCurrentExercise();
                    break;

                default:
                    throw new InvalidOperationException($"Неизвестный тип домена: {domain.GetType().Name}");
            }
        }

        internal void SetCurrentDomain(IDomain domain)
        {
            switch (domain)
            {
                case Cycle cycle:
                    SetCurrentCycle(cycle);
                    break;

                case Day day:
                    SetCurrentDay(day);
                    break;

                case Exercise exercise:
                    SetCurrentExercise(exercise);
                    break;

                default:
                    throw new InvalidOperationException($"Неизвестный тип домена: {domain.GetType().Name}");
            }

            ResetChildDomains(domain);
        }

        internal IDomain? GetCurrentDomain(DomainType domainType, bool throwEx = true)
            => GetCurrentDomain(domainType.ToString(), throwEx);

        internal IDomain? GetCurrentDomain(string domainType, bool throwEx = true)
        {
            IDomain? domain = domainType switch
            {
                Consts.CommonConsts.DomainsAndEntities.Cycle
                    => CurrentCycle,
                Consts.CommonConsts.DomainsAndEntities.Day
                    => CurrentDay,
                Consts.CommonConsts.DomainsAndEntities.Exercise
                    => CurrentExercise,
                _ => throwEx ? throw new NotImplementedException($"Неожиданный domainTyped: {domainType}") : null,
            };

            if (throwEx)
                domain = CommonHelper.GetIfNotNull(domain);

            return domain;
        }
    }
}