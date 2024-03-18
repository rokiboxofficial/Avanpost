using Avanpost.Interviews.Task.Integration.Data.DbCommon.DbModels;
using Avanpost.Interviews.Task.Integration.Data.DbCommon;

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

        public void Dispose()
            => _dataContext.Dispose();
    }

    internal interface IUsersRepository
    {
        public System.Threading.Tasks.Task CreateUserAsync(User user);

        public System.Threading.Tasks.Task CreateSecurityAsync(Sequrity security);
    }
}