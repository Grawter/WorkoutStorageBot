

namespace WorkoutStorageBot.BusinessLogic.Consts
{
    internal class CommonConsts
    {
        public class Domain
        {
            internal const string Cycle = "Cycle";
            internal const string Day = "Day";
            internal const string Exercise = "Exercise";
        }

        public class EventNames
        {
            internal const string StartingBot = "StartingBot";
            internal const string NotSupportedUpdateType = "NotSupportedUpdateType";
            internal const string ExpectedUpdateType = "ExpectedUpdateType";
            internal const string RuntimeError = "RuntimeError";
            internal const string Critical = "CRITICAL";
        }

        public class Common
        {
            internal const string DateTimeFormatDateFirst = "dd.MM.yyyy HH:mm:ss";

            internal const string DateTimeFormatHoursFirst = "HH:mm:ss dd.MM.yyyy";
        }

        public class Exercise
        {
            internal const string ExamplesTypesExercise = @$"Доступные типы упражений:
<b>0</b> - только кол-во повторений (например, подтягивания)
<b>1</b> - вес и кол-во повторений (например, жим лёжа)
<b>2</b> - таймер (например, бег)
<b>3</b> - свободный формат результата (например, отработка на груше)

(Тип упражнения всегда можно поменять в <b>настройках</b>)";

            internal const string InputFormatExercise = @"Формат общего ввода: [название]-[тип]. 
Пример единичного ввода: Жим лёжа-0
Пример множественного ввода: Жим лёжа-0;Становая тяга-0;Прыжки на скакалке-2;...";

        }

        public class ResultExercise
        {
            internal const string InputFormatExerciseResultCount = @"Формат общего ввода: [кол-во повторений].
Пример единичного ввода: 25
Пример множественного ввода: 25 10 5";

            internal const string InputFormatExerciseResultWeightCount = @"Формат общего ввода: [вес] [кол-во повторений].
Пример единичного ввода: 25 10
Пример множественного ввода: 25 10;35 10;45 10";

            internal const string InputFormatExerciseResultFreeResult = @"Формат общего ввода: [Свободная запись до 80 символов].
Пример ввода: Пробежка 10 км/1.25 ч.";
        }
    }
}