using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Baitkm.DAL.Repository.Entities
{
    public interface IEntityRepository : IDisposable
    {
        TEntity Create<TEntity>(TEntity entity) where TEntity : EntityBase;
        Task CreateAsync<TEntity>(TEntity entity) where TEntity : EntityBase;
        Task CreateRangeAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : EntityBase;
        void Update<TEntity>(TEntity entity) where TEntity : EntityBase;
        void UpdateRange<TEntity>(ICollection<TEntity> entities) where TEntity : EntityBase;
        void Remove<TEntity>(TEntity entity) where TEntity : EntityBase;
        void RemoveRange<TEntity>(ICollection<TEntity> entities) where TEntity : EntityBase;
        void RemoveRangeById<TEntity>(IEnumerable<int> ids) where TEntity : EntityBase;
        void HardDelete<TEntity>(TEntity entity) where TEntity : EntityBase;
        void HardDeleteRange<TEntity>(ICollection<TEntity> entities) where TEntity : EntityBase;
        IQueryable<TEntity> FilterAsNoTracking<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : EntityBase;
        IQueryable<TEntity> Filter<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : EntityBase;
        IQueryable<TEntity> GetAll<TEntity>() where TEntity : EntityBase;
        TEntity GetById<TEntity>(int id) where TEntity : EntityBase;

        List<TView> Execute<TView>(string name, params KeyValuePair<string, object>[] parameters) where TView : class, IStoredProcedureResponse, new();



        Task SaveChangesAsync();
        void SaveChanges();
    }
}