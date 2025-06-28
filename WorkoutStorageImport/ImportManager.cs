#region using

using System.Text.Json;
using System.Text.Json.Serialization;
using WorkoutStorageBot.Helpers.Common;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.Domain;
using WorkoutStorageBot.Model.Import;

#endregion

namespace WorkoutStorageImport
{
    internal class ImportManager
    {
        internal ImportManager(EntityContext entityContext)
        {
            EntityContext = CommonHelper.GetIfNotNull(entityContext);
        }

        private readonly EntityContext EntityContext;

        internal string ImportResult { get; private set; } = "Результат будет сформирован после вызова 'StartMigration()'";

        internal void ExecuteImport(UserInformation userInformation, string inputStr, JsonSerializerOptions? options = null)
        {
            List<Cycle> cycles = DeserializeWorkout(inputStr, options);

            Import(userInformation, cycles);
        }

        private List<Cycle> DeserializeWorkout(string workoutStr, JsonSerializerOptions? options)
        {
            JsonSerializerOptions jsonSerializerOptions = options ?? new JsonSerializerOptions()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            };

            List<Cycle> cycles = JsonSerializer.Deserialize<List<Cycle>>(workoutStr, jsonSerializerOptions) ??
                throw new InvalidOperationException("Результат десериализации - null");

            return cycles;
        }

        private void Import(UserInformation userInformation, List<Cycle> cycles)
        {
            DateTime currentDate = DateTime.Now;

            int countCycles = 0, countDays = 0, countExercise = 0, countResultExercise = 0;

            foreach (Cycle cycle in cycles)
            {
                cycle.UserInformationId = userInformation.Id;

                ProcessLightDomain(cycle, userInformation.Id, currentDate, ref countCycles);

                foreach (Day day in cycle.Days)
                {
                    ProcessLightDomain(day, userInformation.Id, currentDate, ref countDays);

                    foreach (Exercise exercise in day.Exercises)
                    {
                        ProcessLightDomain(exercise, userInformation.Id, currentDate, ref countExercise);

                        foreach (ResultExercise resultExercise in exercise.ResultExercises)
                        {
                            ProcessLightDomain(resultExercise, userInformation.Id, currentDate, ref countResultExercise);
                        }
                    }
                }
            }

            EntityContext.SaveChanges();

            ImportResult = GetImportResult(userInformation, countCycles, countDays, countExercise, countResultExercise);

            return;
        }

        private void ProcessLightDomain(ILightDomain lightDomain, int userInformationId, DateTime currentDate, ref int countAddedLightDomain)
        {
            if (TryAddIfLightDomainIsUnique(lightDomain))
            {
                ++countAddedLightDomain;
                AddImportInfoEntry(lightDomain, userInformationId, currentDate);
            }
        }

        private bool TryAddIfLightDomainIsUnique(ILightDomain lightDomain)
        {
            bool lightDomainWasAdded = false;

            switch (lightDomain)
            {
                case Cycle cycle:
                    if (!EntityContext.Cycles.Any(x => x.Id == cycle.Id))
                    {
                        EntityContext.Cycles.Add(cycle);
                        lightDomainWasAdded = true;
                    }
                        
                break;
                case Day day:
                    if (!EntityContext.Days.Any(x => x.Id == day.Id))
                    {
                        EntityContext.Days.Add(day);
                        lightDomainWasAdded = true;
                    }

                    break;

                case Exercise exercise:
                    if (!EntityContext.Exercises.Any(x => x.Id == exercise.Id))
                    {
                        EntityContext.Exercises.Add(exercise);
                        lightDomainWasAdded = true;
                    }
                        
                    break;

                case ResultExercise resultExercise:
                    if (!EntityContext.ResultsExercises.Any(x => x.Id == resultExercise.Id))
                    {
                        EntityContext.ResultsExercises.Add(resultExercise);
                        lightDomainWasAdded = true;
                    }
                        
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный lightDomain: {lightDomain.GetType().FullName}");
            }

            return lightDomainWasAdded;
        }

        private void AddImportInfoEntry(ILightDomain lightDomain, int userInformationId, DateTime currentDate)
        {
            ImportInfo importInfo = new ImportInfo()
            {
                DomainId = lightDomain.Id,
                DomainType = lightDomain.GetType().Name,
                UserInformationId = userInformationId,
                DateTime = currentDate
            };

            EntityContext.ImportInfo.Add(importInfo);
        }

        internal string GetDomainTypeName(ILightDomain lightDomain)
        {
            string domainTypeName;

            if (lightDomain is IDomain domain)
                domainTypeName = domain.Name;
            else
            {
                domainTypeName = lightDomain switch
                {
                    ResultExercise => nameof(ResultExercise),
                    _ => throw new NotImplementedException($"Неожиданный lightDomain: {lightDomain.GetType().FullName}")
                };
            }

            return domainTypeName;
        }

        private string GetImportResult(UserInformation userInformation, int countCycles, int countDays, int countExercise, int countResultExercise)
        {
            int allCountDomains = countCycles + countDays + countExercise + countResultExercise;

            return @$"Для пользователя с userInformationID = '{userInformation.Id}'/telegramID = '{userInformation.UserId}'/username = '{userInformation.Username}' было импортированно: 
{countCycles} циклов,
{countDays} дней,
{countExercise} упражнений,
{countResultExercise} результатов упражнений.
-----------------------
Всего импортированно записей: {allCountDomains}
";
        }
    }
}