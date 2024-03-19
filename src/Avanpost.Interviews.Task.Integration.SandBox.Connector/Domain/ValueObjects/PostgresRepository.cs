using Microsoft.EntityFrameworkCore;
using Avanpost.Interviews.Task.Integration.Data.DbCommon.DbModels;
using Avanpost.Interviews.Task.Integration.Data.DbCommon;
using Avanpost.Interviews.Task.Integration.Data.Models.Models;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector.Domain.ValueObjects
{
    internal sealed class PostgresUsersRepository : IUsersRepository, IDisposable
    {
        private const string ProviderName = "POSTGRE";
        private readonly DataContext _dataContext;

        public PostgresUsersRepository(string rawConnectionString)
        {
            var connectionString = ExtractConnectionString(rawConnectionString);

            var dbContextFactory = new DbContextFactory(connectionString);
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
            var groups = rightIds.Select(Right.Parse).GroupBy(_ => _.Kind);

            foreach (var group in groups)
            {
                var ids = group.Select(_ => _.Id).ToHashSet();

                if (group.Key == RightKind.ITRole)
                {
                    var userITRolesToRemove = await _dataContext.UserITRoles
                        .Where(_ => _.UserId == userLogin && ids.Contains(_.RoleId))
                        .ToArrayAsync();

                    _dataContext.UserITRoles.RemoveRange(userITRolesToRemove);
                }
                else
                {
                    var userRequestRightsToRemove = await _dataContext.UserRequestRights
                       .Where(_ => _.UserId == userLogin && ids.Contains(_.RightId))
                       .ToArrayAsync();

                    _dataContext.UserRequestRights.RemoveRange(userRequestRightsToRemove);
                }
            }

            await _dataContext.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task AddUserPermissionsAsync(string userLogin, IEnumerable<string> rightIds)
        {
            var groups = rightIds.Select(Right.Parse).GroupBy(_ => _.Kind);

            foreach(var group in groups)
            {
                if(group.Key == RightKind.ITRole)
                {
                    var users = group.Select(_ => new UserITRole()
                    {
                        UserId = userLogin,
                        RoleId = _.Id
                    });

                    await _dataContext.UserITRoles.AddRangeAsync(users);
                }
                else
                {
                    var users = group.Select(_ => new UserRequestRight()
                    {
                        UserId = userLogin,
                        RightId = _.Id
                    });

                    await _dataContext.UserRequestRights.AddRangeAsync(users);
                }
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

        private string ExtractConnectionString(string rawConnectionString)
        {
            const string connectionStringPrefix = "ConnectionString='";
            const char connectionStringEnding = '\'';

            var connectionStringSpan = rawConnectionString.AsSpan();
            var startIndex = connectionStringSpan.IndexOf(connectionStringPrefix);
            var cuttedPartSpan = connectionStringSpan.Slice(startIndex + connectionStringPrefix.Length);
            var endIndex = cuttedPartSpan.IndexOf(connectionStringEnding);
            var connectionString = cuttedPartSpan.Slice(0, endIndex).ToString();

            return connectionString;
        }
    }
}