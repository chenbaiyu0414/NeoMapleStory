using MySql.Data.MySqlClient;


namespace NeoMapleStory.Core
{
    public class DbConnectionManager
    {
        private readonly MySqlConnectionStringBuilder _conStrBuilder;
        public static DbConnectionManager Instance { get; } = new DbConnectionManager();

        public DbConnectionManager()
        {
            _conStrBuilder = new MySqlConnectionStringBuilder
            {
                Server="localhost",
                Database = "NeoMapleStory",
                UserID = "root",
                Password = "cby159753",
                Pooling = true,
                MaximumPoolSize = 100,
                MinimumPoolSize = 5
            };
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_conStrBuilder.ConnectionString);
        }
    }
}
