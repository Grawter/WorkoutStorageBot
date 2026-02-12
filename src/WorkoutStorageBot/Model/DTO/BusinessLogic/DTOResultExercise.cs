using WorkoutStorageBot.Model.Interfaces;

namespace WorkoutStorageBot.Model.DTO.BusinessLogic
{
    internal class DTOResultExercise : IDTOByEntity
    {
        public int Id { get; set; }
        public int? Count { get; set; }
        public float? Weight { get; set; }
        public string? FreeResult { get; set; }
        public DateTime DateTime { get; set; }

        public int ExerciseId { get; set; }
        public DTOExercise? Exercise { get; set; }
    }
}