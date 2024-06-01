#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Helpers;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Helpers.InformationSetForSend;
using WorkoutStorageBot.Model;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler.MessageCommandHandler.Context
{
    internal class TextMessageCH : MessageCH
    {
        internal TextMessageCH(EntityContext db, UserContext userContext, TextMessageConverter requestConverter) : base(db, userContext, requestConverter)
        { }

        internal override TextMessageCH Expectation(params HandlerAction[] handlerActions)
        {
            this.handlerActions = handlerActions;

            return this;
        }

        internal TextMessageCH DefaultCommand()
        {
            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (requestConverter.RemoveCompletely().Convert().ToLower())
            {
                case "/start":
                    if (currentUserContext.ActiveCycle != null)
                    {
                        responseConverter = new ResponseConverter("Выберите интересующий раздел");
                        buttonsSets = (ButtonsSet.Main, ButtonsSet.None);
                    }
                    else
                    {
                        currentUserContext.Navigation.QueryFrom = QueryFrom.Start;

                        responseConverter = new ResponseConverter("Начнём");
                        buttonsSets = (ButtonsSet.AddCycle, ButtonsSet.None);
                    }
                    break;

                default:
                    responseConverter = new ResponseConverter($"Неизвестная команда: {requestConverter.Convert()}", "Для получения разделов воспользуйтесь командой /Start");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                    break;
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal TextMessageCH AddCycleCommand()
        {
            requestConverter.RemoveCompletely().WithoutServiceSymbol();

            if (!TryCheckingCycleName(requestConverter.Convert()))
                return this;

            bool hasActiveCycle = currentUserContext.ActiveCycle == null ? false : true;
            domain = currentUserContext.DataManager.SetCycle(requestConverter.Convert(), !hasActiveCycle, currentUserContext.UserInformation.Id);

            if (!hasActiveCycle)
                currentUserContext.UdpateCycleForce((Cycle)domain);

            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (currentUserContext.Navigation.QueryFrom)
            {
                case QueryFrom.Start:
                    currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddDays;

                    responseConverter = new ResponseConverter($"Цикл {currentUserContext.DataManager.CurrentCycle.Name} сохранён!",
                        $"Введите название тренирочного дня для цикла {currentUserContext.DataManager.CurrentCycle.Name}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                    break;

                case QueryFrom.Settings:
                    currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

                    responseConverter = new ResponseConverter($"Цикл {currentUserContext.DataManager.CurrentCycle.Name} сохранён!",
                        "Выберите дальнейшие действия");
                    buttonsSets = (ButtonsSet.AddDays, ButtonsSet.SettingCycles);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {currentUserContext.Navigation.QueryFrom}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal TextMessageCH AddDaysCommand()
        {
            requestConverter.RemoveCompletely().WithoutServiceSymbol();

            if (!TryCheckingDayName(requestConverter.Convert()))
                return this;

            domain = currentUserContext.DataManager.SetDay(requestConverter.Convert());

            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (currentUserContext.Navigation.QueryFrom)
            {
                case QueryFrom.Start:
                    currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddExercises;

                    responseConverter = new ResponseConverter($"День {currentUserContext.DataManager.CurrentDay.Name} сохранён!",
                        "Введите название упражения для этого дня");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                    break;

                case QueryFrom.Settings:
                    currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

                    responseConverter = new ResponseConverter($"День {currentUserContext.DataManager.CurrentDay.Name} сохранён!");
                    buttonsSets = (ButtonsSet.AddExercises, ButtonsSet.SettingDays);

                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {currentUserContext.Navigation.QueryFrom}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal TextMessageCH AddExercisesCommand()
        {
            requestConverter.RemoveCompletely().WithoutServiceSymbol();

            string[] exercisesNames = requestConverter.GetExercises();

            if (!TryCheckingExercisesNames(exercisesNames))
                return this;

            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            if (!currentUserContext.DataManager.TryAddExercise(exercisesNames))
            {
                responseConverter = new ResponseConverter("Упражнение(я) не зафиксировано(ы)!", "В списке фиксаций уже существует(ют) упражнение(я) с таким(и) названием(ями)",
                    $"Введите другое(ие) название(я) упражнение(й) для дня {currentUserContext.DataManager.CurrentDay.Name}");

                switch (currentUserContext.Navigation.QueryFrom)
                {
                    case QueryFrom.Start:
                        buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                        break;

                    case QueryFrom.Settings:
                        buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercises);
                        break;
                    default:
                        throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {currentUserContext.Navigation.QueryFrom}");
                }

                informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                return this;
            }

            responseConverter = new ResponseConverter("Упражнение(я) зафиксировано(ы)!",
                $"Введите след. упражнение(я) для дня {currentUserContext.DataManager.CurrentDay.Name} либо нажмите \"Сохранить\" для сохранения зафиксированных упражнений");
            buttonsSets = (ButtonsSet.SaveExercises, ButtonsSet.None);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal TextMessageCH AddResultForExerciseCommand()
        {
            requestConverter.RemoveCompletely(45).WithoutServiceSymbol();

            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            try
            {
                currentUserContext.DataManager.AddResultsExercise(requestConverter.GetResultsExercise());
            }
            catch (Exception)
            {
                responseConverter = new ResponseConverter("Неожиданный формат результата",
                    $"Введите результат заново для упражения {currentUserContext.DataManager.CurrentExercise.Name}. Пример ожидаемого ввода: 50 10 или 50 10;50 10;50 10... для множественного ввода");
                buttonsSets = (ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay);

                informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                return this;
            }

            responseConverter = new ResponseConverter("Подход(ы) зафиксирован(ы)",
                $"Введите вес и кол-во повторений след. подхода для упражения {currentUserContext.DataManager.CurrentExercise.Name} " +
                $"либо нажмите \"Сохранить\" для сохранения указанных подходов");
            buttonsSets = (ButtonsSet.SaveResultsExercise, ButtonsSet.Main);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal TextMessageCH ChangeNameCommand(string domainType)
        {
            domain = currentUserContext.GetCurrentDomainFromDataManager(domainType);

            requestConverter.RemoveCompletely(25).WithoutServiceSymbol();

            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (domainType)
            {
                case "Cycle":
                    if (!TryCheckingCycleName(requestConverter.Convert()))
                        return this;

                    responseConverter = new ResponseConverter("Название цикла сохранено!");
                    buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);
                    break;

                case "Day":
                    if (!TryCheckingDayName(requestConverter.Convert()))
                        return this;

                    responseConverter = new ResponseConverter("Название дня сохранено!");
                    buttonsSets = (ButtonsSet.SettingDay, ButtonsSet.DaysList);
                    break;

                case "Exercise":

                    if (!TryCheckingExercisesNames(requestConverter.Convert()))
                        return this;

                    responseConverter = new ResponseConverter("Название сохранено!", "Выберите интересующую настройку для указанного упражнения");
                    buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный {nameof(domainType)} : {domainType}");
            }

            domain.Name = requestConverter.Convert();

            currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        private bool TryCheckingCycleName(string name)
        {
            if (currentUserContext.UserInformation.Cycles.Any(c => c.Name == name))
            {
                ResponseConverter responseConverter = new ResponseConverter("Ошибка при добавлении названия!", $"Цикл с названием {requestConverter.Convert()} уже существует",
                    "Ввведите другое название тренировочного цикла");
                (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.None, ButtonsSet.SettingCycles);

                ClearHandlerAction();

                informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                return false;
            }

            return true;
        }

        private bool TryCheckingDayName(string name)
        {
            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            if (currentUserContext.DataManager.CurrentCycle.Days.Any(d => d.Name == name))
            {
                responseConverter = new ResponseConverter("Ошибка при сохранении!", "В этом цикле уже существует день с таким названием",
                    $"Ввведите другое название дня для цикла {currentUserContext.DataManager.CurrentCycle.Name}");

                switch (currentUserContext.Navigation.QueryFrom)
                {
                    case QueryFrom.Start:
                        buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                        break;

                    case QueryFrom.Settings:
                        buttonsSets = (ButtonsSet.None, ButtonsSet.SettingDays);
                        break;
                    default:
                        throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {currentUserContext.Navigation.QueryFrom}");
                }

                ClearHandlerAction();

                informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                return false;
            }

            return true;
        }

        private bool TryCheckingExercisesNames(params string[] names)
        {
            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            foreach (string name in names)
            {
                if (currentUserContext.DataManager.CurrentDay.Exercises.Any(e => e.Name == name))
                {
                    responseConverter = new ResponseConverter("Ошибка при сохранении!", "В этом дне уже существует упражнение(я) с таким(и) названием(ями)",
                        $"Введите другое(ие) название(я) упражнение(ий) для дня {currentUserContext.DataManager.CurrentDay.Name}");

                    switch (currentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                            break;

                        case QueryFrom.Settings:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercises);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {currentUserContext.Navigation.QueryFrom}");
                    }

                    ClearHandlerAction();

                    informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                    return false;
                }
            }

            return true;
        }
    }
}