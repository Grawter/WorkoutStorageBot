using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Converters
{
    internal static class EntityConverter
    {
        internal static DTOUserInformation ToDTOUserInformation(UserInformation userInformation)
        {
            ArgumentNullException.ThrowIfNull(userInformation);

            DTOUserInformation newDTOUserInformation = new DTOUserInformation()
            {
                Id = userInformation.Id,
                UserId = userInformation.UserId,
                Firstname = userInformation.Firstname,
                Username = userInformation.Username,
                WhiteList = userInformation.WhiteList,
                BlackList = userInformation.BlackList,
            };

            newDTOUserInformation.Cycles = userInformation.Cycles.Count > 0
                ? userInformation.Cycles.Select(x => ToDTOCycle(x, newDTOUserInformation)).ToList()
                : new List<DTOCycle>();

            return newDTOUserInformation;
        }

        internal static DTOCycle ToDTOCycle(Cycle cycle, DTOUserInformation userInformation)
        {
            ArgumentNullException.ThrowIfNull(userInformation);

            DTOCycle newDTOCycle = ToDTOCycle(cycle);
            newDTOCycle.UserInformation = userInformation;

            return newDTOCycle;
        }

        internal static DTOCycle ToDTOCycle(Cycle cycle)
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
                ? cycle.Days.Select(x => ToDTODay(x, newDTOCycle)).ToList()
                : new List<DTODay>();

            return newDTOCycle;
        }

        internal static DTODay ToDTODay(Day day, DTOCycle cycle)
        {
            ArgumentNullException.ThrowIfNull(cycle);

            DTODay newDTODay = ToDTODay(day);
            newDTODay.Cycle = cycle;

            return newDTODay;
        }

        internal static DTODay ToDTODay(Day day)
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
                ? day.Exercises.Select(x => ToDTOExercise(x, newDTODay)).ToList()
                : new List<DTOExercise>();

            return newDTODay;
        }

        internal static DTOExercise ToDTOExercise(Exercise exercise, DTODay day)
        {
            ArgumentNullException.ThrowIfNull(day);

            DTOExercise newDTOExercise = ToDTOExercise(exercise);
            newDTOExercise.Day = day;

            return newDTOExercise;
        }

        internal static DTOExercise ToDTOExercise(Exercise exercise)
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
                ? exercise.ResultsExercise.Select(x => ToDTOResultExercise(x, newDTOExercise)).ToList()
                : new List<DTOResultExercise>();

            return newDTOExercise;
        }

        internal static DTOResultExercise ToDTOResultExercise(ResultExercise resultExercise, DTOExercise exercise)
        {
            ArgumentNullException.ThrowIfNull(exercise);

            DTOResultExercise newDTOResultExercise = ToDTOResultExercise(resultExercise);
            newDTOResultExercise.Exercise = exercise;

            return newDTOResultExercise;
        }

        internal static DTOResultExercise ToDTOResultExercise(ResultExercise resultExercise)
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

        internal static UserInformation ToUserInformation(DTOUserInformation userInformation)
        {
            ArgumentNullException.ThrowIfNull(userInformation);

            UserInformation newUserInformation = new UserInformation()
            {
                Id = userInformation.Id,
                UserId = userInformation.UserId,
                Firstname = userInformation.Firstname,
                Username = userInformation.Username,
                WhiteList = userInformation.WhiteList,
                BlackList = userInformation.BlackList,
            };

            newUserInformation.Cycles = userInformation.Cycles.Count > 0
                ? userInformation.Cycles.Select(x => ToCycle(x, newUserInformation)).ToList()
                : new List<Cycle>();

            return newUserInformation;
        }

        internal static Cycle ToCycle(DTOCycle cycle, UserInformation userInformation)
        {
            ArgumentNullException.ThrowIfNull(userInformation);

            Cycle newCycle = ToCycle(cycle);
            newCycle.UserInformation = userInformation;

            return newCycle;
        }

        internal static Cycle ToCycle(DTOCycle cycle)
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
                ? cycle.Days.Select(x => ToDay(x, newCycle)).ToList()
                : new List<Day>();

            return newCycle;
        }

        internal static Day ToDay(DTODay day, Cycle cycle)
        {
            ArgumentNullException.ThrowIfNull(cycle);

            Day newDay = ToDay(day);
            newDay.Cycle = cycle;

            return newDay;
        }

        internal static Day ToDay(DTODay day)
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
                ? day.Exercises.Select(x => ToExercise(x, newDay)).ToList()
                : new List<Exercise>();

            return newDay;
        }

        internal static Exercise ToExercise(DTOExercise exercise, Day day)
        {
            ArgumentNullException.ThrowIfNull(day);

            Exercise newExercise = ToExercise(exercise);
            newExercise.Day = day;

            return newExercise;
        }

        internal static Exercise ToExercise(DTOExercise exercise)
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
                ? exercise.ResultsExercise.Select(x => ToResultExercise(x, newExercise)).ToList()
                : new List<ResultExercise>();

            return newExercise;
        }

        internal static ResultExercise ToResultExercise(DTOResultExercise resultExercise, Exercise exercise)
        {
            ArgumentNullException.ThrowIfNull(exercise);

            ResultExercise newResultExercise = ToResultExercise(resultExercise);
            newResultExercise.Exercise = exercise;

            return newResultExercise;
        }

        internal static ResultExercise ToResultExercise(DTOResultExercise resultExercise)
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