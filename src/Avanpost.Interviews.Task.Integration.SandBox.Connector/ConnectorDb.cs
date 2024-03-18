using Avanpost.Interviews.Task.Integration.Data.DbCommon.DbModels;
using Avanpost.Interviews.Task.Integration.Data.Models;
using Avanpost.Interviews.Task.Integration.Data.Models.Models;
using Avanpost.Interviews.Task.Integration.SandBox.Connector.Domain.Services;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector
{
    public class ConnectorDb : IConnector
    {
        private readonly PropertyMapper _propertyMapper;
        private UserPropertyService _userPropertyService;
        private IUsersRepository _usersRepository = null!;

        public ConnectorDb()
        {
            _propertyMapper = new PropertyMapper();
        }

        public void StartUp(string connectionString)
        {
            // dispose
            var usersRepository = new PostgresUsersRepository(connectionString);
            _usersRepository = usersRepository;
            _userPropertyService = new UserPropertyService(usersRepository);
        }

        public void CreateUser(UserToCreate userToCreate)
        {
            var user = _propertyMapper.Map<User>(userToCreate.Properties.Concat(new UserProperty[] { new UserProperty("login", userToCreate.Login) }));
            var security = new Sequrity()
            {
                UserId = userToCreate.Login,
                Password = userToCreate.HashPassword
            };

            _usersRepository.CreateUserAsync(user).GetAwaiter().GetResult();
            _usersRepository.CreateSecurityAsync(security).GetAwaiter().GetResult();
        }

        public IEnumerable<Property> GetAllProperties()
            => _userPropertyService.GetAllProperties();

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
            => _userPropertyService.GetUserPropertiesAsync(userLogin).GetAwaiter().GetResult();

        public bool IsUserExists(string userLogin)
            => _usersRepository.IsUserExistsAsync(userLogin).GetAwaiter().GetResult();

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
            => _userPropertyService.UpdateUserPropertiesAsync(properties, userLogin).GetAwaiter().GetResult();

        public IEnumerable<Permission> GetAllPermissions()
            => _usersRepository.GetAllPermissionsAsync().GetAwaiter().GetResult();

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
            => _usersRepository.AddUserPermissionsAsync(userLogin, rightIds).GetAwaiter().GetResult();

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
            => _usersRepository.RemoveUserPermissionsAsync(userLogin, rightIds).GetAwaiter().GetResult();

        public IEnumerable<string> GetUserPermissions(string userLogin)
            => _usersRepository.GetUserPermissionsAsync(userLogin).GetAwaiter().GetResult();

        public ILogger Logger { get; set; } = null!;
    }
}