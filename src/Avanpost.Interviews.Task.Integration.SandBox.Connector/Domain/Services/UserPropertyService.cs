using Avanpost.Interviews.Task.Integration.Data.DbCommon.DbModels;
using Avanpost.Interviews.Task.Integration.Data.Models.Models;
using Avanpost.Interviews.Task.Integration.SandBox.Connector.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector.Domain.Services
{
    public sealed class UserPropertyService
    {
        private readonly Lazy<Dictionary<string, PropertyInfo>> _permittedPropertyInfoByColumnName;
        private readonly IUsersRepository _usersRepository;

        public UserPropertyService(IUsersRepository usersRepository)
        {
            _permittedPropertyInfoByColumnName = new(GetPermittedPropertyInfoByColumnName);
            _usersRepository = usersRepository;
        }

        public IEnumerable<Property> GetAllProperties()
            => _permittedPropertyInfoByColumnName.Value.Values.Select(_ => new Property(_.Name, _.PropertyType.Name));

        public async Task<IEnumerable<UserProperty>> GetUserPropertiesAsync(string userLogin)
        {
            var user = await _usersRepository.GetUserAsync(userLogin);

            return _permittedPropertyInfoByColumnName.Value
                .Select(_ => new UserProperty(_.Key, _.Value.GetValue(user)!.ToString()!));
        }

        public async System.Threading.Tasks.Task UpdateUserPropertiesAsync(IEnumerable<UserProperty> properties, string userLogin)
        {
            await _usersRepository.UpdateUserAsync(userLogin, (user) =>
            {
                foreach (var property in properties)
                {
                    var columnName = property.Name;
                    var value = property.Value;

                    if (!_permittedPropertyInfoByColumnName.Value.ContainsKey(columnName))
                        continue;

                    var propertyInfo = _permittedPropertyInfoByColumnName.Value[columnName];
                    propertyInfo.SetValue(user, value);
                }
            });
        }

        private Dictionary<string, PropertyInfo> GetPermittedPropertyInfoByColumnName()
        {
            var type = typeof(User);
            var propertyInfos = type.GetProperties();
            var permittedPropertyInfoByColumnName = propertyInfos
                .Where(_ => Attribute.IsDefined(_, typeof(ColumnAttribute)))
                .Where(_ => !Attribute.IsDefined(_, typeof(KeyAttribute)))
                .ToDictionary(_ => _.GetCustomAttribute<ColumnAttribute>()!.Name!, _ => _);

            return permittedPropertyInfoByColumnName;
        }
    }
}
