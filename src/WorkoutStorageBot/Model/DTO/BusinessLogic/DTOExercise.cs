using WorkoutStorageBot.Model.Interfaces;
using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageBot.Model.DTO.BusinessLogic
{
    internal class DTOExercise : IDTODomain
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public required ExercisesMods Mode { get; set; } = ExercisesMods.WeightCount;

        public List<DTOResultExercise> ResultsExercise { get; set; } = new();

        public int DayId { get; set; }
        public DTODay? Day { get; set; }
        public bool IsArchive { get; set; }
    }
}