using Baitkm.DAL.Configurations.Helpers;
using Baitkm.DAL.Context;
using Baitkm.DTO.ViewModels.Bases;
using Baitkm.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace Baitkm.DAL.Services
{
    public static class HelperService
    {
        public static IEnumerable<T> Execute<T>(this IBaitkmDbContext context, string query, params SqlParameter[] sqlParams) where T : class, IStoredProcedureResponse, new()
        {
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 60;
                if (sqlParams != null)
                {
                    command.Parameters.AddRange(sqlParams);
                }
                using (context.Database.OpenConnectionAsync())
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Any())
                            yield break;
                        var mapper = new DataReaderMapper<T>(reader);
                        do
                        {
                            yield return mapper.MapFrom(reader);
                        } while (reader.Read());
                    }
                }
            }
        }

        public static List<List<TEntity>> SplitList<TEntity>(this List<TEntity> tokens, int range) where TEntity : EntityBase
        {
            var list = new List<List<TEntity>>();
            for (var i = 0; i < tokens.Count; i += range)
            {
                list.Add(tokens.GetRange(i, Math.Min(range, tokens.Count - i)));
            }
            return list;
        }

        public static Expression<Func<TEntity, bool>> Or<TEntity>(this Expression<Func<TEntity, bool>> firstExpr, Expression<Func<TEntity, bool>> secondExpr)
        {
            var invokedExpr = Expression.Invoke(secondExpr, firstExpr.Parameters);
            return Expression.Lambda<Func<TEntity, bool>>(Expression.Or(firstExpr.Body, invokedExpr), firstExpr.Parameters);
        }

        public static Expression<Func<TEntity, bool>> And<TEntity>(this Expression<Func<TEntity, bool>> firstExpr, Expression<Func<TEntity, bool>> secondExpr)
        {
            var invokedExpr = Expression.Invoke(secondExpr, firstExpr.Parameters);
            return Expression.Lambda<Func<TEntity, bool>>(Expression.And(firstExpr.Body, invokedExpr), firstExpr.Parameters);
        }

        public static Expression<Func<TEntity, bool>> OrElse<TEntity>(this Expression<Func<TEntity, bool>> firstExpr, Expression<Func<TEntity, bool>> secondExpr)
        {
            var invokedExpr = Expression.Invoke(secondExpr, firstExpr.Parameters);
            return Expression.Lambda<Func<TEntity, bool>>(Expression.OrElse(firstExpr.Body, invokedExpr), firstExpr.Parameters);
        }
    }
}
