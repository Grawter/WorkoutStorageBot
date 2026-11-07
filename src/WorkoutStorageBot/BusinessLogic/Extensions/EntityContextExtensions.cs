using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.Interfaces;
using WorkoutStorageModels.Entities.BusinessLogic;
using WorkoutStorageModels.Interfaces;

namespace WorkoutStorageBot.BusinessLogic.Extensions
{
    internal static class EntityContextExtensions
    {
        internal static async Task AddEntity(this EntityContext db, IDTOByEntity DTOByEntity, CancellationToken cancellationToken, bool isNeedSaveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(DTOByEntity);

            IEntity entity = ConvertDTOToEntity(DTOByEntity);

            await db.AddAsync(entity, cancellationToken);

            if (isNeedSaveChanges)
            {
                await db.SaveChangesAsync(cancellationToken);

                DTOByEntity.Id = entity.Id;
            }
        }

        internal static async Task AddEntities(this EntityContext db, IEnumerable<IDTODomain> DTODomains, CancellationToken cancellationToken, bool isNeedSaveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(DTODomains);

            List<DTOCycle> DTOCycles = DTODomains.OfType<DTOCycle>().ToList();
            List<DTODay> DTODays = DTODomains.OfType<DTODay>().ToList();
            List<DTOExercise> DTOExercises = DTODomains.OfType<DTOExercise>().ToList();

            if (DTOCycles.Count + DTODays.Count + DTOExercises.Count != DTODomains.Count())
                throw new InvalidOperationException("Для добавления в БД был указан неожиданный тип данных");

            if (DTOCycles.Count > 0)
            {
                var pairs = DTOCycles.Select(x => new { DTO = x, Entity = x.ToCycle() })
                                     .ToList();

                await db.Cycles.AddRangeAsync(pairs.Select(p => p.Entity), cancellationToken);

                if (isNeedSaveChanges)
                {
                    await db.SaveChangesAsync(cancellationToken);

                    foreach (var pair in pairs)
                    {
                        pair.DTO.Id = pair.Entity.Id;
                    }
                }
            }

            if (DTODays.Count > 0)
            {
                var pairs = DTODays.Select(x => new { DTO = x, Entity = x.ToDay() })
                                   .ToList();

                await db.Days.AddRangeAsync(pairs.Select(p => p.Entity), cancellationToken);

                if (isNeedSaveChanges)
                {
                    await db.SaveChangesAsync(cancellationToken);

                    foreach (var pair in pairs)
                    {
                        pair.DTO.Id = pair.Entity.Id;
                    }
                }
            }

            if (DTOExercises.Count > 0)
            {
                var pairs = DTOExercises.Select(x => new { DTO = x, Entity = x.ToExercise() })
                                        .ToList();

                await db.Exercises.AddRangeAsync(pairs.Select(p => p.Entity), cancellationToken);

                if (isNeedSaveChanges)
                {
                    await db.SaveChangesAsync(cancellationToken);

                    foreach (var pair in pairs)
                    {
                        pair.DTO.Id = pair.Entity.Id;
                    }
                }
            }
        }

        internal static async Task AddEntities(this EntityContext db, IEnumerable<DTOResultExercise> DTOResultExercises, CancellationToken cancellationToken, bool isNeedSaveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(DTOResultExercises);

            var pairs = DTOResultExercises.Select(x => new { DTO = x, Entity = x.ToResultExercise() })
                                          .ToList();

            await db.ResultsExercises.AddRangeAsync(pairs.Select(p => p.Entity), cancellationToken);

            if (isNeedSaveChanges)
            {
                await db.SaveChangesAsync(cancellationToken);

                foreach (var pair in pairs)
                {
                    pair.DTO.Id = pair.Entity.Id;
                }
            }
        }

