using Avanpost.Interviews.Task.Integration.Data.Models.Models;
using Avanpost.Interviews.Task.Integration.SandBox.Connector.Extensions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector
{
    internal sealed class PropertyMapper
    {
        public TTarget Map<TTarget>(IEnumerable<UserProperty> properties) where TTarget : new()
        {
            var propertyValueByName = properties.ToDictionary(property => property.Name, property => property.Value);

            var target = new TTarget();
            var type = typeof(TTarget);

            _ = type.GetProperties()
                .Where(_ => Attribute.IsDefined(_, typeof(ColumnAttribute)))
                .Select(GetPropertyInfoAndColumnNamePair)
                .Act(SetPropertyIfRequested);

            return target;

            void SetPropertyIfRequested((PropertyInfo, string) propertyInfoAndColumnNamePair)
            {
                var propertyInfo = propertyInfoAndColumnNamePair.Item1;
                var columnName = propertyInfoAndColumnNamePair.Item2;

                if (propertyValueByName.ContainsKey(columnName))
                {
                    var value = propertyValueByName[columnName];

                    if (propertyInfo.PropertyType == typeof(bool))
                        propertyInfo.SetValue(target, bool.Parse(value));
                    else
                        propertyInfo.SetValue(target, value);
                }
                else
                {
                    if (propertyInfo.PropertyType == typeof(string))
                        propertyInfo.SetValue(target, "");
                }
            }

            (PropertyInfo, string) GetPropertyInfoAndColumnNamePair(PropertyInfo propertyInfo)
            {
                var columnName = propertyInfo.GetCustomAttribute<ColumnAttribute>()!.Name!;

                return (propertyInfo, columnName);
            }
        }
    }
}