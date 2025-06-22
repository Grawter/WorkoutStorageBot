﻿

namespace WorkoutStorageBot.Model.Logging
{
    public class Log
    {
        public int Id { get; set; }

        public required string LogLevel { get; set; }

        public int? EventID { get; set; }

        public string? EventName { get; set; }

        public DateTime DateTime { get; set; }

        public required string Message { get; set; }

        public required string SourceContext { get; set; }

        public long? TelegaramUserId { get; set; }
    }
}