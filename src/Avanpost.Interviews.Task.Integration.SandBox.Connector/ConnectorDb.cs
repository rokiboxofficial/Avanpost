using Avanpost.Interviews.Task.Integration.Data.DbCommon.DbModels;
using Avanpost.Interviews.Task.Integration.Data.Models;
using Avanpost.Interviews.Task.Integration.Data.Models.Models;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector
{
    public class ConnectorDb : IConnector
    {
        private readonly PropertyMapper _propertyMapper;
        private IUsersRepository _usersRepository = null!;

        public ConnectorDb()
        {
            _propertyMapper = new PropertyMapper();
        }

        public void StartUp(string connectionString)
        {
            _usersRepository = new PostgresUsersRepository(connectionString);
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
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            throw new NotImplementedException();
        }

        public bool IsUserExists(string userLogin)
        {
            throw new NotImplementedException();
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            throw new NotImplementedException();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            throw new NotImplementedException();
        }

        public ILogger Logger { get; set; } = null!;
    }
}