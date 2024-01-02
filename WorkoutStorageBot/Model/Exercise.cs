namespace WorkoutStorageBot.Model
{
    public class Exercise
    {
        public int Id { get; set; }
        public string NameExercise { get; set; }
        public List<ResultExercise> ResultExercises { get; set; } = new();

        public int DayId { get; set; }
        public Day? Day { get; set; }
    }
}