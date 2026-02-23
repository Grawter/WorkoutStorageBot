using WorkoutStorageModels.Entities.BusinessLogic;

namespace WorkoutStorageImport.Models
{
    public class DTOUserInformation
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public List<Cycle> Cycles { get; set; } = new();
        public bool WhiteList { get; set; }
        public bool BlackList { get; set; }
    }
}