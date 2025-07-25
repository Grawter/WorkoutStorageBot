﻿#region using

using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.CoreRepositories.Repositories;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Handlers.MainHandlers.Handlers;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.Extenions;
using WorkoutStorageBot.Helpers.CallbackQueryParser;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Helpers.Export;
using WorkoutStorageBot.Model.Domain;
using WorkoutStorageBot.Model.HandlerData;

#endregion

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context
{
    internal class SettingsCH : CallBackCH
    {
        internal SettingsCH(CommandHandlerData commandHandlerTools, CallbackQueryParser callbackQueryParser) : base(commandHandlerTools, callbackQueryParser)
        { }

        internal override SettingsCH Expectation(params HandlerAction[] handlerActions)
        {
            this.HandlerActions = handlerActions;

            return this;
        }

        internal SettingsCH SettingsCommand()
        {
            this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom = QueryFrom.Settings;

            ResponseTextConverter responseConverter = new ResponseTextConverter("Выберите интересующие настройки");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Settings, ButtonsSet.Main);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ArchiveStoreCommand()
        {
            ResponseTextConverter responseConverter = new ResponseTextConverter("Выберите интересующий архив для разархивирования");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ArchiveList, ButtonsSet.Settings);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ArchiveCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycles":
                    responseConverter = new ResponseTextConverter("Выберите архивный цикл для разархивирования");
                    buttonsSets = (ButtonsSet.ArchiveCyclesList, ButtonsSet.ArchiveList);
                    break;
                case "Days":
                    responseConverter = new ResponseTextConverter("Выберите архивный день для разархивирования");
                    buttonsSets = (ButtonsSet.ArchiveDaysList, ButtonsSet.ArchiveList);
                    break;
                case "Exercises":
                    responseConverter = new ResponseTextConverter("Выберите архивное упражнение для разархивирования");
                    buttonsSets = (ButtonsSet.ArchiveExercisesList, ButtonsSet.ArchiveList);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH UnArchiveCommand()
        {
            string domainIDStr = callbackQueryParser.GetRequiredAdditionalParameter(0);
            int domainID = int.Parse(domainIDStr);

            this.Domain = this.CommandHandlerTools.Db.GetDomainWithId(domainID, callbackQueryParser.DomainType);
            this.Domain.IsArchive = false;

            ResponseTextConverter responseConverter = new ResponseTextConverter($"{this.Domain.Name.AddBoldAndQuotes()} разархивирован!");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ArchiveList, ButtonsSet.Settings);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH AboutBotCommand()
        {
            string aboutBotText = this.CommandHandlerTools.ParentHandler.CoreTools.ConfigurationData.AboutBotText;

            if (string.IsNullOrWhiteSpace(aboutBotText))
                aboutBotText = "Информации о боте не указано";

            ResponseTextConverter responseConverter = new ResponseTextConverter(aboutBotText);
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.None, ButtonsSet.Settings);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ExportCommand()
        {
            ResponseTextConverter responseConverter = new ResponseTextConverter("Выберите формат в котором экспортировать данные о ваших тренировках");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Export, ButtonsSet.Settings);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ExportToCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;
            Dictionary<string, string> additionalParameters;

            string exportFormat = callbackQueryParser.AdditionalParameters.First();

            switch (exportFormat)
            {
                case "Excel":
                    responseConverter = new ResponseTextConverter("Выберите временной промежуток формирования данных от последней тренировки");
                    buttonsSets = (ButtonsSet.Period, ButtonsSet.Settings);
                    additionalParameters = new() { { "Act", "Export/Excel" } };
                    break;
                case "JSON":
                    responseConverter = new ResponseTextConverter("Выберите временной промежуток формирования данных от последней тренировки");
                    buttonsSets = (ButtonsSet.Period, ButtonsSet.Settings);
                    additionalParameters = new() { { "Act", "Export/JSON" } };
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets, additionalParameters);

            return this;
        }

        internal SettingsCH SettingCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycles":
                    responseConverter = new ResponseTextConverter("Выберите интересующие настройки для циклов");
                    buttonsSets = (ButtonsSet.SettingCycles, ButtonsSet.Settings);
                    break;
                case "Days":
                    responseConverter = new ResponseTextConverter("Выберите интересующие настройки для дней");
                    buttonsSets = (ButtonsSet.SettingDays, ButtonsSet.SettingCycle);
                    break;
                case "Exercises":
                    responseConverter = new ResponseTextConverter("Выберите интересующие настройки для упражнений");
                    buttonsSets = (ButtonsSet.SettingExercises, ButtonsSet.SettingDay);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH AddCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycle":
                    this.CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddCycle;

                    responseConverter = new ResponseTextConverter("Введите название тренировочного цикла");

                    switch (this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                            break;

                        case QueryFrom.Settings:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.SettingCycles);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
                    }
                    break;

                case "Day":
                    this.CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddDays;

                    responseConverter = new ResponseTextConverter($"Введите название тренирочного дня для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");

                    switch (this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                            break;

                        case QueryFrom.Settings:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.SettingDays);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
                    }
                    break;

                case "Exercise":
                    this.CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddExercises;

                    responseConverter = new ResponseTextConverter($"Введите название(я) и тип(ы) упражнения(й) для дня {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()}",
                        CommonConsts.Exercise.ExamplesTypesExercise,
                        CommonConsts.Exercise.InputFormatExercise);

                    switch (this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                            break;

                        case QueryFrom.Settings:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercises);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
                    }
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH SaveExercisesCommand()
        {
            this.CommandHandlerTools.Db.Exercises.AddRange(this.CommandHandlerTools.CurrentUserContext.DataManager.Exercises);

            this.CommandHandlerTools.CurrentUserContext.DataManager.ResetExercises();

            this.CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.Default;

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
            {
                case QueryFrom.Start:
                    responseConverter = new ResponseTextConverter("Упражнения сохранены!", "Выберите дальнейшее действие");
                    buttonsSets = (ButtonsSet.RedirectAfterSaveExercise, ButtonsSet.None);
                    break;

                case QueryFrom.Settings:
                    responseConverter = new ResponseTextConverter("Упражнения сохранены!", "Выберите интересующие настройки для упражнений");
                    buttonsSets = (ButtonsSet.SettingExercises, ButtonsSet.SettingDays);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH SettingExistingCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycles":
                    responseConverter = new ResponseTextConverter("Выберите интересующий цикл");
                    buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                    break;
                case "Days":
                    responseConverter = new ResponseTextConverter("Выберите интересующий день");
                    buttonsSets = (ButtonsSet.DaysList, ButtonsSet.SettingDays);
                    break;
                case "Exercises":
                    responseConverter = new ResponseTextConverter("Выберите интересующее упражнение");
                    buttonsSets = (ButtonsSet.ExercisesList, ButtonsSet.SettingExercises);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH SelectedCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            string domainIDStr = callbackQueryParser.GetRequiredAdditionalParameter(0);
            int domainID = int.Parse(domainIDStr);

            switch (callbackQueryParser.DomainType)
            {
                case "Cycle":
                    this.CommandHandlerTools.CurrentUserContext.DataManager.SetDomain(this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles
                                                                                                                                .First(c => c.Id == domainID));

                    responseConverter = new ResponseTextConverter($"Выберите интересующую настройку для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);
                    break;

                case "Day":
                    switch (this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:
                            this.CommandHandlerTools.CurrentUserContext.DataManager.SetDomain(this.CommandHandlerTools.CurrentUserContext.ActiveCycle.Days
                                                                                                                                .First(d => d.Id == domainID));

                            responseConverter = new ResponseTextConverter("Выберите упраженение");
                            buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);
                            break;

                        case QueryFrom.Settings:
                            this.CommandHandlerTools.CurrentUserContext.DataManager.SetDomain(this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Days
                                                                                                                                .First(d => d.Id == domainID));

                            responseConverter = new ResponseTextConverter($"Выберите интересующую настройку для дня {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()}");
                            buttonsSets = (ButtonsSet.SettingDay, ButtonsSet.DaysList);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
                    }
                    break;

                case "Exercise":
                    Exercise currentExercise = this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Exercises.First(e => e.Id == domainID);

                    this.CommandHandlerTools.CurrentUserContext.DataManager.SetDomain(currentExercise);

                    string currentExerciseName = currentExercise.Name.AddBoldAndQuotes();

                    string inputFormatExerciseResult = currentExercise.Mode switch
                    {
                        ExercisesMods.Count => CommonConsts.ResultExercise.InputFormatExerciseResultCount,
                        ExercisesMods.WeightCount => CommonConsts.ResultExercise.InputFormatExerciseResultWeightCount,
                        ExercisesMods.FreeResult => CommonConsts.ResultExercise.InputFormatExerciseResultFreeResult,
                        _ => throw new NotImplementedException($"Неожиданный тип упражнения: {currentExercise.Mode.ToString()}")
                    };

                    switch (this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:
                            this.CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.AddResultForExercise;

                            responseConverter = new ResponseTextConverter($"Фиксирование результатов упражнения {currentExerciseName.AddBoldAndQuotes()}",
                                inputFormatExerciseResult,
                                $"Введите результат(ы) подхода(ов)");
                            buttonsSets = (ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay);
                            break;

                        case QueryFrom.Settings:
                            responseConverter = new ResponseTextConverter($"Выберите интересующую настройку для упражнения {currentExerciseName.AddBoldAndQuotes()}");
                            buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
                    }
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ChangeActiveCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycle":
                    if (this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.IsActive)
                    {
                        responseConverter = new ResponseTextConverter($"Выбранный цикл {this.CommandHandlerTools.CurrentUserContext.ActiveCycle.Name.AddBoldAndQuotes()} уже является активным!",
                            $"Выберите интересующую настройку для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                        buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.SettingCycles);

                        ClearHandlerAction();

                        this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return this;
                    }

                    this.CommandHandlerTools.CurrentUserContext.ActiveCycle.IsActive = false;
                    this.CommandHandlerTools.Db.Cycles.Update(this.CommandHandlerTools.CurrentUserContext.ActiveCycle);
                    this.CommandHandlerTools.CurrentUserContext.UdpateActiveCycleForce(this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle);
                    this.CommandHandlerTools.Db.Cycles.Update(this.CommandHandlerTools.CurrentUserContext.ActiveCycle);

                    responseConverter = new ResponseTextConverter($"Активный цикл изменён на {this.CommandHandlerTools.CurrentUserContext.ActiveCycle.Name.AddBoldAndQuotes()}",
                    $"Выберите интересующую настройку для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.SettingCycles);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ArchivingCommand()
        {
            this.Domain = this.CommandHandlerTools.CurrentUserContext.DataManager.GetCurrentDomain(callbackQueryParser.DomainType);

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycle":
                    if (this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.IsActive)
                    {
                        responseConverter = new ResponseTextConverter("Ошибка при архивации!", "Нельзя архивировать активный цикл!",
                            $"Выберите интересующую настройку для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                        buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);

                        this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return this;
                    }

                    responseConverter = new ResponseTextConverter($"Цикл {this.Domain.Name.AddBoldAndQuotes()} был добавлен в архив", $"Выберите интересующий цикл");
                    buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                    break;

                case "Day":
                    responseConverter = new ResponseTextConverter($"День {this.Domain.Name.AddBoldAndQuotes()} был добавлен в архив", $"Выберите интересующий день");
                    buttonsSets = (ButtonsSet.DaysList, ButtonsSet.SettingDays);
                    break;

                case "Exercise":
                    responseConverter = new ResponseTextConverter($"Упражнение {this.Domain.Name.AddBoldAndQuotes()} было добавлено в архив", $"Выберите интересующее упражнение");
                    buttonsSets = (ButtonsSet.ExercisesList, ButtonsSet.SettingExercises);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            this.Domain.IsArchive = true;

            this.CommandHandlerTools.CurrentUserContext.DataManager.ResetDomain(this.Domain);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ReplaceCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Day":
                    responseConverter = new ResponseTextConverter($"Выберите цикл, в который хотите перенести день {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay);
                    break;
                case "Exercise":
                    responseConverter = new ResponseTextConverter($"Выберите день, в который хотите перенести упражнение {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.ReplaceToDay, ButtonsSet.SettingExercise);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ReplaceToCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            string targetDomainIDStr = callbackQueryParser.GetRequiredAdditionalParameter(0);
            int targetDomainID = int.Parse(targetDomainIDStr);

            IDomain targetDomain;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycle":
                    if (this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.CycleId == targetDomainID)
                    {
                        responseConverter = new ResponseTextConverter($"Ошибка при переносе дня!", "Нельзя перенести день в тот же самый цикл",
                            $"Выберите цикл, в который хотите перенести день {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()}");
                        buttonsSets = (ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay);

                        this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return this;
                    }

                    targetDomain = this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles.FirstOrDefault(x => x.Id == targetDomainID)
                        ?? throw new InvalidOperationException($"Не удалось найти targetDomain c ID = '{targetDomainID}' среди циклов у пользователя '{this.CommandHandlerTools.CurrentUserContext.UserInformation.UserId}'");

                    this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.CycleId = targetDomain.Id;
                    this.CommandHandlerTools.Db.Days.Update(this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay);

                    responseConverter = new ResponseTextConverter($"День {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()}, перенесён в цикл {targetDomain.Name.AddBoldAndQuotes()}",
                        "Выберите интересующий цикл");
                    break;

                case "Day":
                    if (this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.DayId == targetDomainID)
                    {
                        responseConverter = new ResponseTextConverter($"Ошибка при переносе упражнения!", "Нельзя перенести упражнение в тот же самый день",
                            $"Выберите день, в который хотите перенести упражнение {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.Name.AddBoldAndQuotes()}");
                        buttonsSets = (ButtonsSet.ReplaceToDay, ButtonsSet.SettingExercise);

                        this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return this;
                    }

                    targetDomain = this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle?.Days?.FirstOrDefault(x => x.Id == targetDomainID)
                        ?? throw new InvalidOperationException($"Не удалось найти targetDomain c ID = '{targetDomainID}' среди дней цикла с ID = '{this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Id}' у пользователя '{this.CommandHandlerTools.CurrentUserContext.UserInformation.UserId}'");

                    this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.DayId = targetDomain.Id;
                    this.CommandHandlerTools.Db.Exercises.Update(this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise);

                    responseConverter = new ResponseTextConverter($"Упражнение {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.Name.AddBoldAndQuotes()}, перенесёно в день {targetDomain.Name.AddBoldAndQuotes()}",
                        "Выберите интересующий цикл");
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ChangeNameCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycle":
                    this.CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.ChangeNameCycle;

                    responseConverter = new ResponseTextConverter($"Введите новоё название для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingCycle);
                    break;

                case "Day":
                    this.CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.ChangeNameDay;

                    responseConverter = new ResponseTextConverter($"Введите новоё название для дня {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingDay);
                    break;

                case "Exercise":
                    this.CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.ChangeNameExercise;

                    responseConverter = new ResponseTextConverter($"Введите новоё название для упражнения {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercise);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ChangeModeCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Exercise":

                    responseConverter = new ResponseTextConverter($"Выберите новый тип для упражнения {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.ChangeType, ButtonsSet.SettingExercise);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH ChangedModeCommand()
        {
            this.Domain = this.CommandHandlerTools.CurrentUserContext.DataManager.GetCurrentDomain(callbackQueryParser.DomainType);

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Exercise":

                    string modeStr = this.callbackQueryParser.AdditionalParameters.FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(modeStr))
                        throw new InvalidOperationException($"Не удалось получить новый режим упражнения");

                    ExercisesMods newMode = (ExercisesMods)int.Parse(modeStr);

                    ((Exercise)this.Domain).Mode = newMode;

                    responseConverter = new ResponseTextConverter($"Режим для упражнения {Domain.Name.AddBoldAndQuotes()} изменён на {newMode.ToString().AddBoldAndQuotes()}",
                        $"Выберите интересующую настройку для упражнения {Domain.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        internal SettingsCH Period()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            string operation = callbackQueryParser.AdditionalParameters.First();
            int monthFilterPeriod = int.Parse(callbackQueryParser.AdditionalParameters.Skip(1).First());

            switch (operation)
            {
                case "Export/Excel":

                    this.InformationSet = TryGetExportFile(".xlsx", monthFilterPeriod);
                       
                    break;
                case "Export/JSON":

                    this.InformationSet = TryGetExportFile(".json", monthFilterPeriod);

                    break;
                default:
                    throw new NotImplementedException($"Неожиданный {nameof(operation)}: {operation}");
            }

            return this;
        }

        internal SettingsCH DeleteCommand()
        {
            Dictionary<string, string> additionalParameters = new Dictionary<string, string>() { { "DomainType", callbackQueryParser.DomainType } };
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Account":
                    responseConverter = new ResponseTextConverter("Вы уверены?", $"{"Удаление аккаунта приведёт к полной и безвозвратной потере информации о ваших тренировках".AddBold()}");
                    buttonsSets = (ButtonsSet.ConfirmDelete, ButtonsSet.Settings);

                    additionalParameters.Add("Name", "аккаунт");
                    break;

                case "Cycle":
                    responseConverter = new ResponseTextConverter("Вы уверены?", $"{"Удаление цикла приведёт к полной и безвозвратной потере информации о ваших тренировках в этом цикле".AddBold()}");
                    buttonsSets = (ButtonsSet.ConfirmDelete, ButtonsSet.SettingCycle);

                    additionalParameters.Add("Name", "цикл");
                    break;

                case "Day":
                    responseConverter = new ResponseTextConverter("Вы уверены?", $"{"Удаление дня приведёт к полной и безвозвратной потере информации о ваших тренировках в этом дне".AddBold()}");
                    buttonsSets = (ButtonsSet.ConfirmDelete, ButtonsSet.SettingDay);

                    additionalParameters.Add("Name", "день");
                    break;

                case "Exercise":
                    responseConverter = new ResponseTextConverter("Вы уверены?", $"{"Удаление упражнения приведёт к полной и безвозвратной потере информации о ваших тренировках с этим упражнением".AddBold()}");
                    buttonsSets = (ButtonsSet.ConfirmDelete, ButtonsSet.SettingExercise);

                    additionalParameters.Add("Name", "упражнение");
                    break;

                case "ResultsExercises":
                    responseConverter = new ResponseTextConverter("Вы уверены?", $"{"Удаление подходов приведёт к полной и безвозвратной потере информации".AddBold()}",
                        "Для удаления введите кол-во последних записей, которые требуется удалить");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercise);

                    this.CommandHandlerTools.CurrentUserContext.Navigation.MessageNavigationTarget = MessageNavigationTarget.DeleteResultsExercises;

                    additionalParameters.Add("Name", "упражнение");
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets, additionalParameters);

            return this;
        }

        internal SettingsCH ConfirmDeleteCommand()
        {
            this.Domain = this.CommandHandlerTools.CurrentUserContext.DataManager.GetCurrentDomain(callbackQueryParser.DomainType);

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case "Cycle":
                    if (this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.IsActive)
                    {
                        this.ClearHandlerAction();

                        responseConverter = new ResponseTextConverter("Ошибка при удалении!", "Нельзя удалить активный цикл!",
                            $"Выберите интересующую настройку для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                        buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);

                        this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return this;
                    }

                    responseConverter = new ResponseTextConverter($"Цикл {this.Domain.Name.AddBoldAndQuotes()} удалён!", "Выберите интересующий цикл");
                    buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                    break;

                case "Day":
                    responseConverter = new ResponseTextConverter($"День {this.Domain.Name.AddBoldAndQuotes()} удалён!", "Выберите интересующий день");
                    buttonsSets = (ButtonsSet.DaysList, ButtonsSet.SettingDays);
                    break;

                case "Exercise":
                    responseConverter = new ResponseTextConverter($"Упражнение {this.Domain.Name.AddBoldAndQuotes()} удалёно!", "Выберите интересующее упражнение");
                    buttonsSets = (ButtonsSet.ExercisesList, ButtonsSet.SettingExercises);
                    break;
                case "Account":

                    PrimaryUpdateHandler handler = CommandHandlerTools.ParentHandler.CoreManager.GetHandler<PrimaryUpdateHandler>();
                    handler.DeleteContext(this.CommandHandlerTools.CurrentUserContext.UserInformation.UserId);

                    AdminRepository repository = CommandHandlerTools.ParentHandler.CoreManager.GetRepository<AdminRepository>();
                    repository.DeleteAccount(this.CommandHandlerTools.CurrentUserContext.UserInformation);

                    buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                    responseConverter = new ResponseTextConverter("Аккаунт успешно удалён");
                    this.ClearHandlerAction();
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            this.CommandHandlerTools.CurrentUserContext.DataManager.ResetDomain(this.Domain);

            this.InformationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return this;
        }

        private IInformationSet TryGetExportFile(string fileExtenstion, int monthFilterPeriod)
        {
            if (string.IsNullOrWhiteSpace(fileExtenstion) || !fileExtenstion.StartsWith("."))
                throw new InvalidOperationException($"Получено некорректное расширение файла");

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Settings, ButtonsSet.Main);

            IInformationSet result;

            if (this.CommandHandlerTools.CurrentUserContext.LimitsManager.HasBlockByTimeLimit("Export", out DateTime limit))
            {
                responseConverter =
                    new ResponseTextConverter($"Следующее выполнение этой операции будет доступно {limit.ToString(CommonConsts.Common.DateTimeFormatHoursFirst)})",
                                                "Выберите интересующую настройку");

                result = new MessageInformationSet(responseConverter.Convert(), buttonsSets);
            }
            else
            {
                IQueryable<ResultExercise> resultsExercisesForExcel = GetResultExercises();

                if (resultsExercisesForExcel.Count() < 1)
                {
                    responseConverter = new ResponseTextConverter("Отсутствуют результаты для экспорта", 
                        "Выберите интересующую настройку");

                    result = new MessageInformationSet(responseConverter.Convert(), buttonsSets);
                    return result;
                }

                this.CommandHandlerTools.CurrentUserContext.LimitsManager.AddOrUpdateTimeLimit("Export", DateTime.Now.AddHours(6));

                byte[] file = new byte[0];

                switch (fileExtenstion)
                {
                    case ".xlsx":
                        file = ExcelExportHelper.GetExcelFile(this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles, resultsExercisesForExcel, monthFilterPeriod);
                    break;
                    case ".json":
                        file = JsonExportHelper.GetJSONFileByte(this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles, resultsExercisesForExcel, monthFilterPeriod);
                    break;
                    default:
                        throw new NotSupportedException("Неподдерживаемый формат экспорта");
                }

                string fileName = $"Workout{fileExtenstion}";

                result = new FileInformationSet(new MemoryStream(file), fileName, $"Тренировки успешно экспортированы!", buttonsSets);
            }

            return result;
        }

        private IQueryable<ResultExercise> GetResultExercises()
        {
            IEnumerable<int> userExercisesIds = GetUserExercisesIds(this.CommandHandlerTools.CurrentUserContext.UserInformation);

            IQueryable<ResultExercise> resultsExercises = this.CommandHandlerTools.Db.ResultsExercises.Where(x => userExercisesIds.Contains(x.ExerciseId));
                                                            
            return resultsExercises;
        }

        private IEnumerable<int> GetUserExercisesIds(UserInformation userInformation)
        {
            foreach (Cycle cycle in userInformation.Cycles)
            {
                foreach (Day day in cycle.Days)
                {
                    foreach (Exercise exercise in day.Exercises)
                    {
                        yield return exercise.Id;
                    }
                }
            }
        }
    }
}