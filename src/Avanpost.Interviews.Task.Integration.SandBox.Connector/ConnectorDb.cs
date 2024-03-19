using Avanpost.Interviews.Task.Integration.Data.DbCommon.DbModels;
using Avanpost.Interviews.Task.Integration.Data.Models;
using Avanpost.Interviews.Task.Integration.Data.Models.Models;
using Avanpost.Interviews.Task.Integration.SandBox.Connector.Domain.Services;
using Avanpost.Interviews.Task.Integration.SandBox.Connector.Domain.ValueObjects;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector
{
    public class ConnectorDb : IConnector, IDisposable
    {
        private readonly PropertyMappingService _propertyMapper;
        private UserPropertyService _userPropertyService = null!;
        private PostgresUsersRepository _usersRepository = null!;

        public ConnectorDb()
        {
            _propertyMapper = new PropertyMappingService();
        }

        public ILogger Logger { get; set; } = null!;

        public void StartUp(string connectionString)
        {
            var usersRepository = new PostgresUsersRepository(connectionString);
            _usersRepository = usersRepository;
            _userPropertyService = new UserPropertyService(usersRepository);
        }

        public void CreateUser(UserToCreate userToCreate)
        {
            ActWithLogging(nameof(CreateUser), () =>
            {
                var user = _propertyMapper.Map<User>(userToCreate.Properties.Concat(new UserProperty[] { new UserProperty("login", userToCreate.Login) }));
                var security = new Sequrity()
                {
                    UserId = userToCreate.Login,
                    Password = userToCreate.HashPassword
                };

                _usersRepository.CreateUserAsync(user).GetAwaiter().GetResult();
                _usersRepository.CreateSecurityAsync(security).GetAwaiter().GetResult();
            });
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return GetResultWithLogging(nameof(GetAllProperties), _userPropertyService.GetAllProperties);
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            return GetResultWithLogging(nameof(GetUserProperties), () =>
            {
                return _userPropertyService.GetUserPropertiesAsync(userLogin).GetAwaiter().GetResult();
            });
        }

        public bool IsUserExists(string userLogin)
        {
            return GetResultWithLogging(nameof(IsUserExists), () =>
            {
                return _usersRepository.IsUserExistsAsync(userLogin).GetAwaiter().GetResult();
            });
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            ActWithLogging(nameof(UpdateUserProperties), () =>
            {
                _userPropertyService.UpdateUserPropertiesAsync(properties, userLogin).GetAwaiter().GetResult();
            });
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            return GetResultWithLogging(nameof(GetAllPermissions), () =>
            {
                return _usersRepository.GetAllPermissionsAsync().GetAwaiter().GetResult();
            });
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            ActWithLogging(nameof(AddUserPermissions), () =>
            {
                _usersRepository.AddUserPermissionsAsync(userLogin, rightIds).GetAwaiter().GetResult();
            });
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            ActWithLogging(nameof(RemoveUserPermissions), () =>
            {
                _usersRepository.RemoveUserPermissionsAsync(userLogin, rightIds).GetAwaiter().GetResult();
            });
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            return GetResultWithLogging(nameof(GetUserPermissions), () =>
            {
                return _usersRepository.GetUserPermissionsAsync(userLogin).GetAwaiter().GetResult();
            });
        }

        public void Dispose()
            => _usersRepository.Dispose();

        private void ActWithLogging(string name, Action action)
        {
            try
            {
                action.Invoke();
                PrintSuccessMessage(name);
            }
            catch (Exception exception)
            {
                PrintFailMessage(name, exception);
                throw;
            }
        }

        private TResult GetResultWithLogging<TResult>(string name, Func<TResult> action)
        {
            try
            {
                var result = action.Invoke();
                PrintSuccessMessage(name);
                return result;
            }
            catch (Exception exception)
            {
                PrintFailMessage(name, exception);
                throw;
            }
        }

        private void PrintSuccessMessage(string name)
            => Logger.Debug($"{name} completed successfully");

        private void PrintFailMessage(string name, Exception exception)
             => Logger.Error($"{name} failed with message: {exception.Message}");
    }
}