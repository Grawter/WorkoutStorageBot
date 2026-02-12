using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Core.Extensions;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.Interfaces;

namespace WorkoutStorageBot.BusinessLogic.Context.Session
{
    internal class DataManager
    {
        internal DTOCycle? CurrentCycle { get; private set; }
        internal DTODay? CurrentDay { get; private set; }
        internal DTOExercise? CurrentExercise { get; private set; }

        internal List<DTOExercise>? TempExercises { get; private set; }
        internal List<DTOResultExercise>? TempResultsExercise { get; private set; }

        internal DateTime ExerciseTimer { get; private set; }

        internal DTOCycle CreateAndSetCurrentCycle(string nameCycle, bool isActive, DTOUserInformation userInformation)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(nameCycle);

            CurrentCycle = new() { Name = nameCycle, IsActive = isActive, UserInformation = userInformation, UserInformationId = userInformation.Id };

            return CurrentCycle;
        }

        private void SetCurrentCycle(DTOCycle cycle)
        {
            CurrentCycle = cycle;
        }

        internal DTODay CreateAndSetCurrentDay(string nameDay)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(nameDay);

            CurrentDay = new() { Name = nameDay, Cycle = CurrentCycle, CycleId = CurrentCycle.ThrowIfNull().Id };

            return CurrentDay;
        }

        internal void SetCurrentDateTimeToExerciseTimer()
        {
            ExerciseTimer = DateTime.Now;
        }

        private void SetCurrentDay(DTODay day)
        {
            CurrentDay = day;
        }

        private void SetCurrentExercise(DTOExercise exercise)
        {
            CurrentExercise = exercise;
        }

        internal bool TryAddTempExercises(List<DTOExercise> exercises, out string existingExerciseName)
        {
            if (!exercises.HasItemsInCollection())
                throw new InvalidOperationException($"Получена пустая коллеция {nameof(exercises)}");

            if (TempExercises == null)
                TempExercises = new();

            existingExerciseName = string.Empty;

            CurrentDay.ThrowIfNull();

            List<DTOExercise> newTempExercises = new List<DTOExercise>();

            foreach (DTOExercise exercise in exercises)
            {
                if (TempExercises.Any(e => e.Name == exercise.Name) ||
                    newTempExercises.Any(e => e.Name == exercise.Name))
                {
                    existingExerciseName = exercise.Name;

                    return false;
                }

                exercise.DayId = CurrentDay.Id;

                newTempExercises.Add(exercise);
            }

            TempExercises.AddRange(newTempExercises);

            return true;
        }

        internal void AddTempResultsExercise(List<DTOResultExercise> resultsExercise)
        {
            if (!resultsExercise.HasItemsInCollection())
                throw new InvalidOperationException($"Получена пустая коллеция {nameof(resultsExercise)}");

            if (TempResultsExercise == null)
                TempResultsExercise = new();

            CurrentExercise.ThrowIfNull();

            foreach (DTOResultExercise result in resultsExercise)
            {
                result.ExerciseId = CurrentExercise.Id;
                TempResultsExercise.Add(result);
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

        internal void ResetTempExercises()
        {
            TempExercises = null;
        }

        internal void ResetTempResultsExercise()
        {
            TempResultsExercise = null;
        }

        internal void ResetExerciseTimer()
        {
            ExerciseTimer = DateTime.MinValue;
        }

        private void ResetChildDomains(IDTODomain domain)
        {
            switch (domain)
            {
                case DTOCycle:
                    ResetCurrentDay();
                    ResetCurrentExercise();
                    ResetTempExercises();
                    ResetTempResultsExercise();
                    ResetExerciseTimer();
                    break;

                case DTODay:
                    ResetCurrentExercise();
                    ResetTempExercises();
                    ResetTempResultsExercise();
                    ResetExerciseTimer();
                    break;

                case DTOExercise:
                    ResetTempExercises();
                    ResetTempResultsExercise();
                    ResetExerciseTimer();
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
            ResetTempExercises();
            ResetTempResultsExercise();
            ResetExerciseTimer();
        }

        internal void ResetCurrentDomain(IDTODomain domain)
        {
            switch (domain)
            {
                case DTOCycle:
                    ResetCurrentCycle();
                    break;

                case DTODay:
                    ResetCurrentDay();
                    break;

                case DTOExercise:
                    ResetCurrentExercise();
                    break;

                default:
                    throw new InvalidOperationException($"Неизвестный тип домена: {domain.GetType().Name}");
            }
        }

        internal void SetCurrentDomain(IDTODomain domain)
        {
            switch (domain)
            {
                case DTOCycle cycle:
                    SetCurrentCycle(cycle);
                    break;

                case DTODay day:
                    SetCurrentDay(day);
                    break;

                case DTOExercise exercise:
                    SetCurrentExercise(exercise);
                    break;

                default:
                    throw new InvalidOperationException($"Неизвестный тип домена: {domain.GetType().Name}");
            }

            ResetChildDomains(domain);
        }

        internal IDTODomain GetRequiredCurrentDomain(DomainType domainType)
            => GetCurrentDomain(domainType).ThrowIfNull();

        internal IDTODomain GetRequiredCurrentDomain(string domainType)
            => GetCurrentDomain(domainType).ThrowIfNull();

        internal IDTODomain? GetCurrentDomain(DomainType domainType)
            => GetCurrentDomain(domainType.ToString());

        internal IDTODomain? GetCurrentDomain(string domainType)
        {
            IDTODomain? domain = domainType switch
            {
                Consts.CommonConsts.DomainsAndEntities.Cycle
                    => CurrentCycle,
                Consts.CommonConsts.DomainsAndEntities.Day
                    => CurrentDay,
                Consts.CommonConsts.DomainsAndEntities.Exercise
                    => CurrentExercise,
                _ => throw new NotImplementedException($"Неожиданный domainTyped: {domainType}")
            };

            return domain;
        }
    }
}