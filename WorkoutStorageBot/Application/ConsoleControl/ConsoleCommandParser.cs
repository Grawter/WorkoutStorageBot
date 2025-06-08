namespace WorkoutStorageBot.Application.ConsoleControl
{
    internal class ConsoleCommandParser
    {
        internal ConsoleCommandParser(string data)
        {
            Args = data.ToLower().Trim().Split(' ');

            if (Args.Length == 0)
                throw new FormatException("Получена пустая консольная команда");
        }

        internal string Act { get => Args[0]; }
        internal string Object { get => Args[1]; }
        internal IEnumerable<string> AdditionalParameters { get => Args.Skip(2); }
        internal string[] Args { get; private set; }
    }
}