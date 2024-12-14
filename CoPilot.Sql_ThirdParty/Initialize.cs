using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CoPilot.Sql_ThirdParty
{
    public static class Initialize
    {
        internal static string ConnectionString;

        internal static InitConfig Config;

        /// <summary>
        /// Initializes the logging system with the specified application name and optional database credentials.
        /// </summary>
        /// <param name="appName">The name of the application.</param>
        /// <param name="username">The username for the database connection (optional).</param>
        /// <param name="password">The password for the database connection (optional).</param>
        /// <param name="customConnectionStringName">The name of the custom connection string in the configuration file (optional).</param>
        /// <remarks>
        /// Ensure that the connection string is defined in the App.config file with the name "SQL" or the custom name provided.
        /// Example:
        /// <code>
        /// <configuration>
        ///     <connectionStrings>
        ///         <add name="SQL" connectionString="Data Source=server;Initial Catalog=database;Integrated Security=True;" providerName="System.Data.SqlClient" />
        ///     </connectionStrings>
        /// </configuration>
        /// </code>
        /// </remarks>
        public static void Init(InitConfig config)
        {
            Config = config;

            try
            {
                var configConnectionString = ConfigurationManager.ConnectionStrings[config.CustomConnectionStringName]?.ConnectionString
                                             ?? ConfigurationManager.ConnectionStrings["SQL"]?.ConnectionString;

                if (configConnectionString == null)
                {
                    throw new InvalidOperationException("Nie znaleziono connectionStringa o podanej nazwie ani domyślnego 'SQL'.");
                }

                var builder = new SqlConnectionStringBuilder(configConnectionString);

                if (builder.IntegratedSecurity == false && !string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
                {
                    builder.UserID = config.Username;
                    builder.Password = config.Password;
                }

                ConnectionString = builder.ConnectionString;
                CreateLogsTable();
                SetLogRetention(config.LogRetentionDays);
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Błąd połączenia z bazą danych: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił nieoczekiwany błąd: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void LogError(Exception ex)
        {
            LogToTable($"{ex.Message}\n\n{ex.ToString()}");
        }

        public static void LogToTable(string message)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    var command = new SqlCommand(@"
                INSERT INTO CDN._IM_MR_Logs (ApplicationName, ExceptionMessage, LogDate)
                VALUES (@AppName, @Message, @LogDate)", connection);
                    command.Parameters.AddWithValue("@AppName", $"{Config.AppName} - {Environment.UserName}");
                    command.Parameters.AddWithValue("@Message", message);
                    command.Parameters.AddWithValue("@LogDate", DateTime.Now);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Błąd podczas logowania błędu: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił nieoczekiwany błąd: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private static void SetLogRetention(int days)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    var command = new SqlCommand($@"
                DELETE FROM CDN._IM_MR_Logs WHERE LogDate < DATEADD(DAY, -{days}, GETDATE())", connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Błąd podczas ustawiania retencji logów: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił nieoczekiwany błąd: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void CreateLogsTable()
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    var command = new SqlCommand(@"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CDN._IM_MR_Logs' AND xtype='U')
                        CREATE TABLE CDN._IM_MR_Logs (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            ApplicationName NVARCHAR(100),
                            Message NVARCHAR(MAX),
                            LogDate DATETIME
                        )", connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Błąd podczas tworzenia tabeli logów: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił nieoczekiwany błąd: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
