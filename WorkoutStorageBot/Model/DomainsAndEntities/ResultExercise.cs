

namespace WorkoutStorageBot.Model.DomainsAndEntities
{
    public class ResultExercise : IEntity
    {
        public int Id { get; set; }
        public int? Count { get; set; }
        public float? Weight { get; set; }
        public string? FreeResult { get; set; }
        public DateTime DateTime { get; set; }

        public int ExerciseId { get; set; }
        public Exercise? Exercise { get; set; }
    }
}