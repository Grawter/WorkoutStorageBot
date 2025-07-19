#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Helpers.Common;
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
            CurrentCycle = new() { Name = nameCycle, IsActive = isActive, UserInformationId = userInformationId };

            return CurrentCycle;
        }

        private void SetCycle(Cycle cycle)
        {
            CurrentCycle = cycle;
        }

        internal Day SetDay(string nameDay)
        {
            CurrentDay = new() { Name = nameDay, CycleId = CurrentCycle.Id };

            return CurrentDay;
        }

        private void SetDay(Day day)
        {
            CurrentDay = day;
        }

        private void SetExercise(Exercise exercise)
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

                default:
                    throw new InvalidOperationException($"Неизвестный тип домена: {domain.GetType().Name}");
            }
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

                default:
                    throw new InvalidOperationException($"Неизвестный тип домена: {domain.GetType().Name}");
            }
        }

        internal IDomain? GetCurrentDomain(DomainType domainType, bool throwEx = true)
            => GetCurrentDomain(domainType.ToString(), throwEx);

        internal IDomain? GetCurrentDomain(string domainType, bool throwEx = true)
        {
            IDomain? domain = domainType switch
            {
                Consts.CommonConsts.Domain.Cycle
                    => CurrentCycle,
                Consts.CommonConsts.Domain.Day
                    => CurrentDay,
                Consts.CommonConsts.Domain.Exercise
                    => CurrentExercise,
                _ => throwEx ? throw new NotImplementedException($"Неожиданный domainTyped: {domainType}") : null,
            };

            if (throwEx)
                domain = CommonHelper.GetIfNotNull(domain);

            return domain;
        }

        internal string ConvertResultExerciseToString(ResultExercise resultExercise)
        {
            if (!string.IsNullOrWhiteSpace(resultExercise.FreeResult))
                return $"=> {resultExercise.FreeResult}";
            else if (resultExercise.Count.HasValue)
            {
                if (resultExercise.Weight.HasValue)
                    return $"Повторения: ({resultExercise.Count}) => Вес: ({resultExercise.Weight})";
                else
                    return $"Повторения: ({resultExercise.Count})";
            }
            else
                throw new InvalidOperationException($"Не удалось отобразить данные для результата упражнения с ID: {resultExercise.Id}, ID упражнения: {resultExercise.ExerciseId}, тип упражнения: {resultExercise.Exercise.Mode.ToString().AddQuotes()}");
        }
    }
}