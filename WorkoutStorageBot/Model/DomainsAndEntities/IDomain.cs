

namespace WorkoutStorageBot.Model.DomainsAndEntities
{
    public interface IDomain : IEntity
    {
        public string Name { get; set; }
        public bool IsArchive { get; set; }
    }
}