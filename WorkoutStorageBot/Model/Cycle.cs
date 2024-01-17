

namespace WorkoutStorageBot.Model
{
    public class Cycle : IDomain
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Day> Days { get; set; } = new();

        public int UserInformationId { get; set; }
        public UserInformation? UserInformation { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchive { get; set; }
    }
}