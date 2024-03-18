using Microsoft.EntityFrameworkCore;
using Avanpost.Interviews.Task.Integration.Data.DbCommon.DbModels;
using Avanpost.Interviews.Task.Integration.Data.DbCommon;
using Avanpost.Interviews.Task.Integration.Data.Models.Models;
using Avanpost.Interviews.Task.Integration.SandBox.Connector.Domain;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector
{
    internal sealed class PostgresUsersRepository : IUsersRepository, IDisposable
    {
        private const string ProviderName = "POSTGRE";
        private readonly DataContext _dataContext;

        public PostgresUsersRepository(string connectionString)
        {
            // Parse conn string
            var dbContextFactory = new DbContextFactory("Server=localhost;Port=5432;Database=testdb;Username=postgres;Password=1;");
            _dataContext = dbContextFactory.GetContext(ProviderName);
        }

        public async System.Threading.Tasks.Task CreateUserAsync(User user)
        {
            await _dataContext.Users.AddAsync(user);
            await _dataContext.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task CreateSecurityAsync(Sequrity security)
        {
            await _dataContext.Passwords.AddAsync(security);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            var requestRights = await _dataContext.RequestRights
                .Select(_ => new Permission(_.Id.ToString()!, _.Name, RightKind.RequestRight.ToString()))
                .ToArrayAsync();

            var iTRoles = await _dataContext.ITRoles
                .Select(_ => new Permission(_.Id.ToString()!, _.Name, RightKind.ITRole.ToString()))
                .ToArrayAsync();

            return requestRights.Concat(iTRoles);
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userLogin)
        {
            var userRequestRightPermissions = await _dataContext.UserRequestRights
                .Where(_ => _.UserId == userLogin)
                .Join(_dataContext.RequestRights,
                    userRequestRight => userRequestRight.RightId,
                    requestRight => requestRight.Id,
                    (userRequestRight, requestRight) => new Right(RightKind.RequestRight, requestRight.Id.Value).ToString())
                .ToArrayAsync();

            var userITRolePermissions = await _dataContext.UserITRoles
                .Where(_ => _.UserId == userLogin)
                .Join(_dataContext.ITRoles,
                    userITRole => userITRole.RoleId,
                    iTRole => iTRole.Id,
                    (userITRole, iTRole) => new Right(RightKind.ITRole, iTRole.Id.Value).ToString())
                .ToArrayAsync();

            return userRequestRightPermissions.Concat(userITRolePermissions);
        }

        public async System.Threading.Tasks.Task RemoveUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds)
        {
            (List<int> iTRoleIds, List<int> requestRightIds) = InitializeIds();

            var userITRolesToRemove = await _dataContext.UserITRoles
                .Where(_ => _.UserId == userLogin && iTRoleIds.Contains(_.RoleId))
                .ToArrayAsync();
            _dataContext.UserITRoles.RemoveRange(userITRolesToRemove);

            var userRequestRightsToRemove = await _dataContext.UserRequestRights
                .Where(_ => _.UserId == userLogin && requestRightIds.Contains(_.RightId))
                .ToArrayAsync();
            _dataContext.UserRequestRights.RemoveRange(userRequestRightsToRemove);

            await _dataContext.SaveChangesAsync();

            (List<int> iTRoleIds, List<int> requestRightIds) InitializeIds()
            {
                var iTRoleIds = new List<int>();
                var requestRightIds = new List<int>();

                foreach (var right in rightIds.Select(Right.Parse))
                {
                    var id = right.Id;

                    if (right.Kind is RightKind.ITRole)
                        iTRoleIds.Add(id);
                    else
                        requestRightIds.Add(id);
                }

                return (iTRoleIds, requestRightIds);
            }
        }

        public async System.Threading.Tasks.Task AddUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds)
        {
            var rights = rightIds.Select(Right.Parse);

            foreach(var right in rights)
            {
                // TODO: make polymorphic
                if (right.Kind is RightKind.ITRole)
                    await _dataContext.UserITRoles.AddAsync(new UserITRole() { RoleId = right.Id, UserId = userLogin });
                else
                    await _dataContext.UserRequestRights.AddAsync(new UserRequestRight() { RightId = right.Id, UserId = userLogin });
            }

            await _dataContext.SaveChangesAsync();
        }

        public async Task<bool> IsUserExistsAsync(string userLogin)
            => await _dataContext.Users.AnyAsync(_ => _.Login == userLogin);

        public async System.Threading.Tasks.Task UpdateUserAsync(string userLogin, Action<User> userUpdater)
        {
            var user = await _dataContext.Users.FirstAsync(_ => _.Login == userLogin);
            userUpdater(user);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<User> GetUserAsync(string userLogin)
            => await _dataContext.Users.FirstAsync(user => user.Login == userLogin);

        public void Dispose()
            => _dataContext.Dispose();
    }

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