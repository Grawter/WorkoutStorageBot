#region using

using WorkoutStorageBot.Model.Interfaces;

#endregion

namespace WorkoutStorageBot.Model.DTO.BusinessLogic
{
    internal class DTOCycle : IDTODomain
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<DTODay> Days { get; set; } = new();

        public int UserInformationId { get; set; }
        public DTOUserInformation? UserInformation { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchive { get; set; }
    }
}