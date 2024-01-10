

namespace WorkoutStorageBot.Model
{
    public interface IDomain
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsArchive { get; set; }
    }
}