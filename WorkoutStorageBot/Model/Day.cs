namespace WorkoutStorageBot.Model
{
    public class Day
    {
        public int Id { get; set; }
        public string NameDay { get; set; }
        public List<Exercise> Exercises { get; set; } = new();

        public int CycleId { get; set; }
        public Cycle? Cycle { get; set; }
    }
}