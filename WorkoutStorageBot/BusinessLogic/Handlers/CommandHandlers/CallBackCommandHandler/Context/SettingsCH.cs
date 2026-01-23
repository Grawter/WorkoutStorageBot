using Microsoft.IO;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extensions;
using WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.SharedCommandHandler;
using WorkoutStorageBot.BusinessLogic.Helpers.CallbackQueryParser;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.BusinessLogic.Helpers.Export;
using WorkoutStorageBot.BusinessLogic.InformationSetForSend;
using WorkoutStorageBot.BusinessLogic.Repositories;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.DTO.HandlerData;
using WorkoutStorageModels.Entities.BusinessLogic;
using WorkoutStorageBot.Model.Interfaces;
using WorkoutStorageBot.Core.Extensions;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context
{
    internal class SettingsCH : CallBackCH
    {
        internal SettingsCH(CommandHandlerData commandHandlerTools, CallbackQueryParser callbackQueryParser) : base(commandHandlerTools, callbackQueryParser)
        { }

        internal override async Task<IInformationSet> GetInformationSet()
        {
            IInformationSet informationSet;

            switch (callbackQueryParser.SubDirection)
            {
                case "Settings":
                    informationSet = SettingsCommand();
                    break;

                case "ArchiveStore":
                    informationSet = ArchiveStoreCommand();
                    break;

                case "Archive":
                    informationSet = ArchiveCommand();
                    break;
                case "Export":
                    informationSet = ExportCommand();
                    break;

                case "ExportTo":
                    informationSet = ExportToCommand();
                    break;

                case "UnArchive":
                    informationSet = await UnArchiveCommand();
                    break;

                case "AboutBot":
                    informationSet = AboutBotCommand();

                    break;

                case "Setting":
                    informationSet = SettingCommand();
                    break;

                case "Add":
                    informationSet = AddCommand();
                    break;

                case "ResetTempDomains":
                    informationSet = ResetTempDomainsCommand();
                    break;

                case "SaveExercises":
                    informationSet = await SaveExercisesCommand();
                    break;

                case "SettingExisting":
                    informationSet = SettingExistingCommand();
                    break;

                case "Selected":
                    informationSet = SelectedCommand();
                    break;

                case "ChangeActive":
                    informationSet = await ChangeActiveCommand();
                    break;

                case "Archiving":
                    informationSet = await ArchivingCommand();
                    break;

                case "Replace":
                    informationSet = ReplaceCommand();
                    break;

                case "ReplaceTo":
                    informationSet = await ReplaceToCommand();
                    break;

                case "ChangeName":
                    informationSet = ChangeNameCommand();
                    break;

                case "ChangeMode":
                    informationSet = ChangeModeCommand();
                    break;

                case "ChangedMode":
                    informationSet = await ChangedModeCommand();
                    break;

                case "Period":
                    informationSet = await Period();
                    break;

                case "Delete":
                    informationSet = DeleteCommand();
                    break;

                case "ConfirmDelete":
                    informationSet = await ConfirmDeleteCommand();
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            CheckInformationSet(informationSet);

            return informationSet;
        }

        private IInformationSet SettingsCommand()
        {
            this.CommandHandlerTools.CurrentUserContext.Navigation.SetQueryFrom(QueryFrom.Settings);

            ResponseTextConverter responseConverter = new ResponseTextConverter("Выберите интересующие настройки");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Settings, ButtonsSet.Main);

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ArchiveStoreCommand()
        {
            ResponseTextConverter responseConverter = new ResponseTextConverter("Выберите интересующий архив для разархивирования");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ArchiveList, ButtonsSet.Settings);

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ArchiveCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycles:
                    responseConverter = new ResponseTextConverter("Выберите архивный цикл для разархивирования");
                    buttonsSets = (ButtonsSet.ArchiveCyclesList, ButtonsSet.ArchiveList);
                    break;
                case CommonConsts.DomainsAndEntities.Days:
                    responseConverter = new ResponseTextConverter("Выберите архивный день для разархивирования");
                    buttonsSets = (ButtonsSet.ArchiveDaysList, ButtonsSet.ArchiveList);
                    break;
                case CommonConsts.DomainsAndEntities.Exercises:
                    responseConverter = new ResponseTextConverter("Выберите архивное упражнение для разархивирования");
                    buttonsSets = (ButtonsSet.ArchiveExercisesList, ButtonsSet.ArchiveList);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task <IInformationSet> UnArchiveCommand()
        {
            string domainIDStr = callbackQueryParser.GetRequiredAdditionalParameter(0);
            int domainID = int.Parse(domainIDStr);

            IDTODomain? DTODomain = null;

            if (callbackQueryParser.DomainType == CommonConsts.DomainsAndEntities.Cycle)
            {
                DTODomain = this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles.FirstOrDefault(x => x.Id == domainID)
                    ?? throw new InvalidOperationException($"Not found cycle for unarchiving with ID = {domainID}");
            }
            else if (callbackQueryParser.DomainType == CommonConsts.DomainsAndEntities.Day)
            {
                IEnumerable<DTODay> allDays = this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles.SelectMany(x => x.Days);

                DTODomain = allDays.FirstOrDefault(x => x.Id == domainID) ?? throw new InvalidOperationException($"Not found day for unarchiving with ID = {domainID}");
            }
            else if (callbackQueryParser.DomainType == CommonConsts.DomainsAndEntities.Exercise)
            {
                IEnumerable<DTOExercise> allExercise = this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles.SelectMany(x => x.Days).SelectMany(x => x.Exercises);

                DTODomain = allExercise.FirstOrDefault(x => x.Id == domainID) ?? throw new InvalidOperationException($"Not found exercise for unarchiving with ID = {domainID}");
            }
            else
                throw new NotImplementedException($"Неожиданный domainTyped: {callbackQueryParser.DomainType}");

            DTODomain.IsArchive = false;
            await this.CommandHandlerTools.Db.UpdateEntity(DTODomain);

            ResponseTextConverter responseConverter = new ResponseTextConverter($"{DTODomain.Name.AddBoldAndQuotes()} разархивирован!");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ArchiveList, ButtonsSet.Settings);

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet AboutBotCommand()
        {
            string? aboutBotText = this.CommandHandlerTools.ParentHandler.CoreTools.ConfigurationData.AboutBotText;

            if (string.IsNullOrWhiteSpace(aboutBotText))
                aboutBotText = "Информации о боте не указано";

            ResponseTextConverter responseConverter = new ResponseTextConverter(aboutBotText);
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.None, ButtonsSet.Settings);

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ExportCommand()
        {
            ResponseTextConverter responseConverter = new ResponseTextConverter("Выберите формат в котором экспортировать данные о ваших тренировках");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Export, ButtonsSet.Settings);

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ExportToCommand()
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

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets, additionalParameters);

            return informationSet;
        }

        private IInformationSet SettingCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycles:
                    responseConverter = new ResponseTextConverter("Выберите интересующие настройки для циклов");
                    buttonsSets = (ButtonsSet.SettingCycles, ButtonsSet.Settings);
                    break;
                case CommonConsts.DomainsAndEntities.Days:
                    responseConverter = new ResponseTextConverter("Выберите интересующие настройки для дней");
                    buttonsSets = (ButtonsSet.SettingDays, ButtonsSet.SettingCycle);
                    break;
                case CommonConsts.DomainsAndEntities.Exercises:
                    responseConverter = new ResponseTextConverter("Выберите интересующие настройки для упражнений");
                    buttonsSets = (ButtonsSet.SettingExercises, ButtonsSet.SettingDay);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet AddCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycle:
                    this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddCycle);

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

                case CommonConsts.DomainsAndEntities.Day:
                    this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddDays);

                    responseConverter = new ResponseTextConverter($"Введите название тренирочного дня для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");

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

                case CommonConsts.DomainsAndEntities.Exercise:
                    this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddExercises);

                    responseConverter = new ResponseTextConverter($"Введите название(я) и тип(ы) упражнения(й) для дня {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}",
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

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ResetTempDomainsCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Day:
                    throw new NotImplementedException($"Нереализовано для типа домена {CommonConsts.DomainsAndEntities.Day}, т.к. не нашлось ни одно небходимого кейса");

                case CommonConsts.DomainsAndEntities.Exercise:
                    this.CommandHandlerTools.CurrentUserContext.DataManager.ResetTempExercises();

                    responseConverter = new ResponseTextConverter("Упражнения для сохранения сброшены!", "Выберите интересующую настройку");
                    buttonsSets = (ButtonsSet.SettingExercises, ButtonsSet.None);

                    break;

                default:
                    throw new InvalidOperationException($"Неожиданный {nameof(callbackQueryParser.DomainType)}: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> SaveExercisesCommand()
        {
            foreach (DTOExercise tempExercise in this.CommandHandlerTools.CurrentUserContext.DataManager.TempExercises.ThrowIfNull())
            {
                tempExercise.Day = this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay;
                this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Exercises.Add(tempExercise);
            }
            await this.CommandHandlerTools.Db.AddEntities(this.CommandHandlerTools.CurrentUserContext.DataManager.TempExercises);

            this.CommandHandlerTools.CurrentUserContext.DataManager.ResetTempExercises();

            this.CommandHandlerTools.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

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

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet SettingExistingCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycles:
                    responseConverter = new ResponseTextConverter("Выберите интересующий цикл");
                    buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                    break;
                case CommonConsts.DomainsAndEntities.Days:
                    responseConverter = new ResponseTextConverter("Выберите интересующий день");
                    buttonsSets = (ButtonsSet.DaysList, ButtonsSet.SettingDays);
                    break;
                case CommonConsts.DomainsAndEntities.Exercises:
                    responseConverter = new ResponseTextConverter("Выберите интересующее упражнение");
                    buttonsSets = (ButtonsSet.ExercisesList, ButtonsSet.SettingExercises);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet SelectedCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            string domainIDStr = callbackQueryParser.GetRequiredAdditionalParameter(0);
            int domainID = int.Parse(domainIDStr);

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycle:
                    this.CommandHandlerTools.CurrentUserContext.DataManager.SetCurrentDomain(this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles
                                                                                                                                .First(c => c.Id == domainID));

                    responseConverter = new ResponseTextConverter($"Выберите интересующую настройку для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    switch (this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:
                            this.CommandHandlerTools.CurrentUserContext.DataManager.SetCurrentDomain(this.CommandHandlerTools.CurrentUserContext.ActiveCycle.ThrowIfNull().Days
                                                                                                                                .First(d => d.Id == domainID));

                            responseConverter = new ResponseTextConverter("Выберите упраженение");
                            buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);
                            break;

                        case QueryFrom.Settings:
                            this.CommandHandlerTools.CurrentUserContext.DataManager.SetCurrentDomain(this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Days
                                                                                                                                .First(d => d.Id == domainID));

                            responseConverter = new ResponseTextConverter($"Выберите интересующую настройку для дня {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}");
                            buttonsSets = (ButtonsSet.SettingDay, ButtonsSet.DaysList);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
                    }
                    break;

                case CommonConsts.DomainsAndEntities.Exercise:
                    DTOExercise currentExercise = this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Exercises.First(e => e.Id == domainID);

                    this.CommandHandlerTools.CurrentUserContext.DataManager.SetCurrentDomain(currentExercise);

                    string currentExerciseNameBoldAndQuotes = currentExercise.Name.AddBoldAndQuotes();

                    string inputFormatExerciseResult = currentExercise.Mode switch
                    {
                        ExercisesMods.Count => CommonConsts.ResultExercise.InputFormatExerciseResultCount,
                        ExercisesMods.WeightCount => CommonConsts.ResultExercise.InputFormatExerciseResultWeightCount,
                        ExercisesMods.Timer => CommonConsts.ResultExercise.InputFormatExerciseResultTimer,
                        ExercisesMods.FreeResult => CommonConsts.ResultExercise.InputFormatExerciseResultFreeResult,
                        _ => throw new NotImplementedException($"Неожиданный тип упражнения: {currentExercise.Mode.ToString()}")
                    };

                    switch (this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:
                            if (currentExercise.Mode == ExercisesMods.Timer)
                            {
                                responseConverter = new ResponseTextConverter($"Включение таймера для упражнения {currentExerciseNameBoldAndQuotes}");
                                buttonsSets = (ButtonsSet.EnableExerciseTimer, ButtonsSet.ExercisesListWithLastWorkoutForDay);
                            }
                            else
                            {
                                this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddResultForExercise);

                                responseConverter = new ResponseTextConverter($"Фиксирование результатов упражнения {currentExerciseNameBoldAndQuotes}",
                                    inputFormatExerciseResult,
                                    $"Введите результат(ы) подхода(ов)");
                                buttonsSets = (ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay);
                            }
                                
                            break;

                        case QueryFrom.Settings:
                            responseConverter = new ResponseTextConverter($"Выберите интересующую настройку для упражнения {currentExerciseNameBoldAndQuotes}");
                            buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CommandHandlerTools.CurrentUserContext.Navigation.QueryFrom}");
                    }
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ChangeActiveCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycle:

                    this.CommandHandlerTools.CurrentUserContext.ActiveCycle.ThrowIfNull();

                    if (this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().IsActive)
                    {
                        responseConverter = new ResponseTextConverter($"Выбранный цикл {this.CommandHandlerTools.CurrentUserContext.ActiveCycle.Name.AddBoldAndQuotes()} уже является активным!",
                            $"Выберите интересующую настройку для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                        buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.SettingCycles);

                        informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return informationSet;
                    }

                    this.CommandHandlerTools.CurrentUserContext.ActiveCycle.IsActive = false;
                    await this.CommandHandlerTools.Db.UpdateEntity(this.CommandHandlerTools.CurrentUserContext.ActiveCycle, false);
                    this.CommandHandlerTools.CurrentUserContext.UdpateActiveCycleForce(this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle);
                    await this.CommandHandlerTools.Db.UpdateEntity(this.CommandHandlerTools.CurrentUserContext.ActiveCycle);

                    responseConverter = new ResponseTextConverter($"Активный цикл изменён на {this.CommandHandlerTools.CurrentUserContext.ActiveCycle.Name.AddBoldAndQuotes()}",
                    $"Выберите интересующую настройку для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.SettingCycles);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ArchivingCommand()
        {
            IDTODomain DTODomain = this.CommandHandlerTools.CurrentUserContext.DataManager.GetRequiredCurrentDomain(callbackQueryParser.DomainType);

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycle:
                    if (this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().IsActive)
                    {
                        responseConverter = new ResponseTextConverter("Ошибка при архивации!", "Нельзя архивировать активный цикл!",
                            $"Выберите интересующую настройку для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                        buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);

                        informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return informationSet;
                    }

                    responseConverter = new ResponseTextConverter($"Цикл {DTODomain.Name.AddBoldAndQuotes()} был добавлен в архив", $"Выберите интересующий цикл");
                    buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    responseConverter = new ResponseTextConverter($"День {DTODomain.Name.AddBoldAndQuotes()} был добавлен в архив", $"Выберите интересующий день");
                    buttonsSets = (ButtonsSet.DaysList, ButtonsSet.SettingDays);
                    break;

                case CommonConsts.DomainsAndEntities.Exercise:
                    responseConverter = new ResponseTextConverter($"Упражнение {DTODomain.Name.AddBoldAndQuotes()} было добавлено в архив", $"Выберите интересующее упражнение");
                    buttonsSets = (ButtonsSet.ExercisesList, ButtonsSet.SettingExercises);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            DTODomain.IsArchive = true;
            await this.CommandHandlerTools.Db.UpdateEntity(DTODomain);

            this.CommandHandlerTools.CurrentUserContext.DataManager.ResetCurrentDomain(DTODomain);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ReplaceCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Day:
                    responseConverter = new ResponseTextConverter($"Выберите цикл, в который хотите перенести день {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay);
                    break;
                case CommonConsts.DomainsAndEntities.Exercise:
                    responseConverter = new ResponseTextConverter($"Выберите день, в который хотите перенести упражнение {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.ReplaceToDay, ButtonsSet.SettingExercise);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ReplaceToCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            string targetDomainIDStr = callbackQueryParser.GetRequiredAdditionalParameter(0);
            int targetDomainID = int.Parse(targetDomainIDStr);

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycle:
                    if (this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().CycleId == targetDomainID)
                    {
                        responseConverter = new ResponseTextConverter($"Ошибка при переносе дня!", "Нельзя перенести день в тот же самый цикл",
                            $"Выберите цикл, в который хотите перенести день {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()}");
                        buttonsSets = (ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay);

                        informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return informationSet;
                    }

                    DTOCycle targetCycle = this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles.FirstOrDefault(x => x.Id == targetDomainID)
                        ?? throw new InvalidOperationException($"Не удалось найти targetDomain c ID = '{targetDomainID}' среди циклов у пользователя '{this.CommandHandlerTools.CurrentUserContext.UserInformation.UserId}'");

                    DTODay currentDay = this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay;

                    targetCycle.Days.Add(currentDay);
                    DTOCycle oldCycleByDay = currentDay.Cycle.ThrowIfNull();
                    oldCycleByDay.Days.Remove(currentDay);

                    currentDay.Cycle = targetCycle;
                    currentDay.CycleId = targetCycle.Id;

                    await this.CommandHandlerTools.Db.UpdateEntity(currentDay);

                    responseConverter = new ResponseTextConverter($"День {currentDay.Name.AddBoldAndQuotes()}, перенесён в цикл {targetCycle.Name.AddBoldAndQuotes()}",
                        "Выберите интересующий цикл");
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    if (this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().DayId == targetDomainID)
                    {
                        responseConverter = new ResponseTextConverter($"Ошибка при переносе упражнения!", "Нельзя перенести упражнение в тот же самый день",
                            $"Выберите день, в который хотите перенести упражнение {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.Name.AddBoldAndQuotes()}");
                        buttonsSets = (ButtonsSet.ReplaceToDay, ButtonsSet.SettingExercise);

                        informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                        return informationSet;
                    }

                    DTODay targetDay = this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles.SelectMany(cycle => cycle.Days)
                                                                                                      ?.FirstOrDefault(day => day.Id == targetDomainID)
                        ?? throw new InvalidOperationException($"Не удалось найти targetDomain c ID = '{targetDomainID}' среди дней неархивных циклов у пользователя '{this.CommandHandlerTools.CurrentUserContext.UserInformation.UserId}'");

                    DTOExercise currentExercise = this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise;

                    targetDay.Exercises.Add(currentExercise);
                    DTODay oldDayByExercise = currentExercise.Day.ThrowIfNull();
                    oldDayByExercise.Exercises.Remove(currentExercise);

                    currentExercise.Day = targetDay;
                    currentExercise.DayId = targetDay.Id;

                    await this.CommandHandlerTools.Db.UpdateEntity(currentExercise);

                    responseConverter = new ResponseTextConverter($"Упражнение {currentExercise.Name.AddBoldAndQuotes()}, перенесёно в день {targetDay.Name.AddBoldAndQuotes()}",
                        "Выберите интересующий цикл");
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ChangeNameCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycle:
                    this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.ChangeNameCycle);

                    responseConverter = new ResponseTextConverter($"Введите новоё название для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingCycle);
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.ChangeNameDay);

                    responseConverter = new ResponseTextConverter($"Введите новоё название для дня {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingDay);
                    break;

                case CommonConsts.DomainsAndEntities.Exercise:
                    this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.ChangeNameExercise);

                    responseConverter = new ResponseTextConverter($"Введите новоё название для упражнения {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercise);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ChangeModeCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Exercise:

                    responseConverter = new ResponseTextConverter($"Выберите новый тип для упражнения {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.ChangeType, ButtonsSet.SettingExercise);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ChangedModeCommand()
        {
            IDTODomain DTODomain = this.CommandHandlerTools.CurrentUserContext.DataManager.GetRequiredCurrentDomain(callbackQueryParser.DomainType);

            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Exercise:

                    string? modeStr = this.callbackQueryParser.AdditionalParameters.FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(modeStr))
                        throw new InvalidOperationException($"Не удалось получить новый режим упражнения");

                    ExercisesMods newMode = (ExercisesMods)int.Parse(modeStr);

                    ((Exercise)DTODomain).Mode = newMode;
                    await this.CommandHandlerTools.Db.UpdateEntity(DTODomain);

                    responseConverter = new ResponseTextConverter($"Режим для упражнения {DTODomain.Name.AddBoldAndQuotes()} изменён на {newMode.ToString().AddBoldAndQuotes()}",
                        $"Выберите интересующую настройку для упражнения {DTODomain.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> Period()
        {
            IInformationSet informationSet;

            string operation = callbackQueryParser.AdditionalParameters.First();
            int monthFilterPeriod = int.Parse(callbackQueryParser.AdditionalParameters.Skip(1).First());

            switch (operation)
            {
                case "Export/Excel":

                    informationSet = await TryGetExportFile(".xlsx", monthFilterPeriod);
                       
                    break;
                case "Export/JSON":

                    informationSet = await TryGetExportFile(".json", monthFilterPeriod);

                    break;
                default:
                    throw new NotImplementedException($"Неожиданный {nameof(operation)}: {operation}");
            }

            return informationSet;
        }

        private IInformationSet DeleteCommand()
        {
            Dictionary<string, string> additionalParameters = new Dictionary<string, string>() { { "DomainType", callbackQueryParser.DomainType } };
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Account:
                    responseConverter = new ResponseTextConverter("Вы уверены?", $"{"Удаление аккаунта приведёт к полной и безвозвратной потере информации о ваших тренировках".AddBold()}");
                    buttonsSets = (ButtonsSet.ConfirmDelete, ButtonsSet.Settings);

                    additionalParameters.Add("Name", "аккаунт");
                    break;

                case CommonConsts.DomainsAndEntities.Cycle:
                    responseConverter = new ResponseTextConverter("Вы уверены?", $"{"Удаление цикла приведёт к полной и безвозвратной потере информации о ваших тренировках в этом цикле".AddBold()}");
                    buttonsSets = (ButtonsSet.ConfirmDelete, ButtonsSet.SettingCycle);

                    additionalParameters.Add("Name", "цикл");
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    responseConverter = new ResponseTextConverter("Вы уверены?", $"{"Удаление дня приведёт к полной и безвозвратной потере информации о ваших тренировках в этом дне".AddBold()}");
                    buttonsSets = (ButtonsSet.ConfirmDelete, ButtonsSet.SettingDay);

                    additionalParameters.Add("Name", "день");
                    break;

                case CommonConsts.DomainsAndEntities.Exercise:
                    responseConverter = new ResponseTextConverter("Вы уверены?", $"{"Удаление упражнения приведёт к полной и безвозвратной потере информации о ваших тренировках с этим упражнением".AddBold()}");
                    buttonsSets = (ButtonsSet.ConfirmDelete, ButtonsSet.SettingExercise);

                    additionalParameters.Add("Name", "упражнение");
                    break;

                case CommonConsts.DomainsAndEntities.ResultsExercise:
                    responseConverter = new ResponseTextConverter("Вы уверены?", $"{"Удаление подходов приведёт к полной и безвозвратной потере информации".AddBold()}",
                        "Для удаления введите кол-во последних записей, которые требуется удалить");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercise);

                    this.CommandHandlerTools.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.DeleteResultsExercise);

                    additionalParameters.Add("Name", "упражнение");
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets, additionalParameters);

            return informationSet;
        }

        private async Task<IInformationSet> ConfirmDeleteCommand()
        {
            ResponseTextConverter responseConverter;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            if (callbackQueryParser.DomainType == CommonConsts.DomainsAndEntities.Account)
            {
                this.CommandHandlerTools.ParentHandler.CoreManager.ContextKeeper.RemoveContext(this.CommandHandlerTools.CurrentUserContext.UserInformation.UserId);

                AdminRepository repository = CommandHandlerTools.ParentHandler.CoreManager.GetRequiredRepository<AdminRepository>();
                UserInformation currentUser = await repository.GetRequiredUserInformation(this.CommandHandlerTools.CurrentUserContext.UserInformation.UserId);
                await repository.DeleteAccount(currentUser);
                
                buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                responseConverter = new ResponseTextConverter("Аккаунт успешно удалён");
            }
            else
            {
                IDTODomain DTODomain = this.CommandHandlerTools.CurrentUserContext.DataManager.GetRequiredCurrentDomain(callbackQueryParser.DomainType);

                switch (callbackQueryParser.DomainType)
                {
                    case CommonConsts.DomainsAndEntities.Cycle:
                        if (this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().IsActive)
                        {
                            responseConverter = new ResponseTextConverter("Ошибка при удалении!", "Нельзя удалить активный цикл!",
                                $"Выберите интересующую настройку для цикла {this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                            buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);

                            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

                            return informationSet;
                        }

                        this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles.Remove((DTOCycle)DTODomain);

                        responseConverter = new ResponseTextConverter($"Цикл {DTODomain.Name.AddBoldAndQuotes()} удалён!", "Выберите интересующий цикл");
                        buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                        break;

                    case CommonConsts.DomainsAndEntities.Day:

                        this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Days.Remove((DTODay)DTODomain);

                        responseConverter = new ResponseTextConverter($"День {DTODomain.Name.AddBoldAndQuotes()} удалён!", "Выберите интересующий день");
                        buttonsSets = (ButtonsSet.DaysList, ButtonsSet.SettingDays);
                        break;

                    case CommonConsts.DomainsAndEntities.Exercise:

                        this.CommandHandlerTools.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Exercises.Remove((DTOExercise)DTODomain);

                        responseConverter = new ResponseTextConverter($"Упражнение {DTODomain.Name.AddBoldAndQuotes()} удалёно!", "Выберите интересующее упражнение");
                        buttonsSets = (ButtonsSet.ExercisesList, ButtonsSet.SettingExercises);
                        break;
                    default:
                        throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
                }

                await this.CommandHandlerTools.Db.RemoveEntity(DTODomain);
                this.CommandHandlerTools.CurrentUserContext.DataManager.ResetCurrentDomain(DTODomain);
            }

            informationSet = new MessageInformationSet(responseConverter.Convert(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> TryGetExportFile(string fileExtenstion, int monthFilterPeriod)
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
                SharedCH sharedCH = new SharedCH(this.CommandHandlerTools);

                IQueryable<ResultExercise> resultsExercisesForExcel = sharedCH.GetAllUserResultsExercises();

                if (!resultsExercisesForExcel.Any())
                {
                    responseConverter = new ResponseTextConverter("Отсутствуют результаты для экспорта", 
                        "Выберите интересующую настройку");

                    result = new MessageInformationSet(responseConverter.Convert(), buttonsSets);
                    return result;
                }

                this.CommandHandlerTools.CurrentUserContext.LimitsManager.AddOrUpdateTimeLimit("Export", DateTime.Now.AddHours(6));

                RecyclableMemoryStream recyclableMemoryStream;

                switch (fileExtenstion)
                {
                    case ".xlsx":
                        recyclableMemoryStream = await ExcelExportHelper.GetExcelFile(this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles, resultsExercisesForExcel, monthFilterPeriod);
                    break;
                    case ".json":
                        recyclableMemoryStream = await JsonExportHelper.GetJSONFile(this.CommandHandlerTools.CurrentUserContext.UserInformation.Cycles, resultsExercisesForExcel, monthFilterPeriod);
                    break;
                    default:
                        throw new NotSupportedException("Неподдерживаемый формат экспорта");
                }

                string fileName = $"Workout{fileExtenstion}";

                recyclableMemoryStream.Position = 0;

                result = new FileInformationSet(recyclableMemoryStream, fileName, $"Тренировки успешно экспортированы!", buttonsSets);
            }

            return result;
        }
    }
}