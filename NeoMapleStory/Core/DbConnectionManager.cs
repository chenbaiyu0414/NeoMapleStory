using MySql.Data.MySqlClient;

namespace NeoMapleStory.Core
{
    public class DbConnectionManager
    {
        private readonly MySqlConnectionStringBuilder m_conStrBuilder;

        public DbConnectionManager()
        {
            m_conStrBuilder = new MySqlConnectionStringBuilder
            {
                Server = "localhost",
                Database = "NeoMapleStory",
                UserID = "root",
                Password = "cby159753",
                Pooling = true,
                MaximumPoolSize = 100,
                MinimumPoolSize = 5
            };
        }

        public static DbConnectionManager Instance { get; } = new DbConnectionManager();

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(m_conStrBuilder.ConnectionString);
        }
    }
}