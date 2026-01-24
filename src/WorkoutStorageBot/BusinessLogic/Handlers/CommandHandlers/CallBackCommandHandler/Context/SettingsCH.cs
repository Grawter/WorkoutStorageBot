using Microsoft.IO;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Extensions;
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
using WorkoutStorageBot.BusinessLogic.Helpers.SharedBusinessLogic;

namespace WorkoutStorageBot.BusinessLogic.Handlers.CommandHandlers.CallBackCommandHandler.Context
{
    internal class SettingsCH : CallBackCH
    {
        internal SettingsCH(CommandHandlerTools commandHandlerTools, CallbackQueryParser callbackQueryParser) : base(commandHandlerTools, callbackQueryParser)
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
            this.CurrentUserContext.Navigation.SetQueryFrom(QueryFrom.Settings);

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder("Выберите интересующие настройки");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Settings, ButtonsSet.Main);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ArchiveStoreCommand()
        {
            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder("Выберите интересующий архив для разархивирования");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ArchiveList, ButtonsSet.Settings);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ArchiveCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycles:
                    responseTextBuilder = new ResponseTextBuilder("Выберите архивный цикл для разархивирования");
                    buttonsSets = (ButtonsSet.ArchiveCyclesList, ButtonsSet.ArchiveList);
                    break;
                case CommonConsts.DomainsAndEntities.Days:
                    responseTextBuilder = new ResponseTextBuilder("Выберите архивный день для разархивирования");
                    buttonsSets = (ButtonsSet.ArchiveDaysList, ButtonsSet.ArchiveList);
                    break;
                case CommonConsts.DomainsAndEntities.Exercises:
                    responseTextBuilder = new ResponseTextBuilder("Выберите архивное упражнение для разархивирования");
                    buttonsSets = (ButtonsSet.ArchiveExercisesList, ButtonsSet.ArchiveList);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task <IInformationSet> UnArchiveCommand()
        {
            string domainIDStr = callbackQueryParser.GetRequiredAdditionalParameter(0);
            int domainID = int.Parse(domainIDStr);

            IDTODomain? DTODomain = null;

            if (callbackQueryParser.DomainType == CommonConsts.DomainsAndEntities.Cycle)
            {
                DTODomain = this.CurrentUserContext.UserInformation.Cycles.FirstOrDefault(x => x.Id == domainID)
                    ?? throw new InvalidOperationException($"Not found cycle for unarchiving with ID = {domainID}");
            }
            else if (callbackQueryParser.DomainType == CommonConsts.DomainsAndEntities.Day)
            {
                IEnumerable<DTODay> allDays = this.CurrentUserContext.UserInformation.Cycles.SelectMany(x => x.Days);

                DTODomain = allDays.FirstOrDefault(x => x.Id == domainID) ?? throw new InvalidOperationException($"Not found day for unarchiving with ID = {domainID}");
            }
            else if (callbackQueryParser.DomainType == CommonConsts.DomainsAndEntities.Exercise)
            {
                IEnumerable<DTOExercise> allExercise = this.CurrentUserContext.UserInformation.Cycles.SelectMany(x => x.Days)
                                                                                                     .SelectMany(x => x.Exercises);

                DTODomain = allExercise.FirstOrDefault(x => x.Id == domainID) ?? throw new InvalidOperationException($"Not found exercise for unarchiving with ID = {domainID}");
            }
            else
                throw new NotImplementedException($"Неожиданный domainTyped: {callbackQueryParser.DomainType}");

            DTODomain.IsArchive = false;
            await this.Db.UpdateEntity(DTODomain);

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder($"{DTODomain.Name.AddBoldAndQuotes()} разархивирован!");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.ArchiveList, ButtonsSet.Settings);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet AboutBotCommand()
        {
            string? aboutBotText = this.GetBotDescription();

            if (string.IsNullOrWhiteSpace(aboutBotText))
                aboutBotText = "Информации о боте не указано";

            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder(aboutBotText);
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.None, ButtonsSet.Settings);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ExportCommand()
        {
            ResponseTextBuilder responseTextBuilder = new ResponseTextBuilder("Выберите формат в котором экспортировать данные о ваших тренировках");
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Export, ButtonsSet.Settings);

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ExportToCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;
            Dictionary<string, string> additionalParameters;