        internal static async Task UpdateEntity(this EntityContext db, IDTOByEntity DTOByEntity, CancellationToken cancellationToken, bool isNeedSaveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(DTOByEntity);

            switch (DTOByEntity)
            {
                case DTOCycle DTOCycle:
                    Cycle? cycle = await db.Cycles.FindAsync(DTOCycle.Id, cancellationToken)
                        ?? throw new InvalidOperationException($"Не удалось найти цикл с Id = {DTOCycle.Id} для обновления");

                    if (cycle.Name != DTOCycle.Name)
                        cycle.Name = DTOCycle.Name;

                    if (cycle.UserInformationId != DTOCycle.UserInformationId)
                        cycle.UserInformationId = DTOCycle.UserInformationId;

                    if (cycle.IsActive != DTOCycle.IsActive)
                        cycle.IsActive = DTOCycle.IsActive;

                    if (cycle.IsArchive != DTOCycle.IsArchive)
                        cycle.IsArchive = DTOCycle.IsArchive;

                    break;
                case DTODay DTODay:
                    Day? day = await db.Days.FindAsync(DTODay.Id, cancellationToken)
                        ?? throw new InvalidOperationException($"Не удалось найти день с Id = {DTODay.Id} для обновления");

                    if (day.Name != DTODay.Name)
                        day.Name = DTODay.Name;

                    if (day.CycleId != DTODay.CycleId)
                        day.CycleId = DTODay.CycleId;

                    if (day.IsArchive != DTODay.IsArchive)
                        day.IsArchive = DTODay.IsArchive;

                    break;
                case DTOExercise DTOExercise:
                    Exercise exercise = await db.Exercises.FindAsync(DTOExercise.Id, cancellationToken)
                        ?? throw new InvalidOperationException($"Не удалось найти упражнение с Id = {DTOExercise.Id} для обновления");

                    if (exercise.Name != DTOExercise.Name)
                        exercise.Name = DTOExercise.Name;

                    if (exercise.Mode != DTOExercise.Mode)
                        exercise.Mode = DTOExercise.Mode;

                    if (exercise.DayId != DTOExercise.DayId)
                        exercise.DayId = DTOExercise.DayId;

                    if (exercise.IsArchive != DTOExercise.IsArchive)
                        exercise.IsArchive = DTOExercise.IsArchive;

                    break;
                case DTOResultExercise DTOResultExercise:

                    ResultExercise resultExercise = await db.ResultsExercises.FindAsync(DTOResultExercise.Id, cancellationToken)
                        ?? throw new InvalidOperationException($"Не удалось найти результат упражнение с Id = {DTOResultExercise.Id} для обновления");

                    if (resultExercise.Count != DTOResultExercise.Count)
                        resultExercise.Count = DTOResultExercise.Count;

                    if (resultExercise.Weight != DTOResultExercise.Weight)
                        resultExercise.Weight = DTOResultExercise.Weight;

                    if (resultExercise.FreeResult != DTOResultExercise.FreeResult)
                        resultExercise.FreeResult = DTOResultExercise.FreeResult;

                    if (resultExercise.DateTime != DTOResultExercise.DateTime)
                        resultExercise.DateTime = DTOResultExercise.DateTime;

                    if (resultExercise.ExerciseId != DTOResultExercise.ExerciseId)
                        resultExercise.ExerciseId = DTOResultExercise.ExerciseId;

                    break;
                default:
                    throw new NotSupportedException($"Неподдерживаемый тип IDTOByEntity: {DTOByEntity.GetType().Name}");
            }

            if (isNeedSaveChanges)
                await db.SaveChangesAsync(cancellationToken);
        }

        internal static async Task UpdateEntities(this EntityContext db, IEnumerable<IDTOByEntity> DTOByEntities, CancellationToken cancellationToken, bool isNeedSaveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(DTOByEntities);

            foreach (IDTOByEntity DTOByEntity in DTOByEntities)
            {
                await db.UpdateEntity(DTOByEntity, cancellationToken, false);
            }

            if (isNeedSaveChanges)
                await db.SaveChangesAsync(cancellationToken);
        }

        internal static async Task RemoveEntity(this EntityContext db, IDTOByEntity DTOByEntity, CancellationToken cancellationToken, bool isNeedSaveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(DTOByEntity);

            IEntity entity = DTOByEntity is IDTODomain domain
                ? await db.GetDomainByDTO(domain, cancellationToken)
                : await db.ResultsExercises.FindAsync(DTOByEntity.Id, cancellationToken) ?? throw new InvalidOperationException($"Not found ResultExercise with ID = {DTOByEntity.Id}");

            db.Remove(entity);

            if (isNeedSaveChanges)
                await db.SaveChangesAsync(cancellationToken);
        }

        internal static async Task RemoveEntities(this EntityContext db, IEnumerable<IDTOByEntity> DTOByEntities, CancellationToken cancellationToken, bool isNeedSaveChanges = true)
        {
            ArgumentNullException.ThrowIfNull(DTOByEntities);

            foreach (IDTOByEntity entity in DTOByEntities)
            {
                await db.RemoveEntity(entity, cancellationToken, false);
            }

            if (isNeedSaveChanges)
                await db.SaveChangesAsync(cancellationToken);
        }

        internal static async Task<IDomain> GetDomainByDTO(this EntityContext db, IDTODomain DTODomain, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(DTODomain);

            return DTODomain switch
            {
                DTOCycle DTOCycle
                    => await db.GetDomainWithId(DTOCycle.Id, DomainType.Cycle, cancellationToken),
                DTODay DTODay
                    => await db.GetDomainWithId(DTODay.Id, DomainType.Day, cancellationToken),
                DTOExercise DTOExercise
                    => await db.GetDomainWithId(DTOExercise.Id, DomainType.Exercise, cancellationToken),
                _ => throw new NotImplementedException($"Неожиданный DTODomain: {DTODomain.GetType().Name}")
            };
        }

        private static async Task<IDomain> GetDomainWithId(this EntityContext db, int id, DomainType domainType, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(db);

            return domainType switch
            {
                DomainType.Cycle
                    => await db.Cycles.FindAsync(id, cancellationToken) ?? throw new InvalidOperationException($"Not found cycle with ID = {id}"),
                DomainType.Day
                    => await db.Days.FindAsync(id, cancellationToken) ?? throw new InvalidOperationException($"Not found day with ID = {id}"),
                DomainType.Exercise
                    => await db.Exercises.FindAsync(id, cancellationToken) ?? throw new InvalidOperationException($"Not found Exercise with ID = {id}"),
                _ => throw new NotImplementedException($"Неожиданный domainTyped: {domainType}")
            };
        }

        private static IEntity ConvertDTOToEntity(IDTOByEntity DTOByEntity)
        {
            return DTOByEntity switch
            {
                DTOCycle DTOCycle
                    => DTOCycle.ToCycle(),
                DTODay DTODay
                    => DTODay.ToDay(),
                DTOExercise DTOExercise
                    => DTOExercise.ToExercise(),
                DTOResultExercise DTOResultExercise
                    => DTOResultExercise.ToResultExercise(),
                _ => throw new NotSupportedException($"Неподдерживаемый тип IDTOByEntity: {DTOByEntity.GetType().Name}")
            };
        }
    }
}