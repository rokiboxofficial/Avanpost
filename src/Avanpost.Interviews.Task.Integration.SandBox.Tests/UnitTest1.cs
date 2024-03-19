using Avanpost.Interviews.Task.Integration.Data.DbCommon;
using Avanpost.Interviews.Task.Integration.Data.Models;
using Avanpost.Interviews.Task.Integration.Data.Models.Models;
using Avanpost.Interviews.Task.Integration.SandBox.Connector;
using Avanpost.Interviews.Task.Integration.SandBox.Connector.Domain.ValueObjects;

namespace Avanpost.Interviews.Task.Integration.SandBox.Tests
{
    public class UnitTest1
    {
        private const string CurrentProvider = "POSTGRE";
        private static string mssqlConnectionString = "";
        private static string postgreConnectionString = "Server=localhost;Port=5432;Database=testdb;Username=postgres;Password=1;";
        private static Dictionary<string, string> connectorsCS = new Dictionary<string, string>
        {
            { "MSSQL",$"ConnectionString='{mssqlConnectionString}';Provider='SqlServer.2019';SchemaName='AvanpostIntegrationTestTaskSchema';"},
            { "POSTGRE", $"ConnectionString='{postgreConnectionString}';Provider='PostgreSQL.9.5';SchemaName='AvanpostIntegrationTestTaskSchema';"}
        };
        private static Dictionary<string, string> dataBasesCS = new Dictionary<string, string>
        {
            { "MSSQL",mssqlConnectionString},
            { "POSTGRE", postgreConnectionString}
        };

        public DataManager Init(string providerName)
        {
            var factory = new DbContextFactory(dataBasesCS[providerName]);
            var dataSetter = new DataManager(factory, providerName);
            dataSetter.PrepareDbForTest();
            return dataSetter;
        }

        public IConnector GetConnector(string provider)
        {
            IConnector connector = new ConnectorDb();
            connector.StartUp(connectorsCS[provider]);
            connector.Logger = new FileLogger($"{DateTime.Now.ToString("yyyy-M-dd--HH-mm-ss")}connector{provider}.Log", $"{DateTime.Now}connector{provider}");
            return connector;
        }

        [Theory]
        [InlineData(CurrentProvider)]
        public void CreateUser(string provider)
        {
            // Arrange.
            const string testUserLogin = "testUserToCreate";
            const string testUserHashPassword = "testPassword";

            var dataSetter = Init(provider);
            var connector = GetConnector(provider);

            // Act.
            connector.CreateUser(new UserToCreate(testUserLogin, testUserHashPassword) { Properties = new UserProperty[] { new UserProperty("isLead", "false") } });

            // Assert.
            Assert.NotNull(dataSetter.GetUser(testUserLogin));
            Assert.Equal(testUserHashPassword, dataSetter.GetUserPassword(testUserLogin));
        }

        [Theory]
        [InlineData(CurrentProvider)]
        public void GetAllProperties(string provider)
        {
            // Arrange.
            Init(provider);
            var connector = GetConnector(provider);

            // Act.
            var propInfos = connector.GetAllProperties();

            // Assert.
            Assert.Equal(DefaultData.PropsCount, propInfos.Count());
        }

        [Theory]
        [InlineData(CurrentProvider)]
        public void GetUserProperties(string provider)
        {
            // Arrange.
            Init(provider);
            var connector = GetConnector(provider);

            // Act.
            var userInfo = connector.GetUserProperties(DefaultData.MasterUserLogin);

            // Assert.
            Assert.NotNull(userInfo);
            Assert.Equal(DefaultData.PropsCount, userInfo.Count());
            Assert.True(userInfo.FirstOrDefault(_ => _.Value.Equals(DefaultData.MasterUser.TelephoneNumber)) != null);
        }

        [Theory]
        [InlineData(CurrentProvider)]
        public void IsUserExists(string provider)
        {
            // Arrange.
            Init(provider);
            var connector = GetConnector(provider);

            // Act, Assert.
            Assert.True(connector.IsUserExists(DefaultData.MasterUserLogin));
            Assert.False(connector.IsUserExists(TestData.NotExistingUserLogin));
        }

        [Theory]
        [InlineData(CurrentProvider)]
        public void UpdateUserProperties(string provider)
        {
            // Arrange.
            var dataSetter = Init(provider);
            var connector = GetConnector(provider);
            var userInfo = connector.GetUserProperties(DefaultData.MasterUserLogin);
            var propertyName = connector.GetUserProperties(DefaultData.MasterUserLogin).First(_ => _.Value.Equals(DefaultData.MasterUser.TelephoneNumber)).Name;
            var propsToUpdate = new UserProperty[]
            {
                new UserProperty(propertyName,TestData.NewPhoneValueForMasterUser)
            };

            // Act.
            connector.UpdateUserProperties(propsToUpdate, DefaultData.MasterUserLogin);

            // Assert.
            Assert.Equal(TestData.NewPhoneValueForMasterUser, dataSetter.GetUser(DefaultData.MasterUserLogin).TelephoneNumber);
        }

        [Theory]
        [InlineData(CurrentProvider)]
        public void GetAllPermissions(string provider)
        {
            // Arrange.
            Init(provider);
            var connector = GetConnector(provider);

            // Act.
            var permissions = connector.GetAllPermissions();

            // Assert.
            Assert.NotNull(permissions);
            Assert.Equal(DefaultData.RequestRights.Length + DefaultData.ITRoles.Length, permissions.Count());
        }

        [Theory]
        [InlineData(CurrentProvider)]
        public void AddUserPermissions(string provider)
        {
            // Arrange.
            var dataSetter = Init(provider);
            var connector = GetConnector(provider);
            var RoleId = $"{Right.ITRoleRightGroupName}{Right.Delimeter}{dataSetter.GetITRoleId()}";

            // Act.
            connector.AddUserPermissions(
                DefaultData.MasterUserLogin,
                new [] { RoleId });

            // Assert.
            Assert.True(dataSetter.MasterUserHasITRole(dataSetter.GetITRoleId().ToString()));
            Assert.True(dataSetter.MasterUserHasRequestRight(dataSetter.GetRequestRightId(DefaultData.RequestRights[DefaultData.MasterUserRequestRights.First()].Name).ToString()));
        }

        [Theory]
        [InlineData(CurrentProvider)]
        public void RemoveUserPermissions(string provider)
        {
            // Arrange.
            var dataSetter = Init(provider);
            var connector = GetConnector(provider);
            var requestRightIdToDrop = $"{Right.RequestRightGroupName}{Right.Delimeter}{dataSetter.GetRequestRightId(DefaultData.RequestRights[DefaultData.MasterUserRequestRights.First()].Name)}";
            
            // Act.
            connector.RemoveUserPermissions(
                DefaultData.MasterUserLogin,
                new [] { requestRightIdToDrop });

            // Assert.
            Assert.False(dataSetter.MasterUserHasITRole(dataSetter.GetITRoleId().ToString()));
            Assert.False(dataSetter.MasterUserHasRequestRight(dataSetter.GetRequestRightId(DefaultData.RequestRights[DefaultData.MasterUserRequestRights.First()].Name).ToString()));
        }

        [Theory]
        [InlineData(CurrentProvider)]
        public void GetUserPermissions(string provider)
        {
            // Arrange.
            Init(provider);
            var connector = GetConnector(provider);

            // Act.
            var permissions = connector.GetUserPermissions(DefaultData.MasterUserLogin);

            // Assert.
            Assert.Equal(DefaultData.MasterUserRequestRights.Length, permissions.Count());
        }
    }
}