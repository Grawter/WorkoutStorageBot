using WorkoutStorageBot.Model.Interfaces;

namespace WorkoutStorageBot.Model.Entities.Import
{
    public class ImportInfo : IEntity
    {
        public int Id { get; set; }
        public required string DomainType { get; set; }
        public int DomainId { get; set; }
        public int UserInformationId { get; set; }
        public DateTime DateTime { get; set; }
    }
}