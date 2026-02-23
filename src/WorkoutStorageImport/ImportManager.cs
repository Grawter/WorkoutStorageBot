using System.Text.Json;
using System.Text.Json.Serialization;
using WorkoutStorageImport.Models;
using WorkoutStorageModels.Entities.BusinessLogic;
using WorkoutStorageModels.Entities.Import;
using WorkoutStorageModels.Interfaces;

namespace WorkoutStorageImport
{
    internal class ImportManager
    {
        internal ImportManager(EntityContext entityContext)
        {
            ArgumentNullException.ThrowIfNull(entityContext);

            EntityContext = entityContext;
        }

        private readonly EntityContext EntityContext;

        internal string ImportResult { get; private set; } = "Результат будет сформирован после вызова 'StartMigration()'";

        internal void ExecuteImport(UserInformation userInformation, string inputStr, JsonSerializerOptions? options = null)
        {
            DTOUserInformation DTOUserInformation = DeserializationWorkout(inputStr, options);

            Import(userInformation, DTOUserInformation.Cycles);
        }

        private DTOUserInformation DeserializationWorkout(string workoutStr, JsonSerializerOptions? options)
        {
            JsonSerializerOptions jsonSerializerOptions = options ?? new JsonSerializerOptions()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            };

            DTOUserInformation DTOUserInformation = JsonSerializer.Deserialize<DTOUserInformation>(workoutStr, jsonSerializerOptions) ??
                throw new InvalidOperationException("Результат десериализации - null");

            return DTOUserInformation;
        }

        private void Import(UserInformation userInformation, List<Cycle> cycles)
        {
            DateTime currentDate = DateTime.Now;

            int countCycles = 0, countDays = 0, countExercise = 0, countResultExercise = 0;

            foreach (Cycle cycle in cycles)
            {
                cycle.UserInformationId = userInformation.Id;

                ProcessEntity(cycle, userInformation.Id, currentDate, ref countCycles);

                foreach (Day day in cycle.Days)
                {
                    ProcessEntity(day, userInformation.Id, currentDate, ref countDays);

                    foreach (Exercise exercise in day.Exercises)
                    {
                        ProcessEntity(exercise, userInformation.Id, currentDate, ref countExercise);

                        foreach (ResultExercise resultExercise in exercise.ResultsExercise)
                        {
                            ProcessEntity(resultExercise, userInformation.Id, currentDate, ref countResultExercise);
                        }
                    }
                }
            }

            EntityContext.SaveChanges();

            ImportResult = GetImportResult(userInformation, countCycles, countDays, countExercise, countResultExercise);

            return;
        }

        private void ProcessEntity(IEntity entityDomain, int userInformationId, DateTime currentDate, ref int countAddedEntityDomain)
        {
            if (TryAddIfEntityDomainIsUnique(entityDomain))
            {
                ++countAddedEntityDomain;
                AddImportInfoEntry(entityDomain, userInformationId, currentDate);
            }
        }

        private bool TryAddIfEntityDomainIsUnique(IEntity entityDomain)
        {
            bool entityWasAdded = false;

            switch (entityDomain)
            {
                case Cycle cycle:
                    if (!EntityContext.Cycles.Any(x => x.Id == cycle.Id))
                    {
                        EntityContext.Cycles.Add(cycle);
                        entityWasAdded = true;
                    }
                        
                break;
                case Day day:
                    if (!EntityContext.Days.Any(x => x.Id == day.Id))
                    {
                        EntityContext.Days.Add(day);
                        entityWasAdded = true;
                    }

                    break;

                case Exercise exercise:
                    if (!EntityContext.Exercises.Any(x => x.Id == exercise.Id))
                    {
                        EntityContext.Exercises.Add(exercise);
                        entityWasAdded = true;
                    }
                        
                    break;

                case ResultExercise resultExercise:
                    if (!EntityContext.ResultsExercises.Any(x => x.Id == resultExercise.Id))
                    {
                        EntityContext.ResultsExercises.Add(resultExercise);
                        entityWasAdded = true;
                    }
                        
                    break;
                default:
                    throw new NotImplementedException($"Неожиданный entityDomain: {entityDomain.GetType().FullName}");
            }

            return entityWasAdded;
        }

        private void AddImportInfoEntry(IEntity entityDomain, int userInformationId, DateTime currentDate)
        {
            ImportInfo importInfo = new ImportInfo()
            {
                DomainId = entityDomain.Id,
                DomainType = entityDomain.GetType().Name,
                UserInformationId = userInformationId,
                DateTime = currentDate
            };

            EntityContext.ImportInfo.Add(importInfo);
        }

        internal string GetDomainTypeName(IEntity entityDomain)
        {
            string domainTypeName;

            if (entityDomain is IDomain domain)
                domainTypeName = domain.Name;
            else
            {
                domainTypeName = entityDomain switch
                {
                    ResultExercise => nameof(ResultExercise),
                    _ => throw new NotImplementedException($"Неожиданный entityDomain: {entityDomain.GetType().FullName}")
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