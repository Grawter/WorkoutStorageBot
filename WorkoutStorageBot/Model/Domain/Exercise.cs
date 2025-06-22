

namespace WorkoutStorageBot.Model.Domain
{
    public class Exercise : IDomain
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public required ExercisesMods Mode { get; set; } = ExercisesMods.WeightCount;

        public List<ResultExercise> ResultExercises { get; set; } = new();

        public int DayId { get; set; }
        public Day? Day { get; set; }
        public bool IsArchive { get; set; }
    }

    public enum ExercisesMods
    {
        Count = 0,
        WeightCount = 1,
        FreeResult = 2
    }
}