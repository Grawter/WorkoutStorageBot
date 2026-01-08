using WorkoutStorageBot.Model.Interfaces;

namespace WorkoutStorageBot.Model.DTO.BusinessLogic
{
    internal class DTODay : IDTODomain
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<DTOExercise> Exercises { get; set; } = new();

        public int CycleId { get; set; }
        public DTOCycle? Cycle { get; set; }
        public bool IsArchive { get; set; }
    }
}