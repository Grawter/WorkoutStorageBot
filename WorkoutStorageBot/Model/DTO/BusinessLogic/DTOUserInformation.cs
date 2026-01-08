using WorkoutStorageBot.Model.Interfaces;

namespace WorkoutStorageBot.Model.DTO.BusinessLogic
{
    internal class DTOUserInformation : IDTOByEntity
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public string Firstname { get; set; }
        public string Username { get; set; }
        public List<DTOCycle> Cycles { get; set; } = new();
        public bool WhiteList { get; set; }
        public bool BlackList { get; set; }
    }
}