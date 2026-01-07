#region using

#endregion

namespace WorkoutStorageBot.Model.Interfaces
{
    internal interface IDTODomain : IDTOByEntity
    {
        public string Name { get; set; }
        public bool IsArchive { get; set; }
    }
}