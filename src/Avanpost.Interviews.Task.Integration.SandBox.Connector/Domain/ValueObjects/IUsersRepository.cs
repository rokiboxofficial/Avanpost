using Avanpost.Interviews.Task.Integration.Data.DbCommon.DbModels;
using Avanpost.Interviews.Task.Integration.Data.Models.Models;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector.Domain.ValueObjects
{
    public interface IUsersRepository
    {
        public System.Threading.Tasks.Task CreateUserAsync(User user);

        public System.Threading.Tasks.Task CreateSecurityAsync(Sequrity security);

        public Task<IEnumerable<Permission>> GetAllPermissionsAsync();

        public Task<IEnumerable<string>> GetUserPermissionsAsync(string userLogin);

        public System.Threading.Tasks.Task RemoveUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds);

        public System.Threading.Tasks.Task AddUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds);

        public Task<bool> IsUserExistsAsync(string userLogin);

        public System.Threading.Tasks.Task UpdateUserAsync(string userLogin, Action<User> userUpdater);

        public Task<User> GetUserAsync(string userLogin);
    }
}