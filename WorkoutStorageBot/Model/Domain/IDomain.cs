

namespace WorkoutStorageBot.Model.Domain
{
    public interface IDomain : ILightDomain
    {
        public string Name { get; set; }
        public bool IsArchive { get; set; }
    }
}