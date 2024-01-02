namespace WorkoutStorageBot.Model
{
    public class Cycle
    {
        public int Id { get; set; }
        public string NameCycle { get; set; }
        public List<Day> Days { get; set; } = new();

        public int UserInformationId { get; set; }
        public UserInformation? UserInformation { get; set; }
        public bool IsActive { get; set; }
    }
}