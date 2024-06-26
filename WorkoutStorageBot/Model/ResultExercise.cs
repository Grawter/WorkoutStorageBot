﻿namespace WorkoutStorageBot.Model
{
    public class ResultExercise
    {
        public int Id { get; set; }
        public float Weight { get; set; }
        public float Count { get; set; }
        public DateTime DateTime { get; set; }

        public int ExerciseId { get; set; }
        public Exercise? Exercise { get; set; }
    }
}