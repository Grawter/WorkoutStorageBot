using Microsoft.IO;

namespace WorkoutStorageBot.BusinessLogic.Helpers.Stream
{
    internal static class StreamHelper
    {
        internal static RecyclableMemoryStreamManager RecyclableMSManager { get; } = new RecyclableMemoryStreamManager();
    }
}