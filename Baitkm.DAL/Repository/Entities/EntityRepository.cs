using Baitkm.DAL.Context;
using Baitkm.DAL.Services;
using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Entities.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Baitkm.DAL.Repository.Entities
{
    public class EntityRepository : IEntityRepository
    {
        //private readonly IBaitkmDbContext _context;
        //public EntityRepository(IBaitkmDbContext context)
        //{
        //    _context = context;
        //}
        private readonly BaitkmDbContext _context;
        public EntityRepository(BaitkmDbContext context)
        {
            _context = context;
        }


        public TEntity Create<TEntity>(TEntity entity) where TEntity : EntityBase
        {
            entity.CreatedDt = DateTime.UtcNow;
            entity.UpdatedDt = DateTime.UtcNow;

            _context.Add(entity);
            return entity;
        }

        public async Task CreateAsync<TEntity>(TEntity entity) where TEntity : EntityBase
        {
            entity.CreatedDt = DateTime.UtcNow;
            entity.UpdatedDt = DateTime.UtcNow;

            await _context.AddAsync(entity);
        }

        public async Task CreateRangeAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : EntityBase
        {
            foreach (var entity in entities)
            {
                entity.CreatedDt = DateTime.UtcNow;
                entity.UpdatedDt = DateTime.UtcNow;
            }
            await _context.AddRangeAsync(entities);
        }

        public void Update<TEntity>(TEntity entity) where TEntity : EntityBase
        {
            entity.UpdatedDt = DateTime.UtcNow;
            _context.Update(entity);
        }

        public void UpdateRange<TEntity>(ICollection<TEntity> entities) where TEntity : EntityBase
        {
            _context.UpdateRange(entities);
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : EntityBase
        {
            entity.IsDeleted = true;
            Update(entity);
        }

        public void RemoveRange<TEntity>(ICollection<TEntity> entities) where TEntity : EntityBase
        {
            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                entity.UpdatedDt = DateTime.UtcNow;
            }
            UpdateRange(entities);
        }

        public void RemoveRangeById<TEntity>(IEnumerable<int> ids) where TEntity : EntityBase
        {
            foreach (var id in ids)
            {
                TEntity entity = _context.Find<TEntity>(id);
                Remove(entity);
            }
        }

        public void HardDelete<TEntity>(TEntity entity) where TEntity : EntityBase
        {
            _context.Remove(entity);
        }

        public void HardDeleteRange<TEntity>(ICollection<TEntity> entities) where TEntity : EntityBase
        {
            foreach (var entity in entities)
            {
                _context.Remove(entity);
            }
        }

        public TEntity FindPerson<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : EntityBase
        {
            return _context.Set<TEntity>().Where(expression).Where(e => !e.IsDeleted).FirstOrDefault();
        }

        public TEntity FindPersonAsNoTracking<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : EntityBase
        {
            return _context.Set<TEntity>().Where(expression).Where(e => !e.IsDeleted).AsNoTracking().FirstOrDefault();
        }

        public IQueryable<TEntity> Filter<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : EntityBase
        {
            return _context.Set<TEntity>().Where(expression).Where(e => !e.IsDeleted);
        }

        public IQueryable<TEntity> FilterAsNoTracking<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : EntityBase
        {
            return _context.Set<TEntity>().Where(expression).Where(e => !e.IsDeleted).AsNoTracking();
        }

        public IQueryable<TEntity> GetAll<TEntity>() where TEntity : EntityBase
        {
            return _context.Set<TEntity>().Where(e => !e.IsDeleted);
        }

        public TEntity GetById<TEntity>(int id) where TEntity : EntityBase
        {
            TEntity entity = _context.Set<TEntity>().Find(id);
            if (entity == null || entity.IsDeleted)
                throw new InvalidOperationException("Invalid id");
            return entity;
        }

        public List<TView> Execute<TView>(string name, params KeyValuePair<string, object>[] parameters) where TView : class, IStoredProcedureResponse, new()
        {
            var list = new List<SqlParameter>();
            if (parameters != null)
                foreach (var keyValuePair in parameters)
                {
                    var param = new SqlParameter
                    {
                        Value = keyValuePair.Value,
                        ParameterName = keyValuePair.Key
                    };
                    if (param.DbType == DbType.Decimal)
                    {
                        param.Precision = 18;
                        param.Scale = 6;
                    }
                    list.Add(param);
                }
            return _context.Execute<TView>(name, list.ToArray()).ToList();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}