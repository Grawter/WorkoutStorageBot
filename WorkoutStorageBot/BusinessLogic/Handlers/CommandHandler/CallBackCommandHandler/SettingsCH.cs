#region using
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.SessionContext;
using WorkoutStorageBot.Helpers;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Helpers.InformationSetForSend;
using WorkoutStorageBot.Model;
#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandler.CallBackCommandHandler
{
    internal class SettingsCH : CallBackCH
    {
        internal SettingsCH(EntityContext db, UserContext userContext, CallbackQueryParser callbackQueryParser) : base(db, userContext, callbackQueryParser)
        { }

        internal override SettingsCH Expectation(params HandlerAction[] handlerActions)
        {
            this.handlerActions = handlerActions;

            return this;
        }

        internal override MessageInformationSet GetData()
        {
            foreach (var handlerAction in handlerActions)
            {
                switch (handlerAction)
                {
                    case HandlerAction.None:
                        break;
                    case HandlerAction.Update:
                        db.Update(domain);
                        break;
                    case HandlerAction.Add:
                        db.Add(domain);
                        break;
                    case HandlerAction.Remove:
                        db.Remove(domain);
                        break;
                    case HandlerAction.Save:
                        db.SaveChanges();
                        break;
                    default:
                        throw new NotImplementedException($"Неожиданный handlerAction: {handlerAction}");
                }

            }

            return new MessageInformationSet(responseConverter.Convert(), buttonsSets);
        }

        public SettingsCH SettingsCommand()
        {
            currentUserContext.Navigation.QueryFrom = QueryFrom.Settings;

            responseConverter = new ResponseConverter("Выберите интересующие настройки");
            buttonsSets = (ButtonsSet.Settings, ButtonsSet.Main);

            return this;
        }

        internal SettingsCH ArchiveStoreCommand()
        {
            responseConverter = new ResponseConverter("Выберите интересующий архив для разархивирования");
            buttonsSets = (ButtonsSet.ArchiveList, ButtonsSet.Settings);

            return this;
        }

        internal SettingsCH ArchiveCommand()
        {
            switch (callbackQueryParser.ObjectType)
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
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
            }

            return this;
        }

        internal SettingsCH UnArchiveCommand()
        {
            domainProvider = new DomainProvider(db, currentUserContext);
            domain = domainProvider.GetDomainWithId(callbackQueryParser.ObjectId, callbackQueryParser.ObjectType);
            domain.IsArchive = false;

            responseConverter = new ResponseConverter($"{domain.Name} разархивирован!");
            buttonsSets = (ButtonsSet.ArchiveList, ButtonsSet.Settings);

            return this;
        }

        internal SettingsCH SettingCommand()
        {
            switch (callbackQueryParser.ObjectType)
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
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
            }

            return this;
        }

        internal SettingsCH AddCommand()
        {
            switch (callbackQueryParser.ObjectType)
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

            return this;
        }

        internal SettingsCH SaveExercisesCommand()
        {
            db.Exercises.AddRange(currentUserContext.DataManager.Exercises);

            currentUserContext.DataManager.ResetExercises();

            currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

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

            return this;
        }

        internal SettingsCH SettingExistingCommand()
        {
            switch (callbackQueryParser.ObjectType)
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
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
            }

            return this;
        }

        internal SettingsCH SelectedCommand()
        {
            switch (callbackQueryParser.ObjectType)
            {
                case "Cycle":
                    currentUserContext.DataManager.SetDomain(currentUserContext.UserInformation.Cycles.First(c => c.Id == callbackQueryParser.ObjectId));

                    responseConverter = new ResponseConverter($"Выберите интересующую настройку для цикла {currentUserContext.DataManager.CurrentCycle.Name}");
                    buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);
                    break;

                case "Day":
                    switch (currentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:
                            currentUserContext.DataManager.SetDomain(currentUserContext.ActiveCycle.Days.First(d => d.Id == callbackQueryParser.ObjectId));

                            responseConverter = new ResponseConverter("Выберите упраженение");
                            buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);
                            break;

                        case QueryFrom.Settings:
                            currentUserContext.DataManager.SetDomain(currentUserContext.DataManager.CurrentCycle.Days.First(d => d.Id == callbackQueryParser.ObjectId));

                            responseConverter = new ResponseConverter($"Выберите интересующую настройку для дня {currentUserContext.DataManager.CurrentDay.Name}");
                            buttonsSets = (ButtonsSet.SettingDay, ButtonsSet.DaysList);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {currentUserContext.Navigation.QueryFrom}");
                    }
                    break;

                case "Exercise":
                    currentUserContext.DataManager.SetDomain(currentUserContext.DataManager.CurrentDay.Exercises.First(e => e.Id == callbackQueryParser.ObjectId));

                    switch (currentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:
                            currentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddResultForExercise;

                            responseConverter = new ResponseConverter("Введите вес и количество повторений в формате 50 10 или 50 10;50 10;50 10... для множественного добавления");
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
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
            }

            return this;
        }

        internal SettingsCH ChangeActiveCommand()
        {
            switch (callbackQueryParser.ObjectType)
            {
                case "Cycle":
                    if (currentUserContext.DataManager.CurrentCycle.IsActive)
                    {
                        responseConverter = new ResponseConverter($"Выбранный цикл {currentUserContext.ActiveCycle.Name} уже являается активным!",
                            $"Выберите интересующую настройку для цикла {currentUserContext.DataManager.CurrentCycle.Name}");
                        buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.SettingCycles);

                        ClearHandlerAction();

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
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
            }

            return this;
        }

        internal SettingsCH ArchivingCommand()
        {
            domainProvider = new DomainProvider(db, currentUserContext);
            domain = domainProvider.GetDomainFromDataManager(callbackQueryParser.ObjectType);

            switch (callbackQueryParser.ObjectType)
            {
                case "Cycle":
                    if (currentUserContext.DataManager.CurrentCycle.IsActive)
                    {
                        responseConverter = new ResponseConverter("Ошибка при архивации!", "Нельзя архивировать активный цикл!",
                            $"Выберите интересующую настройку для цикла {currentUserContext.DataManager.CurrentCycle.Name}");
                        buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);

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
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
            }

            domain.IsArchive = true;

            currentUserContext.DataManager.ResetDomain(domain);

            return this;
        }

        internal SettingsCH ReplaceCommand()
        {
            switch (callbackQueryParser.ObjectType)
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
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
            }

            return this;
        }

        internal SettingsCH ReplaceToCommand()
        {
            switch (callbackQueryParser.ObjectType)
            {
                case "Cycle":
                    if (currentUserContext.DataManager.CurrentDay.CycleId == callbackQueryParser.ObjectId)
                    {
                        responseConverter = new ResponseConverter($"Ошибка при переносе дня!", "Нельзя перенести день в тот же самый цикл",
                            $"Выберите цикл, в который хотите перенести день {currentUserContext.DataManager.CurrentDay.Name}");
                        buttonsSets = (ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay);

                        return this;
                    }

                    currentUserContext.DataManager.CurrentDay.CycleId = callbackQueryParser.ObjectId;
                    db.Days.Update(currentUserContext.DataManager.CurrentDay);

                    responseConverter = new ResponseConverter($"День {currentUserContext.DataManager.CurrentDay.Name}, перенесён в цикл {callbackQueryParser.ObjectName}",
                        "Выберите интересующий цикл");
                    break;

                case "Day":
                    if (currentUserContext.DataManager.CurrentExercise.DayId == callbackQueryParser.ObjectId)
                    {
                        responseConverter = new ResponseConverter($"Ошибка при переносе упражнения!", "Нельзя перенести упражнение в тот же самый день",
                            $"Выберите день, в который хотите перенести упражнение {currentUserContext.DataManager.CurrentExercise.Name}");
                        buttonsSets = (ButtonsSet.ReplaceToDay, ButtonsSet.SettingExercise);

                        return this;
                    }

                    currentUserContext.DataManager.CurrentExercise.DayId = callbackQueryParser.ObjectId;
                    db.Exercises.Update(currentUserContext.DataManager.CurrentExercise);

                    responseConverter = new ResponseConverter($"Упражнение {currentUserContext.DataManager.CurrentExercise.Name}, перенесёно в день {callbackQueryParser.ObjectName}",
                        "Выберите интересующий цикл");
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
            }

            buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);

            return this;
        }

        internal SettingsCH ChangeNameCommand()
        {
            switch (callbackQueryParser.ObjectType)
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
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
            }

            return this;
        }

        internal SettingsCH DeleteCommand()
        {
            switch (callbackQueryParser.ObjectType)
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
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
            }

            return this;
        }

        internal SettingsCH ConfirmDeleteCommand()
        {
            domainProvider = new DomainProvider(db, currentUserContext);
            domain = domainProvider.GetDomainFromDataManager(callbackQueryParser.ObjectType);

            switch (callbackQueryParser.ObjectType)
            {
                case "Cycle":
                    if (currentUserContext.DataManager.CurrentCycle.IsActive)
                    {
                        responseConverter = new ResponseConverter("Ошибка при удалении!", "Нельзя удалить активный цикл!",
                            $"Выберите интересующую настройку для цикла {currentUserContext.DataManager.CurrentCycle.Name}");
                        buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);

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
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.ObjectType}");
            }

            currentUserContext.DataManager.ResetDomain(domain);

            return this;
        }
    }
}