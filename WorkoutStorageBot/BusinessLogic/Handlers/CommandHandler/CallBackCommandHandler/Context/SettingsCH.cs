#region using
using System.Text;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Helpers.Export;
using WorkoutStorageBot.Model;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler.CallBackCommandHandler
{
    internal class SettingsCH : CallBackCH
    {
        internal SettingsCH(EntityContext db, UserContext userContext, CallbackQueryParser callbackQueryParser) 
            : base(db, userContext, callbackQueryParser)
        { }

        internal override SettingsCH Expectation(params HandlerAction[] handlerActions)
        {
            this.handlerActions = handlerActions;

            return this;
        }

        internal SettingsCH SettingsCommand()
        {
            currentUserContext.Navigation.QueryFrom = QueryFrom.Settings;

            ResponseConverter responseConverter = new ResponseConverter("Выберите интересующие настройки");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Settings, ButtonsSet.Main);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ArchiveStoreCommand()
        {
            ResponseConverter responseConverter = new ResponseConverter("Выберите интересующий архив для разархивирования");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ArchiveList, ButtonsSet.Settings);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ArchiveCommand()
        {
            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycles":
                    responseConverter = new ResponseConverter("Выберите архивный цикл для разархивирования");
                    buttonsSets = (ButtonsSet.ArchiveCyclesList, ButtonsSet.ArchiveList);
                    break;
                case "Days":
                    responseConverter = new ResponseConverter("Выберите архивный день для разархивирования");
                    buttonsSets = (ButtonsSet.ArchiveDaysList, ButtonsSet.ArchiveList);
                    break;
                case "Exercises":
                    responseConverter = new ResponseConverter("Выберите архивное упражнение для разархивирования");
                    buttonsSets = (ButtonsSet.ArchiveExercisesList, ButtonsSet.ArchiveList);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH UnArchiveCommand()
        {
            domain = db.GetDomainWithId(callbackQueryParser.DomainId, callbackQueryParser.DomainType);
            domain.IsArchive = false;

            ResponseConverter responseConverter = new ResponseConverter($"{domain.Name} разархивирован!");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ArchiveList, ButtonsSet.Settings);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ExportCommand()
        {
            ResponseConverter responseConverter = new ResponseConverter("Выберите формат в котором экспортировать данные о ваших  тренировках");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Export, ButtonsSet.Settings);
 
            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ExportToCommand()
        {
            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;
            Dictionary<string, string> additionalParameters;

            switch (callbackQueryParser.DomainType)
            {
                case "Excel":
                    responseConverter = new ResponseConverter("Выберите временной промежуток формирования данных от последней тренировки");
                    buttonsSets = (ButtonsSet.Period, ButtonsSet.Settings);
                    additionalParameters = new() { { "Act", "Export/Excel" } };
                    break;
                case "JSON":
                    responseConverter = new ResponseConverter("Выберите временной промежуток формирования данных от последней тренировки");
                    buttonsSets = (ButtonsSet.Period, ButtonsSet.Settings);
                    additionalParameters = new() { { "Act", "Export/JSON" } };
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets, additionalParameters);

            return this;
        }

        internal SettingsCH SettingCommand()
        {
            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycles":
                    responseConverter = new ResponseConverter("Выберите интересующие настройки для циклов");
                    buttonsSets = (ButtonsSet.SettingCycles, ButtonsSet.Settings);
                    break;
                case "Days":
                    responseConverter = new ResponseConverter("Выберите интересующие настройки для дней");
                    buttonsSets = (ButtonsSet.SettingDays, ButtonsSet.SettingCycle);
                    break;
                case "Exercises":
                    responseConverter = new ResponseConverter("Выберите интересующие настройки для упражнений");
                    buttonsSets = (ButtonsSet.SettingExercises, ButtonsSet.SettingDay);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH AddCommand()
        {
            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycle":
                    currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddCycle;

                    responseConverter = new ResponseConverter("Введите название тренировочного цикла");

                    switch (currentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                            break;

                        case QueryFrom.Settings:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.SettingCycles);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {currentUserContext.Navigation.QueryFrom}");
                    }
                    break;

                case "Day":
                    currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddDays;

                    responseConverter = new ResponseConverter($"Введите название тренирочного дня для цикла {currentUserContext.DataManager.CurrentCycle.Name}");

                    switch (currentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                            break;

                        case QueryFrom.Settings:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.SettingDay);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {currentUserContext.Navigation.QueryFrom}");
                    }
                    break;

                case "Exercise":
                    currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddExercises;

                    responseConverter = new ResponseConverter($"Введите название(я) упражнение(я) для дня {currentUserContext.DataManager.CurrentDay.Name}",
                        "Формат для множественного ввода название;название;название...");
                    switch (currentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                            break;

                        case QueryFrom.Settings:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercise);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {currentUserContext.Navigation.QueryFrom}");
                    }
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH SaveExercisesCommand()
        {
            db.Exercises.AddRange(currentUserContext.DataManager.Exercises);

            currentUserContext.DataManager.ResetExercises();

            currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (currentUserContext.Navigation.QueryFrom)
            {
                case QueryFrom.Start:
                    responseConverter = new ResponseConverter("Упражнения сохранены!", "Выберите дальнейшее действие");
                    buttonsSets = (ButtonsSet.RedirectAfterSaveExercise, ButtonsSet.None);
                    break;

                case QueryFrom.Settings:
                    responseConverter = new ResponseConverter("Упражнения сохранены!", "Выберите интересующие настройки для упражнений");
                    buttonsSets = (ButtonsSet.SettingExercises, ButtonsSet.SettingDays);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {currentUserContext.Navigation.QueryFrom}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH SettingExistingCommand()
        {
            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycles":
                    responseConverter = new ResponseConverter("Выберите интересующий цикл");
                    buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                    break;
                case "Days":
                    responseConverter = new ResponseConverter("Выберите интересующий день");
                    buttonsSets = (ButtonsSet.DaysList, ButtonsSet.SettingDays);
                    break;
                case "Exercises":
                    responseConverter = new ResponseConverter("Выберите интересующее упражнение");
                    buttonsSets = (ButtonsSet.ExercisesList, ButtonsSet.SettingExercises);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH SelectedCommand()
        {
            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycle":
                    currentUserContext.DataManager.SetDomain(currentUserContext.UserInformation.Cycles.First(c => c.Id == callbackQueryParser.DomainId));

                    responseConverter = new ResponseConverter($"Выберите интересующую настройку для цикла {currentUserContext.DataManager.CurrentCycle.Name}");
                    buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);
                    break;

                case "Day":
                    switch (currentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:
                            currentUserContext.DataManager.SetDomain(currentUserContext.ActiveCycle.Days.First(d => d.Id == callbackQueryParser.DomainId));

                            responseConverter = new ResponseConverter("Выберите упраженение");
                            buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);
                            break;

                        case QueryFrom.Settings:
                            currentUserContext.DataManager.SetDomain(currentUserContext.DataManager.CurrentCycle.Days.First(d => d.Id == callbackQueryParser.DomainId));

                            responseConverter = new ResponseConverter($"Выберите интересующую настройку для дня {currentUserContext.DataManager.CurrentDay.Name}");
                            buttonsSets = (ButtonsSet.SettingDay, ButtonsSet.DaysList);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {currentUserContext.Navigation.QueryFrom}");
                    }
                    break;

                case "Exercise":
                    currentUserContext.DataManager.SetDomain(currentUserContext.DataManager.CurrentDay.Exercises.First(e => e.Id == callbackQueryParser.DomainId));

                    switch (currentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:
                            currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddResultForExercise;

                            responseConverter = new ResponseConverter($"Введите вес и количество повторений для упражения {currentUserContext.DataManager.CurrentExercise.Name} " +
                                $"в формате 50 10 или 50 10;50 10;50 10... для множественного добавления");
                            buttonsSets = (ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay);
                            break;

                        case QueryFrom.Settings:
                            responseConverter = new ResponseConverter($"Выберите интересующую настройку для упражнения {currentUserContext.DataManager.CurrentExercise.Name}");
                            buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {currentUserContext.Navigation.QueryFrom}");
                    }
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ChangeActiveCommand()
        {
            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycle":
                    if (currentUserContext.DataManager.CurrentCycle.IsActive)
                    {
                        responseConverter = new ResponseConverter($"Выбранный цикл {currentUserContext.ActiveCycle.Name} уже являается активным!",
                            $"Выберите интересующую настройку для цикла {currentUserContext.DataManager.CurrentCycle.Name}");
                        buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.SettingCycles);

                        ClearHandlerAction();

                        informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return this;
                    }

                    currentUserContext.ActiveCycle.IsActive = false;
                    db.Cycles.Update(currentUserContext.ActiveCycle);
                    currentUserContext.UdpateCycleForce(currentUserContext.DataManager.CurrentCycle);
                    db.Cycles.Update(currentUserContext.ActiveCycle);

                    responseConverter = new ResponseConverter($"Активный цикл изменён на {currentUserContext.ActiveCycle.Name}",
                   $"Выберите интересующую настройку для цикла {currentUserContext.DataManager.CurrentCycle.Name}");
                    buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.SettingCycles);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ArchivingCommand()
        {
            domain = currentUserContext.GetCurrentDomainFromDataManager(callbackQueryParser.DomainType);

            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycle":
                    if (currentUserContext.DataManager.CurrentCycle.IsActive)
                    {
                        responseConverter = new ResponseConverter("Ошибка при архивации!", "Нельзя архивировать активный цикл!",
                            $"Выберите интересующую настройку для цикла {currentUserContext.DataManager.CurrentCycle.Name}");
                        buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);

                        informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return this;
                    }

                    responseConverter = new ResponseConverter($"Цикл {domain.Name} был добавлен в архив", $"Выберите интересующий цикл");
                    buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                    break;

                case "Day":
                    responseConverter = new ResponseConverter($"День {domain.Name} был добавлен в архив", $"Выберите интересующий день");
                    buttonsSets = (ButtonsSet.DaysList, ButtonsSet.SettingDays);
                    break;

                case "Exercise":
                    responseConverter = new ResponseConverter($"Упражнение {domain.Name} было добавлено в архив", $"Выберите интересующее упражнение");
                    buttonsSets = (ButtonsSet.ExercisesList, ButtonsSet.SettingExercises);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            domain.IsArchive = true;

            currentUserContext.DataManager.ResetDomain(domain);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ReplaceCommand()
        {
            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Day":
                    responseConverter = new ResponseConverter($"Выберите цикл, в который хотите перенести день {currentUserContext.DataManager.CurrentDay.Name}");
                    buttonsSets = (ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay);
                    break;
                case "Exercise":
                    responseConverter = new ResponseConverter($"Выберите день, в который хотите перенести упражнение {currentUserContext.DataManager.CurrentExercise.Name}");
                    buttonsSets = (ButtonsSet.ReplaceToDay, ButtonsSet.SettingExercise);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ReplaceToCommand()
        {
            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycle":
                    if (currentUserContext.DataManager.CurrentDay.CycleId == callbackQueryParser.DomainId)
                    {
                        responseConverter = new ResponseConverter($"Ошибка при переносе дня!", "Нельзя перенести день в тот же самый цикл",
                            $"Выберите цикл, в который хотите перенести день {currentUserContext.DataManager.CurrentDay.Name}");
                        buttonsSets = (ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay);

                        informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return this;
                    }

                    currentUserContext.DataManager.CurrentDay.CycleId = callbackQueryParser.DomainId;
                    db.Days.Update(currentUserContext.DataManager.CurrentDay);

                    responseConverter = new ResponseConverter($"День {currentUserContext.DataManager.CurrentDay.Name}, перенесён в цикл {callbackQueryParser.DomainName}",
                        "Выберите интересующий цикл");
                    break;

                case "Day":
                    if (currentUserContext.DataManager.CurrentExercise.DayId == callbackQueryParser.DomainId)
                    {
                        responseConverter = new ResponseConverter($"Ошибка при переносе упражнения!", "Нельзя перенести упражнение в тот же самый день",
                            $"Выберите день, в который хотите перенести упражнение {currentUserContext.DataManager.CurrentExercise.Name}");
                        buttonsSets = (ButtonsSet.ReplaceToDay, ButtonsSet.SettingExercise);

                        informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return this;
                    }

                    currentUserContext.DataManager.CurrentExercise.DayId = callbackQueryParser.DomainId;
                    db.Exercises.Update(currentUserContext.DataManager.CurrentExercise);

                    responseConverter = new ResponseConverter($"Упражнение {currentUserContext.DataManager.CurrentExercise.Name}, перенесёно в день {callbackQueryParser.DomainName}",
                        "Выберите интересующий цикл");
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ChangeNameCommand()
        {
            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycle":
                    currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.ChangeNameCycle;

                    responseConverter = new ResponseConverter($"Введите новоё название для цикла {currentUserContext.DataManager.CurrentCycle.Name}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingCycle);
                    break;

                case "ChangeNameDay":
                    currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.ChangeNameDay;

                    responseConverter = new ResponseConverter($"Введите новоё название для дня {currentUserContext.DataManager.CurrentDay.Name}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingDay);
                    break;

                case "Exercise":
                    currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.ChangeNameExercise;

                    responseConverter = new ResponseConverter($"Введите новоё название для упражнения {currentUserContext.DataManager.CurrentExercise.Name}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercise);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH Period()
        {
            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            IQueryable<ResultExercise> resultsExercises = callbackQueryParser.AdditionalParameters.Any(a => a.Contains("Export"))
                                                            ? db.ResultsExercises
                                                                            .Where(re => currentUserContext.GetUserExercisesIds().Contains(re.ExerciseId))
                                                            : default;
         
            string operation = callbackQueryParser.AdditionalParameters.First();
            int monthFilterPeriod = int.Parse(callbackQueryParser.AdditionalParameters.Skip(1).First());

            switch (operation)
            {
                case "Export/Excel":
                    byte[] excelFile = ExcelExportHelper.GetExcelFile(currentUserContext.UserInformation.Cycles, resultsExercises, monthFilterPeriod);

                    informationSet = new FileInformationSet(new MemoryStream(excelFile), "Workout.xlsx", $"Тренировки успешно экспортированы!", (ButtonsSet.Main, ButtonsSet.None));
                    break;
                case "Export/JSON":
                    string json = JsonExportHelper.GetJSONFile(currentUserContext.UserInformation.Cycles, resultsExercises, monthFilterPeriod);
                    byte[] byteJson = new UTF8Encoding(true).GetBytes(json);

                    informationSet = new FileInformationSet(new MemoryStream(byteJson), "Workout.json", $"Тренировки успешно экспортированы!", (ButtonsSet.Main, ButtonsSet.None));
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный {nameof(operation)}: {operation}");
            }

            return this;
        }

        internal SettingsCH DeleteCommand()
        {
            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Account":
                    responseConverter = new ResponseConverter("Вы уверены?", "Удаление аккаунта приведёт к полной и безвозвратной потере информации о ваших тренировках");
                    buttonsSets = (ButtonsSet.ConfirmDeleteAccount, ButtonsSet.Settings);
                    break;

                case "Cycle":
                    responseConverter = new ResponseConverter("Вы уверены?", "Удаление цикла приведёт к полной и безвозвратной потере информации о ваших тренировках в этом цикле");
                    buttonsSets = (ButtonsSet.ConfirmDeleteCycle, ButtonsSet.SettingCycle);
                    break;

                case "Day":
                    responseConverter = new ResponseConverter("Вы уверены?", "Удаление дня приведёт к полной и безвозвратной потере информации о ваших тренировках в этом дне");
                    buttonsSets = (ButtonsSet.ConfirmDeleteDay, ButtonsSet.SettingDay);
                    break;

                case "Exercise":
                    responseConverter = new ResponseConverter("Вы уверены?", "Удаление упражнения приведёт к полной и безвозвратной потере информации о ваших тренировках с этим упражнением");
                    buttonsSets = (ButtonsSet.ConfirmDeleteExercise, ButtonsSet.SettingExercise);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ConfirmDeleteCommand()
        {
            domain = currentUserContext.GetCurrentDomainFromDataManager(callbackQueryParser.DomainType);

            ResponseConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycle":
                    if (currentUserContext.DataManager.CurrentCycle.IsActive)
                    {
                        responseConverter = new ResponseConverter("Ошибка при удалении!", "Нельзя удалить активный цикл!",
                            $"Выберите интересующую настройку для цикла {currentUserContext.DataManager.CurrentCycle.Name}");
                        buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);

                        informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return this;
                    }

                    responseConverter = new ResponseConverter($"Цикл {domain.Name} удалён!", "Выберите интересующий цикл");
                    buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                    break;

                case "Day":
                    responseConverter = new ResponseConverter($"День {domain.Name} удалён!", "Выберите интересующий день");
                    buttonsSets = (ButtonsSet.DaysList, ButtonsSet.SettingDays);
                    break;

                case "Exercise":
                    responseConverter = new ResponseConverter($"Упражнение {domain.Name} удалёно!", "Выберите интересующее упражнение");
                    buttonsSets = (ButtonsSet.ExercisesList, ButtonsSet.SettingExercises);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            currentUserContext.DataManager.ResetDomain(domain);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }
    }
}