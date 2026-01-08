using WorkoutStorageBot.BusinessLogic.Consts;
using WorkoutStorageBot.BusinessLogic.Enums;
using WorkoutStorageBot.Helpers.Converters;
using WorkoutStorageBot.Model.AppContext;
using WorkoutStorageBot.Model.DTO.BusinessLogic;
using WorkoutStorageBot.Model.Entities.BusinessLogic;
using WorkoutStorageBot.Model.Interfaces;

namespace WorkoutStorageBot.Extenions
{
    internal static class DomainExtensions
    {
        internal static void AddEntity(this EntityContext db, IDTOByEntity DTOByEntity, bool isNeedSave = true)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(DTOByEntity);

            IEntity entity = ConvertDTOToEntity(DTOByEntity);

            db.Add(entity);

            if (isNeedSave)
            {
                db.SaveChanges();

                DTOByEntity.Id = entity.Id;
            }
        }

        internal static void AddEntities(this EntityContext db, IEnumerable<DTOExercise> DTOExercises, bool isNeedSave = true)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(DTOExercises);

            var pairs = DTOExercises.Select(x => new { DTO = x, Entity = EntityConverter.ToExercise(x) });

            db.Exercises.AddRange(pairs.Select(p => p.Entity));

            if (isNeedSave)
            {
                db.SaveChanges();

                foreach (var pair in pairs)
                {
                    pair.DTO.Id = pair.Entity.Id;
                }
            }
        }

        internal static void AddEntities(this EntityContext db, IEnumerable<DTOResultExercise> DTOResultExercise, bool isNeedSave = true)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(DTOResultExercise);

            var pairs = DTOResultExercise.Select(x => new { DTO = x, Entity = EntityConverter.ToResultExercise(x) });

            db.ResultsExercises.AddRange(pairs.Select(p => p.Entity));

            if (isNeedSave)
            {
                db.SaveChanges();

                foreach (var pair in pairs)
                {
                    pair.DTO.Id = pair.Entity.Id;
                }
            }
        }

        internal static void UpdateEntity(this EntityContext db, IDTOByEntity DTOByEntity, bool isNeedSave = true)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(DTOByEntity);

            switch (DTOByEntity)
            {
                case DTOCycle DTOCycle:
                    Cycle cycle = db.Cycles.First(x => x.Id == DTOCycle.Id);

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
                    Day day = db.Days.First(x => x.Id == DTODay.Id);

                    if (day.Name != DTODay.Name)
                        day.Name = DTODay.Name;

                    if (day.CycleId != DTODay.CycleId)
                        day.CycleId = DTODay.CycleId;

                    if (day.IsArchive != DTODay.IsArchive)
                        day.IsArchive = DTODay.IsArchive;

                    break;
                case DTOExercise DTOExercise:
                    Exercise exercise = db.Exercises.First(x => x.Id == DTOExercise.Id);

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

                    ResultExercise resultExercise = db.ResultsExercises.First(x => x.Id == DTOResultExercise.Id);

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
                db.SaveChanges();
        }

        internal static void UpdateEntities(this EntityContext db, ICollection<IDTOByEntity> DTOByEntities, bool isNeedSave = true)
        {
            ArgumentNullException.ThrowIfNull(DTOByEntities);

            foreach (IDTOByEntity DTOByEntity in DTOByEntities)
            {
                UpdateEntity(db, DTOByEntity, false);
            }

            if (isNeedSave)
                db.SaveChanges();
        }

        internal static void RemoveEntity(this EntityContext db, IDTOByEntity DTOByEntity, bool isNeedSave = true)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(DTOByEntity);

            IEntity entity = ConvertDTOToEntity(DTOByEntity);

            db.Remove(entity);

            if (isNeedSave)
                db.SaveChanges();
        }

        internal static void RemoveEntities(this EntityContext db, ICollection<IDTOByEntity> DTOByEntities, bool isNeedSave = true)
        {
            ArgumentNullException.ThrowIfNull(DTOByEntities);

            foreach (IDTOByEntity entity in DTOByEntities)
            {
                RemoveEntity(db, entity, false);
            }

            if (isNeedSave)
                db.SaveChanges();
        }

        internal static IDomain GetDomainByDTO(this EntityContext db, IDTODomain DTODomain)
        {
            ArgumentNullException.ThrowIfNull(DTODomain);

            return DTODomain switch
            {
                DTOCycle DTOCycle
                    => GetDomainWithId(db, DTOCycle.Id, DomainType.Cycle),
                DTODay DTODay
                    => GetDomainWithId(db, DTODay.Id, DomainType.Day),
                DTOExercise DTOExercise
                    => GetDomainWithId(db, DTOExercise.Id, DomainType.Exercise),
                _ => throw new NotImplementedException($"Неожиданный DTODomain: {DTODomain.GetType().Name}")
            };
        }

        internal static IDomain GetDomainWithId(this EntityContext db, int id, DomainType domainType)
            => GetDomainWithId(db, id, domainType.ToString());
        
        internal static IDomain GetDomainWithId(this EntityContext db, int id, string domainType)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(domainType);

            return domainType switch
            {
                CommonConsts.DomainsAndEntities.Cycle
                    => db.Cycles.Find(id) ?? throw new InvalidOperationException($"Not found cycle with ID = {id}"),
                CommonConsts.DomainsAndEntities.Day
                    => db.Days.Find(id) ?? throw new InvalidOperationException($"Not found day with ID = {id}"),
                CommonConsts.DomainsAndEntities.Exercise
                    => db.Exercises.Find(id) ?? throw new InvalidOperationException($"Not found Exercise with ID = {id}"),
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