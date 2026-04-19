namespace WorkoutStorageBot.Core.Logging.OutputWriter
{
    internal class ConsoleWriter : IOutputWriter
    {
        internal void Write(string message) => Console.WriteLine(message);

        void IOutputWriter.Write(string message) => this.Write(message);
    }
}