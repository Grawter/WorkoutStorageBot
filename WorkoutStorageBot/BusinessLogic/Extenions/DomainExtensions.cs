using Microsoft.EntityFrameworkCore;
using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.BusinessLogic.Helpers.Converters;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.Interfaces;
using WorkoutStorageModels.Entities.BusinessLogic;
using WorkoutStorageModels.Interfaces;

namespace WorkoutStorageBot.BusinessLogic.Extensions
{
    internal static class DomainExtensions
    {
        internal static async Task AddEntity(this EntityContext db, IDTOByEntity DTOByEntity, bool isNeedSave = true)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(DTOByEntity);

            IEntity entity = ConvertDTOToEntity(DTOByEntity);

            await db.AddAsync(entity);

            if (isNeedSave)
            {
                await db.SaveChangesAsync();

                DTOByEntity.Id = entity.Id;
            }
        }

        internal static async Task AddEntities(this EntityContext db, IEnumerable<DTOExercise> DTOExercises, bool isNeedSave = true)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(DTOExercises);

            var pairs = DTOExercises.Select(x => new { DTO = x, Entity = EntityConverter.ToExercise(x) });

            await db.Exercises.AddRangeAsync(pairs.Select(p => p.Entity));

            if (isNeedSave)
            {
                await db.SaveChangesAsync();

                foreach (var pair in pairs)
                {
                    pair.DTO.Id = pair.Entity.Id;
                }
            }
        }

        internal static async Task AddEntities(this EntityContext db, IEnumerable<DTOResultExercise> DTOResultExercise, bool isNeedSave = true)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(DTOResultExercise);

            var pairs = DTOResultExercise.Select(x => new { DTO = x, Entity = EntityConverter.ToResultExercise(x) });

            await db.ResultsExercises.AddRangeAsync(pairs.Select(p => p.Entity));

            if (isNeedSave)
            {
                await db.SaveChangesAsync();

                foreach (var pair in pairs)
                {
                    pair.DTO.Id = pair.Entity.Id;
                }
            }
        }

        internal static async Task UpdateEntity(this EntityContext db, IDTOByEntity DTOByEntity, bool isNeedSave = true)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(DTOByEntity);

            switch (DTOByEntity)
            {
                case DTOCycle DTOCycle:
                    Cycle cycle = await db.Cycles.FirstAsync(x => x.Id == DTOCycle.Id);

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
                    Day day = await db.Days.FirstAsync(x => x.Id == DTODay.Id);

                    if (day.Name != DTODay.Name)
                        day.Name = DTODay.Name;

                    if (day.CycleId != DTODay.CycleId)
                        day.CycleId = DTODay.CycleId;

                    if (day.IsArchive != DTODay.IsArchive)
                        day.IsArchive = DTODay.IsArchive;

                    break;
                case DTOExercise DTOExercise:
                    Exercise exercise = await db.Exercises.FirstAsync(x => x.Id == DTOExercise.Id);

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

                    ResultExercise resultExercise = await db.ResultsExercises.FirstAsync(x => x.Id == DTOResultExercise.Id);

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

            if (isNeedSave)
                await db.SaveChangesAsync();
        }

        internal static async Task UpdateEntities(this EntityContext db, ICollection<IDTOByEntity> DTOByEntities, bool isNeedSave = true)
        {
            ArgumentNullException.ThrowIfNull(DTOByEntities);

            foreach (IDTOByEntity DTOByEntity in DTOByEntities)
            {
                await db.UpdateEntity(DTOByEntity, false);
            }

            if (isNeedSave)
                await db.SaveChangesAsync();
        }

        internal static async Task RemoveEntity(this EntityContext db, IDTOByEntity DTOByEntity, bool isNeedSave = true)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(DTOByEntity);

            IEntity entity = ConvertDTOToEntity(DTOByEntity);

            db.Remove(entity);

            if (isNeedSave)
                await db.SaveChangesAsync();
        }

        internal static async Task RemoveEntities(this EntityContext db, ICollection<IDTOByEntity> DTOByEntities, bool isNeedSave = true)
        {
            ArgumentNullException.ThrowIfNull(DTOByEntities);

            foreach (IDTOByEntity entity in DTOByEntities)
            {
                await db.RemoveEntity(entity, false);
            }

            if (isNeedSave)
                await db.SaveChangesAsync();
        }

        internal static async Task<IDomain> GetDomainByDTO(this EntityContext db, IDTODomain DTODomain)
        {
            ArgumentNullException.ThrowIfNull(DTODomain);

            return DTODomain switch
            {
                DTOCycle DTOCycle
                    => await db.GetDomainWithId(DTOCycle.Id, DomainType.Cycle),
                DTODay DTODay
                    => await db.GetDomainWithId(DTODay.Id, DomainType.Day),
                DTOExercise DTOExercise
                    => await db.GetDomainWithId(DTOExercise.Id, DomainType.Exercise),
                _ => throw new NotImplementedException($"Неожиданный DTODomain: {DTODomain.GetType().Name}")
            };
        }

        internal static async Task<IDomain> GetDomainWithId(this EntityContext db, int id, DomainType domainType)
            => await db.GetDomainWithId(id, domainType.ToString());
        
        internal static async Task<IDomain> GetDomainWithId(this EntityContext db, int id, string domainType)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentException.ThrowIfNullOrWhiteSpace(domainType);

            return domainType switch
            {
                CommonConsts.DomainsAndEntities.Cycle
                    => await db.Cycles.FindAsync(id) ?? throw new InvalidOperationException($"Not found cycle with ID = {id}"),
                CommonConsts.DomainsAndEntities.Day
                    => await db.Days.FindAsync(id) ?? throw new InvalidOperationException($"Not found day with ID = {id}"),
                CommonConsts.DomainsAndEntities.Exercise
                    => await db.Exercises.FindAsync(id) ?? throw new InvalidOperationException($"Not found Exercise with ID = {id}"),
                _ => throw new NotImplementedException($"Неожиданный domainTyped: {domainType}")
            };
        }

        private static IEntity ConvertDTOToEntity(IDTOByEntity DTOByEntity)
        {
            return DTOByEntity switch
            {
                DTOCycle DTOCycle
                    => EntityConverter.ToCycle(DTOCycle),
                DTODay DTODay
                    => EntityConverter.ToDay(DTODay),
                DTOExercise DTOExercise
                    => EntityConverter.ToExercise(DTOExercise),
                DTOResultExercise DTOResultExercise
                    => EntityConverter.ToResultExercise(DTOResultExercise),
                _ => throw new NotSupportedException($"Неподдерживаемый тип IDTOByEntity: {DTOByEntity.GetType().Name}")
            };
        }
    }
}