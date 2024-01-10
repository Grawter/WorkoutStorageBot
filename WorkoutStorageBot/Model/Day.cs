

namespace WorkoutStorageBot.Model
{
    public class Day : IDomain
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Exercise> Exercises { get; set; } = new();

        public int CycleId { get; set; }
        public Cycle? Cycle { get; set; }
        public bool IsArchive { get; set; }
    }
}