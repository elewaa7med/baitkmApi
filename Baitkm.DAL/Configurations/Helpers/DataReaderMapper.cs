using Baitkm.DAL.Configurations.CustomAbstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Baitkm.DAL.Configurations.Helpers
{
    public class DataReaderMapper<T>
    {
        private readonly bool _isPrimitiveish;
        private readonly Dictionary<int, Either<FieldInfo, PropertyInfo>> _mappings;

        private class JoinInfo
        {
            public Either<FieldInfo, PropertyInfo> Info;
            public string Name;
        }

        static Dictionary<int, Either<FieldInfo, PropertyInfo>> Mappings(IDataReader reader)
        {
            var columns = Enumerable.Range(0, reader.FieldCount);
            var fieldsAndProps = typeof(T).FieldsAndProps()
                .Select(fp => fp.Match(
                    f => new JoinInfo { Info = f, Name = f.Name },
                    p => new JoinInfo { Info = p, Name = p.Name }
                ));
            var joined = columns
                  .Join(fieldsAndProps, reader.GetName, x => x.Name, (index, x) => new
                  {
                      index,
                      info = x.Info
                  }, StringComparer.InvariantCultureIgnoreCase).ToList();
            if (columns.Count() != joined.Count || fieldsAndProps.Count() != joined.Count)
            {
                throw new ApplicationException("Expected to map every column in the result. " +
                    $"Instead, {columns.Count()} columns and {fieldsAndProps.Count()} fields produced only {joined.Count} matches. " +
                    "Hint: be sure all your columns have _names_, and the names match up.");
            }
            return joined.ToDictionary(x => x.index, x => x.info);
        }

        public DataReaderMapper(IDataReader reader)
        {
            var type = Nullable.GetUnderlyingType(typeof(T));
            _isPrimitiveish = typeof(T).IsPrimitive || type != null && type.IsPrimitive;
            if (!_isPrimitiveish)
            {
                _mappings = Mappings(reader);
            }
        }

        public T MapFrom(IDataRecord record)
        {
            if (_isPrimitiveish)
            {
                return (T)record.GetValue(0);
            }
            var element = Activator.CreateInstance<T>();
            foreach (var map in _mappings)
                map.Value.Match(
                    f => f.SetValue(element, ChangeType(record[map.Key], f.FieldType)),
                    p => p.SetValue(element, ChangeType(record[map.Key], p.PropertyType))
                );

            return element;
        }

        static object ChangeType(object value, Type targetType)
        {
            if (value == null || value == DBNull.Value)
                return null;

            return Convert.ChangeType(value, Nullable.GetUnderlyingType(targetType) ?? targetType);
        }
    }

    public static class FieldAndPropsExtension
    {
        public static IEnumerable<Either<FieldInfo, PropertyInfo>> FieldsAndProps(this Type T)
        {
            var lst = new List<Either<FieldInfo, PropertyInfo>>();
            lst.AddRange(
                T.GetFields()
                .Select(field => new Either<FieldInfo, PropertyInfo>.Left(field))
            );
            lst.AddRange(
                T.GetProperties()
                .Select(prop => new Either<FieldInfo, PropertyInfo>.Right(prop))
            );
            return lst;
        }
    }
}