            string exportFormat = callbackQueryParser.AdditionalParameters.First();

            switch (exportFormat)
            {
                case "Excel":
                    responseTextBuilder = new ResponseTextBuilder("Выберите временной промежуток формирования данных от последней тренировки");
                    buttonsSets = (ButtonsSet.Period, ButtonsSet.Settings);
                    additionalParameters = new() { { "Act", "Export/Excel" } };
                    break;
                case "JSON":
                    responseTextBuilder = new ResponseTextBuilder("Выберите временной промежуток формирования данных от последней тренировки");
                    buttonsSets = (ButtonsSet.Period, ButtonsSet.Settings);
                    additionalParameters = new() { { "Act", "Export/JSON" } };
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets, additionalParameters);

            return informationSet;
        }

        private IInformationSet SettingCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycles:
                    responseTextBuilder = new ResponseTextBuilder("Выберите интересующие настройки для циклов");
                    buttonsSets = (ButtonsSet.SettingCycles, ButtonsSet.Settings);
                    break;
                case CommonConsts.DomainsAndEntities.Days:
                    responseTextBuilder = new ResponseTextBuilder("Выберите интересующие настройки для дней");
                    buttonsSets = (ButtonsSet.SettingDays, ButtonsSet.SettingCycle);
                    break;
                case CommonConsts.DomainsAndEntities.Exercises:
                    responseTextBuilder = new ResponseTextBuilder("Выберите интересующие настройки для упражнений");
                    buttonsSets = (ButtonsSet.SettingExercises, ButtonsSet.SettingDay);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet AddCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycle:
                    this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddCycle);

                    responseTextBuilder = new ResponseTextBuilder("Введите название тренировочного цикла");

                    switch (this.CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                            break;

                        case QueryFrom.Settings:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.SettingCycles);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CurrentUserContext.Navigation.QueryFrom}");
                    }
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddDays);

                    responseTextBuilder = new ResponseTextBuilder($"Введите название тренирочного дня для цикла {this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");

                    switch (this.CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                            break;

                        case QueryFrom.Settings:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.SettingDays);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CurrentUserContext.Navigation.QueryFrom}");
                    }
                    break;

                case CommonConsts.DomainsAndEntities.Exercise:
                    this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddExercises);

                    responseTextBuilder = new ResponseTextBuilder($"Введите название(я) и тип(ы) упражнения(й) для дня {this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}",
                        CommonConsts.Exercise.ExamplesTypesExercise,
                        CommonConsts.Exercise.InputFormatExercise);

                    switch (this.CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.Start:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                            break;

                        case QueryFrom.Settings:
                            buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercises);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CurrentUserContext.Navigation.QueryFrom}");
                    }
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CallbackQueryParser.SubDirection: {callbackQueryParser.SubDirection}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ResetTempDomainsCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Day:
                    throw new NotImplementedException($"Нереализовано для типа домена {CommonConsts.DomainsAndEntities.Day}, т.к. не нашлось ни одно небходимого кейса");

                case CommonConsts.DomainsAndEntities.Exercise:
                    this.CurrentUserContext.DataManager.ResetTempExercises();

                    responseTextBuilder = new ResponseTextBuilder("Упражнения для сохранения сброшены!", "Выберите интересующую настройку");
                    buttonsSets = (ButtonsSet.SettingExercises, ButtonsSet.None);

                    break;

                default:
                    throw new InvalidOperationException($"Неожиданный {nameof(callbackQueryParser.DomainType)}: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> SaveExercisesCommand()
        {
            foreach (DTOExercise tempExercise in this.CurrentUserContext.DataManager.TempExercises.ThrowIfNull())
            {
                tempExercise.Day = this.CurrentUserContext.DataManager.CurrentDay;
                this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Exercises.Add(tempExercise);
            }
            await this.Db.AddEntities(this.CurrentUserContext.DataManager.TempExercises);

            this.CurrentUserContext.DataManager.ResetTempExercises();

            this.CurrentUserContext.Navigation.ResetMessageNavigationTarget();

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (this.CurrentUserContext.Navigation.QueryFrom)
            {
                case QueryFrom.Start:
                    responseTextBuilder = new ResponseTextBuilder("Упражнения сохранены!", "Выберите дальнейшее действие");
                    buttonsSets = (ButtonsSet.RedirectAfterSaveExercise, ButtonsSet.None);
                    break;

                case QueryFrom.Settings:
                    responseTextBuilder = new ResponseTextBuilder("Упражнения сохранены!", "Выберите интересующие настройки для упражнений");
                    buttonsSets = (ButtonsSet.SettingExercises, ButtonsSet.SettingDays);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CurrentUserContext.Navigation.QueryFrom}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet SettingExistingCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycles:
                    responseTextBuilder = new ResponseTextBuilder("Выберите интересующий цикл");
                    buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                    break;
                case CommonConsts.DomainsAndEntities.Days:
                    responseTextBuilder = new ResponseTextBuilder("Выберите интересующий день");
                    buttonsSets = (ButtonsSet.DaysList, ButtonsSet.SettingDays);
                    break;
                case CommonConsts.DomainsAndEntities.Exercises:
                    responseTextBuilder = new ResponseTextBuilder("Выберите интересующее упражнение");
                    buttonsSets = (ButtonsSet.ExercisesList, ButtonsSet.SettingExercises);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet SelectedCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            string domainIDStr = callbackQueryParser.GetRequiredAdditionalParameter(0);
            int domainID = int.Parse(domainIDStr);

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycle:
                    this.CurrentUserContext.DataManager.SetCurrentDomain(this.CurrentUserContext.UserInformation.Cycles.First(c => c.Id == domainID));

                    responseTextBuilder = new ResponseTextBuilder($"Выберите интересующую настройку для цикла {this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    switch (this.CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:
                            this.CurrentUserContext.DataManager.SetCurrentDomain(this.CurrentUserContext.ActiveCycle.ThrowIfNull().Days.First(d => d.Id == domainID));

                            responseTextBuilder = new ResponseTextBuilder("Выберите упраженение");
                            buttonsSets = (ButtonsSet.ExercisesListWithLastWorkoutForDay, ButtonsSet.DaysListWithLastWorkout);
                            break;

                        case QueryFrom.Settings:
                            this.CurrentUserContext.DataManager.SetCurrentDomain(this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Days.First(d => d.Id == domainID));

                            responseTextBuilder = new ResponseTextBuilder($"Выберите интересующую настройку для дня {this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}");
                            buttonsSets = (ButtonsSet.SettingDay, ButtonsSet.DaysList);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CurrentUserContext.Navigation.QueryFrom}");
                    }
                    break;

                case CommonConsts.DomainsAndEntities.Exercise:
                    DTOExercise currentExercise = this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Exercises.First(e => e.Id == domainID);

                    this.CurrentUserContext.DataManager.SetCurrentDomain(currentExercise);

                    string currentExerciseNameBoldAndQuotes = currentExercise.Name.AddBoldAndQuotes();

                    string inputFormatExerciseResult = currentExercise.Mode switch
                    {
                        ExercisesMods.Count => CommonConsts.ResultExercise.InputFormatExerciseResultCount,
                        ExercisesMods.WeightCount => CommonConsts.ResultExercise.InputFormatExerciseResultWeightCount,
                        ExercisesMods.Timer => CommonConsts.ResultExercise.InputFormatExerciseResultTimer,
                        ExercisesMods.FreeResult => CommonConsts.ResultExercise.InputFormatExerciseResultFreeResult,
                        _ => throw new NotImplementedException($"Неожиданный тип упражнения: {currentExercise.Mode.ToString()}")
                    };

                    switch (this.CurrentUserContext.Navigation.QueryFrom)
                    {
                        case QueryFrom.NoMatter:
                            if (currentExercise.Mode == ExercisesMods.Timer)
                            {
                                responseTextBuilder = new ResponseTextBuilder($"Включение таймера для упражнения {currentExerciseNameBoldAndQuotes}");
                                buttonsSets = (ButtonsSet.EnableExerciseTimer, ButtonsSet.ExercisesListWithLastWorkoutForDay);
                            }
                            else
                            {
                                this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.AddResultForExercise);

                                responseTextBuilder = new ResponseTextBuilder($"Фиксирование результатов упражнения {currentExerciseNameBoldAndQuotes}",
                                    inputFormatExerciseResult,
                                    $"Введите результат(ы) подхода(ов)");
                                buttonsSets = (ButtonsSet.None, ButtonsSet.ExercisesListWithLastWorkoutForDay);
                            }
                                
                            break;

                        case QueryFrom.Settings:
                            responseTextBuilder = new ResponseTextBuilder($"Выберите интересующую настройку для упражнения {currentExerciseNameBoldAndQuotes}");
                            buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);
                            break;
                        default:
                            throw new NotImplementedException($"Неожиданный CurrentUserContext.Navigation.QueryFrom: {this.CurrentUserContext.Navigation.QueryFrom}");
                    }
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ChangeActiveCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycle:

                    this.CurrentUserContext.ActiveCycle.ThrowIfNull();

                    if (this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().IsActive)
                    {
                        responseTextBuilder = new ResponseTextBuilder($"Выбранный цикл {this.CurrentUserContext.ActiveCycle.Name.AddBoldAndQuotes()} уже является активным!",
                            $"Выберите интересующую настройку для цикла {this.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                        buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.SettingCycles);

                        informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

                        return informationSet;
                    }

                    this.CurrentUserContext.ActiveCycle.IsActive = false;
                    await this.Db.UpdateEntity(this.CurrentUserContext.ActiveCycle, false);
                    this.CurrentUserContext.UpdateActiveCycleForce(this.CurrentUserContext.DataManager.CurrentCycle);
                    await this.Db.UpdateEntity(this.CurrentUserContext.ActiveCycle);

                    responseTextBuilder = new ResponseTextBuilder($"Активный цикл изменён на {this.CurrentUserContext.ActiveCycle.Name.AddBoldAndQuotes()}",
                    $"Выберите интересующую настройку для цикла {this.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.SettingCycles);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ArchivingCommand()
        {
            IDTODomain DTODomain = this.CurrentUserContext.DataManager.GetRequiredCurrentDomain(callbackQueryParser.DomainType);

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycle:
                    if (this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().IsActive)
                    {
                        responseTextBuilder = new ResponseTextBuilder("Ошибка при архивации!", "Нельзя архивировать активный цикл!",
                            $"Выберите интересующую настройку для цикла {this.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                        buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);

                        informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

                        return informationSet;
                    }

                    responseTextBuilder = new ResponseTextBuilder($"Цикл {DTODomain.Name.AddBoldAndQuotes()} был добавлен в архив", $"Выберите интересующий цикл");
                    buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    responseTextBuilder = new ResponseTextBuilder($"День {DTODomain.Name.AddBoldAndQuotes()} был добавлен в архив", $"Выберите интересующий день");
                    buttonsSets = (ButtonsSet.DaysList, ButtonsSet.SettingDays);
                    break;

                case CommonConsts.DomainsAndEntities.Exercise:
                    responseTextBuilder = new ResponseTextBuilder($"Упражнение {DTODomain.Name.AddBoldAndQuotes()} было добавлено в архив", $"Выберите интересующее упражнение");
                    buttonsSets = (ButtonsSet.ExercisesList, ButtonsSet.SettingExercises);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            DTODomain.IsArchive = true;
            await this.Db.UpdateEntity(DTODomain);

            this.CurrentUserContext.DataManager.ResetCurrentDomain(DTODomain);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ReplaceCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Day:
                    responseTextBuilder = new ResponseTextBuilder($"Выберите цикл, в который хотите перенести день {this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay);
                    break;
                case CommonConsts.DomainsAndEntities.Exercise:
                    responseTextBuilder = new ResponseTextBuilder($"Выберите день, в который хотите перенести упражнение {this.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.ReplaceToDay, ButtonsSet.SettingExercise);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ReplaceToCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            string targetDomainIDStr = callbackQueryParser.GetRequiredAdditionalParameter(0);
            int targetDomainID = int.Parse(targetDomainIDStr);

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycle:
                    if (this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().CycleId == targetDomainID)
                    {
                        responseTextBuilder = new ResponseTextBuilder($"Ошибка при переносе дня!", "Нельзя перенести день в тот же самый цикл",
                            $"Выберите цикл, в который хотите перенести день {this.CurrentUserContext.DataManager.CurrentDay.Name.AddBoldAndQuotes()}");
                        buttonsSets = (ButtonsSet.ReplaceToCycle, ButtonsSet.SettingDay);

                        informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

                        return informationSet;
                    }

                    DTOCycle targetCycle = this.CurrentUserContext.UserInformation.Cycles.FirstOrDefault(x => x.Id == targetDomainID)
                        ?? throw new InvalidOperationException($"Не удалось найти targetDomain c ID = '{targetDomainID}' среди циклов у пользователя '{this.CurrentUserContext.UserInformation.UserId}'");

                    DTODay currentDay = this.CurrentUserContext.DataManager.CurrentDay;

                    targetCycle.Days.Add(currentDay);
                    DTOCycle oldCycleByDay = currentDay.Cycle.ThrowIfNull();
                    oldCycleByDay.Days.Remove(currentDay);

                    currentDay.Cycle = targetCycle;
                    currentDay.CycleId = targetCycle.Id;

                    await this.Db.UpdateEntity(currentDay);

                    responseTextBuilder = new ResponseTextBuilder($"День {currentDay.Name.AddBoldAndQuotes()}, перенесён в цикл {targetCycle.Name.AddBoldAndQuotes()}",
                        "Выберите интересующий цикл");
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    if (this.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().DayId == targetDomainID)
                    {
                        responseTextBuilder = new ResponseTextBuilder($"Ошибка при переносе упражнения!", "Нельзя перенести упражнение в тот же самый день",
                            $"Выберите день, в который хотите перенести упражнение {this.CurrentUserContext.DataManager.CurrentExercise.Name.AddBoldAndQuotes()}");
                        buttonsSets = (ButtonsSet.ReplaceToDay, ButtonsSet.SettingExercise);

                        informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

                        return informationSet;
                    }

                    DTODay targetDay = this.CurrentUserContext.UserInformation.Cycles.SelectMany(cycle => cycle.Days)
                                                                                                      ?.FirstOrDefault(day => day.Id == targetDomainID)
                        ?? throw new InvalidOperationException($"Не удалось найти targetDomain c ID = '{targetDomainID}' среди дней неархивных циклов у пользователя '{this.CurrentUserContext.UserInformation.UserId}'");

                    DTOExercise currentExercise = this.CurrentUserContext.DataManager.CurrentExercise;

                    targetDay.Exercises.Add(currentExercise);
                    DTODay oldDayByExercise = currentExercise.Day.ThrowIfNull();
                    oldDayByExercise.Exercises.Remove(currentExercise);

                    currentExercise.Day = targetDay;
                    currentExercise.DayId = targetDay.Id;

                    await this.Db.UpdateEntity(currentExercise);

                    responseTextBuilder = new ResponseTextBuilder($"Упражнение {currentExercise.Name.AddBoldAndQuotes()}, перенесёно в день {targetDay.Name.AddBoldAndQuotes()}",
                        "Выберите интересующий цикл");
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ChangeNameCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Cycle:
                    this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.ChangeNameCycle);

                    responseTextBuilder = new ResponseTextBuilder($"Введите новоё название для цикла {this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingCycle);
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.ChangeNameDay);

                    responseTextBuilder = new ResponseTextBuilder($"Введите новоё название для дня {this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingDay);
                    break;

                case CommonConsts.DomainsAndEntities.Exercise:
                    this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.ChangeNameExercise);

                    responseTextBuilder = new ResponseTextBuilder($"Введите новоё название для упражнения {this.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercise);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private IInformationSet ChangeModeCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Exercise:

                    responseTextBuilder = new ResponseTextBuilder($"Выберите новый тип для упражнения {this.CurrentUserContext.DataManager.CurrentExercise.ThrowIfNull().Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.ChangeType, ButtonsSet.SettingExercise);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> ChangedModeCommand()
        {
            IDTODomain DTODomain = this.CurrentUserContext.DataManager.GetRequiredCurrentDomain(callbackQueryParser.DomainType);

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Exercise:

                    string? modeStr = this.callbackQueryParser.AdditionalParameters.FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(modeStr))
                        throw new InvalidOperationException($"Не удалось получить новый режим упражнения");

                    ExercisesMods newMode = (ExercisesMods)int.Parse(modeStr);

                    ((Exercise)DTODomain).Mode = newMode;
                    await this.Db.UpdateEntity(DTODomain);

                    responseTextBuilder = new ResponseTextBuilder($"Режим для упражнения {DTODomain.Name.AddBoldAndQuotes()} изменён на {newMode.ToString().AddBoldAndQuotes()}",
                        $"Выберите интересующую настройку для упражнения {DTODomain.Name.AddBoldAndQuotes()}");
                    buttonsSets = (ButtonsSet.SettingExercise, ButtonsSet.ExercisesList);
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

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
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;

            switch (callbackQueryParser.DomainType)
            {
                case CommonConsts.DomainsAndEntities.Account:
                    responseTextBuilder = new ResponseTextBuilder("Вы уверены?", $"{"Удаление аккаунта приведёт к полной и безвозвратной потере информации о ваших тренировках".AddBold()}");
                    buttonsSets = (ButtonsSet.ConfirmDelete, ButtonsSet.Settings);

                    additionalParameters.Add("Name", "аккаунт");
                    break;

                case CommonConsts.DomainsAndEntities.Cycle:
                    responseTextBuilder = new ResponseTextBuilder("Вы уверены?", $"{"Удаление цикла приведёт к полной и безвозвратной потере информации о ваших тренировках в этом цикле".AddBold()}");
                    buttonsSets = (ButtonsSet.ConfirmDelete, ButtonsSet.SettingCycle);

                    additionalParameters.Add("Name", "цикл");
                    break;

                case CommonConsts.DomainsAndEntities.Day:
                    responseTextBuilder = new ResponseTextBuilder("Вы уверены?", $"{"Удаление дня приведёт к полной и безвозвратной потере информации о ваших тренировках в этом дне".AddBold()}");
                    buttonsSets = (ButtonsSet.ConfirmDelete, ButtonsSet.SettingDay);

                    additionalParameters.Add("Name", "день");
                    break;

                case CommonConsts.DomainsAndEntities.Exercise:
                    responseTextBuilder = new ResponseTextBuilder("Вы уверены?", $"{"Удаление упражнения приведёт к полной и безвозвратной потере информации о ваших тренировках с этим упражнением".AddBold()}");
                    buttonsSets = (ButtonsSet.ConfirmDelete, ButtonsSet.SettingExercise);

                    additionalParameters.Add("Name", "упражнение");
                    break;

                case CommonConsts.DomainsAndEntities.ResultsExercise:
                    responseTextBuilder = new ResponseTextBuilder("Вы уверены?", $"{"Удаление подходов приведёт к полной и безвозвратной потере информации".AddBold()}",
                        "Для удаления введите кол-во последних записей, которые требуется удалить");
                    buttonsSets = (ButtonsSet.None, ButtonsSet.SettingExercise);

                    this.CurrentUserContext.Navigation.SetMessageNavigationTarget(MessageNavigationTarget.DeleteResultsExercise);

                    additionalParameters.Add("Name", "упражнение");
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
            }

            IInformationSet informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets, additionalParameters);

            return informationSet;
        }

        private async Task<IInformationSet> ConfirmDeleteCommand()
        {
            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets;
            IInformationSet informationSet;

            if (callbackQueryParser.DomainType == CommonConsts.DomainsAndEntities.Account)
            {
                this.ContextKeeper.RemoveContext(this.CurrentUserContext.UserInformation.UserId);

                AdminRepository repository = GetRequiredRepository<AdminRepository>();
                UserInformation currentUser = await repository.GetRequiredUserInformation(this.CurrentUserContext.UserInformation.UserId);
                await repository.DeleteAccount(currentUser);
                
                buttonsSets = (ButtonsSet.None, ButtonsSet.None);
                responseTextBuilder = new ResponseTextBuilder("Аккаунт успешно удалён");
            }
            else
            {
                IDTODomain DTODomain = this.CurrentUserContext.DataManager.GetRequiredCurrentDomain(callbackQueryParser.DomainType);

                switch (callbackQueryParser.DomainType)
                {
                    case CommonConsts.DomainsAndEntities.Cycle:
                        if (this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().IsActive)
                        {
                            responseTextBuilder = new ResponseTextBuilder("Ошибка при удалении!", "Нельзя удалить активный цикл!",
                                $"Выберите интересующую настройку для цикла {this.CurrentUserContext.DataManager.CurrentCycle.Name.AddBoldAndQuotes()}");
                            buttonsSets = (ButtonsSet.SettingCycle, ButtonsSet.CycleList);

                            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

                            return informationSet;
                        }

                        this.CurrentUserContext.UserInformation.Cycles.Remove((DTOCycle)DTODomain);

                        responseTextBuilder = new ResponseTextBuilder($"Цикл {DTODomain.Name.AddBoldAndQuotes()} удалён!", "Выберите интересующий цикл");
                        buttonsSets = (ButtonsSet.CycleList, ButtonsSet.SettingCycles);
                        break;

                    case CommonConsts.DomainsAndEntities.Day:

                        this.CurrentUserContext.DataManager.CurrentCycle.ThrowIfNull().Days.Remove((DTODay)DTODomain);

                        responseTextBuilder = new ResponseTextBuilder($"День {DTODomain.Name.AddBoldAndQuotes()} удалён!", "Выберите интересующий день");
                        buttonsSets = (ButtonsSet.DaysList, ButtonsSet.SettingDays);
                        break;

                    case CommonConsts.DomainsAndEntities.Exercise:

                        this.CurrentUserContext.DataManager.CurrentDay.ThrowIfNull().Exercises.Remove((DTOExercise)DTODomain);

                        responseTextBuilder = new ResponseTextBuilder($"Упражнение {DTODomain.Name.AddBoldAndQuotes()} удалёно!", "Выберите интересующее упражнение");
                        buttonsSets = (ButtonsSet.ExercisesList, ButtonsSet.SettingExercises);
                        break;
                    default:
                        throw new NotImplementedException($"Неожиданный callbackQueryParser.ObjectType: {callbackQueryParser.DomainType}");
                }

                await this.Db.RemoveEntity(DTODomain);
                this.CurrentUserContext.DataManager.ResetCurrentDomain(DTODomain);
            }

            informationSet = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);

            return informationSet;
        }

        private async Task<IInformationSet> TryGetExportFile(string fileExtenstion, int monthFilterPeriod)
        {
            if (string.IsNullOrWhiteSpace(fileExtenstion) || !fileExtenstion.StartsWith("."))
                throw new InvalidOperationException($"Получено некорректное расширение файла");

            ResponseTextBuilder responseTextBuilder;
            (ButtonsSet, ButtonsSet) buttonsSets = (ButtonsSet.Settings, ButtonsSet.Main);

            IInformationSet result;

            if (this.CurrentUserContext.LimitsManager.HasBlockByTimeLimit("Export", out DateTime limit))
            {
                responseTextBuilder =
                    new ResponseTextBuilder($"Следующее выполнение этой операции будет доступно {limit.ToString(CommonConsts.Common.DateTimeFormatHoursFirst)})",
                                                "Выберите интересующую настройку");

                result = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);
            }
            else
            {
                IQueryable<ResultExercise> resultsExercisesForExcel = SharedExercisesAndResultsLogicHelper.GetAllUserResultsExercises(this.Db, this.CurrentUserContext);

                if (!resultsExercisesForExcel.Any())
                {
                    responseTextBuilder = new ResponseTextBuilder("Отсутствуют результаты для экспорта", 
                        "Выберите интересующую настройку");

                    result = new MessageInformationSet(responseTextBuilder.Build(), buttonsSets);
                    return result;
                }

                this.CurrentUserContext.LimitsManager.AddOrUpdateTimeLimit("Export", DateTime.Now.AddHours(6));

                RecyclableMemoryStream recyclableMemoryStream;

                switch (fileExtenstion)
                {
                    case ".xlsx":
                        recyclableMemoryStream = await ExcelExportHelper.GetExcelFile(this.CurrentUserContext.UserInformation.Cycles, resultsExercisesForExcel, monthFilterPeriod);
                    break;
                    case ".json":
                        recyclableMemoryStream = await JsonExportHelper.GetJSONFile(this.CurrentUserContext.UserInformation.Cycles, resultsExercisesForExcel, monthFilterPeriod);
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