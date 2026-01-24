using WorkoutStorageModels.Interfaces;

namespace WorkoutStorageModels.Entities.BusinessLogic
{
    public class UserInformation : IEntity
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public required string FirstName { get; set; }
        public required string Username { get; set; }
        public List<Cycle> Cycles { get; set; } = new();
        public bool WhiteList { get; set; }
        public bool BlackList { get; set; }
    }
}