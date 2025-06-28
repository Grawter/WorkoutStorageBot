

namespace WorkoutStorageBot.Model.Domain
{
    public class ResultExercise : ILightDomain
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