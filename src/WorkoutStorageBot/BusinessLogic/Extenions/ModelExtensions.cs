using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Extenions
{
    internal static class ModelExtensions
    {
        internal static DTOUserInformation ToDTOUserInformation(this UserInformation userInformation)
        {
            ArgumentNullException.ThrowIfNull(userInformation);

            DTOUserInformation newDTOUserInformation = new DTOUserInformation()
            {
                Id = userInformation.Id,
                UserId = userInformation.UserId,
                FirstName = userInformation.FirstName,
                Username = userInformation.Username,
                WhiteList = userInformation.WhiteList,
                BlackList = userInformation.BlackList,
            };

            newDTOUserInformation.Cycles = userInformation.Cycles.Count > 0
                ? userInformation.Cycles.Select(x => x.ToDTOCycle(newDTOUserInformation)).ToList()
                : new List<DTOCycle>();

            return newDTOUserInformation;
        }

        internal static DTOCycle ToDTOCycle(this Cycle cycle, DTOUserInformation userInformation)
        {
            ArgumentNullException.ThrowIfNull(userInformation);

            DTOCycle newDTOCycle = cycle.ToDTOCycle();
            newDTOCycle.UserInformation = userInformation;

            return newDTOCycle;
        }

        internal static DTOCycle ToDTOCycle(this Cycle cycle)
        {
            ArgumentNullException.ThrowIfNull(cycle);

            DTOCycle newDTOCycle = new DTOCycle()
            {
                Id = cycle.Id,
                Name = cycle.Name,
                UserInformationId = cycle.UserInformationId,
                IsActive = cycle.IsActive,
                IsArchive = cycle.IsArchive,
            };

            newDTOCycle.Days = cycle.Days.Count > 0
                ? cycle.Days.Select(x => x.ToDTODay(newDTOCycle)).ToList()
                : new List<DTODay>();

            return newDTOCycle;
        }

        internal static DTODay ToDTODay(this Day day, DTOCycle cycle)
        {
            ArgumentNullException.ThrowIfNull(cycle);

            DTODay newDTODay = day.ToDTODay();
            newDTODay.Cycle = cycle;

            return newDTODay;
        }

        internal static DTODay ToDTODay(this Day day)
        {
            ArgumentNullException.ThrowIfNull(day);

            DTODay newDTODay = new DTODay()
            {
                Id = day.Id,
                Name = day.Name,
                CycleId = day.CycleId,
                IsArchive = day.IsArchive,
            };

            newDTODay.Exercises = day.Exercises.Count > 0
                ? day.Exercises.Select(x => x.ToDTOExercise(newDTODay)).ToList()
                : new List<DTOExercise>();

            return newDTODay;
        }

        internal static DTOExercise ToDTOExercise(this Exercise exercise, DTODay day)
        {
            ArgumentNullException.ThrowIfNull(day);

            DTOExercise newDTOExercise = exercise.ToDTOExercise();
            newDTOExercise.Day = day;

            return newDTOExercise;
        }

        internal static DTOExercise ToDTOExercise(this Exercise exercise)
        {
            ArgumentNullException.ThrowIfNull(exercise);

            DTOExercise newDTOExercise = new DTOExercise()
            {
                Id = exercise.Id,
                Name = exercise.Name,
                Mode = exercise.Mode,
                IsArchive = exercise.IsArchive,
                DayId = exercise.DayId,
            };

            newDTOExercise.ResultsExercise = exercise.ResultsExercise.Count > 0
                ? exercise.ResultsExercise.Select(x => x.ToDTOResultExercise(newDTOExercise)).ToList()
                : new List<DTOResultExercise>();

            return newDTOExercise;
        }

        internal static DTOResultExercise ToDTOResultExercise(this ResultExercise resultExercise, DTOExercise exercise)
        {
            ArgumentNullException.ThrowIfNull(exercise);

            DTOResultExercise newDTOResultExercise = resultExercise.ToDTOResultExercise();
            newDTOResultExercise.Exercise = exercise;

            return newDTOResultExercise;
        }

        internal static DTOResultExercise ToDTOResultExercise(this ResultExercise resultExercise)
        {
            ArgumentNullException.ThrowIfNull(resultExercise);

            return new DTOResultExercise()
            {
                Id = resultExercise.Id,
                Count = resultExercise.Count,
                Weight = resultExercise.Weight,
                FreeResult = resultExercise.FreeResult,
                DateTime = resultExercise.DateTime,
                ExerciseId = resultExercise.ExerciseId,
            };
        }

        internal static UserInformation ToUserInformation(this DTOUserInformation userInformation)
        {
            ArgumentNullException.ThrowIfNull(userInformation);

            UserInformation newUserInformation = new UserInformation()
            {
                Id = userInformation.Id,
                UserId = userInformation.UserId,
                FirstName = userInformation.FirstName,
                Username = userInformation.Username,
                WhiteList = userInformation.WhiteList,
                BlackList = userInformation.BlackList,
            };

            newUserInformation.Cycles = userInformation.Cycles.Count > 0
                ? userInformation.Cycles.Select(x => x.ToCycle(newUserInformation)).ToList()
                : new List<Cycle>();

            return newUserInformation;
        }

        internal static Cycle ToCycle(this DTOCycle cycle, UserInformation userInformation)
        {
            ArgumentNullException.ThrowIfNull(userInformation);

            Cycle newCycle = cycle.ToCycle();
            newCycle.UserInformation = userInformation;

            return newCycle;
        }

        internal static Cycle ToCycle(this DTOCycle cycle)
        {
            ArgumentNullException.ThrowIfNull(cycle);

            Cycle newCycle = new Cycle()
            {
                Id = cycle.Id,
                Name = cycle.Name,
                UserInformationId = cycle.UserInformationId,
                IsActive = cycle.IsActive,
                IsArchive = cycle.IsArchive,
            };

            newCycle.Days = cycle.Days.Count > 0
                ? cycle.Days.Select(x => x.ToDay(newCycle)).ToList()
                : new List<Day>();

            return newCycle;
        }

        internal static Day ToDay(this DTODay day, Cycle cycle)
        {
            ArgumentNullException.ThrowIfNull(cycle);

            Day newDay = day.ToDay();
            newDay.Cycle = cycle;

            return newDay;
        }

        internal static Day ToDay(this DTODay day)
        {
            ArgumentNullException.ThrowIfNull(day);

            Day newDay = new Day()
            {
                Id = day.Id,
                Name = day.Name,
                CycleId = day.CycleId,
                IsArchive = day.IsArchive,
            };

            newDay.Exercises = day.Exercises.Count > 0
                ? day.Exercises.Select(x => x.ToExercise(newDay)).ToList()
                : new List<Exercise>();

            return newDay;
        }

        internal static Exercise ToExercise(this DTOExercise exercise, Day day)
        {
            ArgumentNullException.ThrowIfNull(day);

            Exercise newExercise = exercise.ToExercise();
            newExercise.Day = day;

            return newExercise;
        }

        internal static Exercise ToExercise(this DTOExercise exercise)
        {
            ArgumentNullException.ThrowIfNull(exercise);

            Exercise newExercise = new Exercise()
            {
                Id = exercise.Id,
                Name = exercise.Name,
                Mode = exercise.Mode,
                IsArchive = exercise.IsArchive,
                DayId = exercise.DayId,
            };

            newExercise.ResultsExercise = exercise.ResultsExercise.Count > 0
                ? exercise.ResultsExercise.Select(x => x.ToResultExercise(newExercise)).ToList()
                : new List<ResultExercise>();

            return newExercise;
        }

        internal static ResultExercise ToResultExercise(this DTOResultExercise resultExercise, Exercise exercise)
        {
            ArgumentNullException.ThrowIfNull(exercise);

            ResultExercise newResultExercise = resultExercise.ToResultExercise();
            newResultExercise.Exercise = exercise;

            return newResultExercise;
        }

        internal static ResultExercise ToResultExercise(this DTOResultExercise resultExercise)
        {
            ArgumentNullException.ThrowIfNull(resultExercise);

            return new ResultExercise()
            {
                Id = resultExercise.Id,
                Count = resultExercise.Count,
                Weight = resultExercise.Weight,
                FreeResult = resultExercise.FreeResult,
                DateTime = resultExercise.DateTime,
                ExerciseId = resultExercise.ExerciseId,
            };
        }
    }
}