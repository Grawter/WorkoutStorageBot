#region using

using WorkoutStorageBot.Model.Interfaces;

#endregion

namespace WorkoutStorageBot.Model.Entities.BusinessLogic
{
    public class UserInformation : IEntity
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public string Firstname { get; set; }
        public string Username { get; set; }
        public List<Cycle> Cycles { get; set; } = new();
        public bool WhiteList { get; set; }
        public bool BlackList { get; set; }
    }
}