using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Sql_ThirdParty
{
    public static class Initialize
    {
        internal static string ConnectionString;

        internal static InitConfig Config;

        /// <summary>
        /// Initializes the logging system with the specified application name and optional database credentials.
        /// </summary>
        /// <remarks>
        /// Ensure that the connection string is defined in the App.config file with the name "SQL" or the custom name provided.
        /// Example:
        /// <code>
        ///     &lt;connectionStrings&gt;
        ///         &lt;add name="SQL" connectionString="Data Source=server;Initial Catalog=database;Integrated Security=True;" providerName="System.Data.SqlClient" /&gt;
        ///     &lt;/connectionStrings&gt;
        /// </code>
        /// </remarks>
        public static void Init(InitConfig config)
        {
            Config = config;

            try
            {
                var configConnectionString = String.IsNullOrEmpty(config.ConnectionString)
                    ? ConfigurationManager.ConnectionStrings[config.CustomConnectionStringName]?.ConnectionString
                                             ?? ConfigurationManager.ConnectionStrings["SQL"]?.ConnectionString
                    : config.ConnectionString;

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


        private static void SetLogRetention(int days)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    var command = new SqlCommand($@"
DECLARE @days INT = {Config.LogRetentionDays}; -- Przykładowa liczba dni
DECLARE @sql NVARCHAR(MAX);

SET @sql = 'DELETE FROM ' + @tableName + ' WHERE LogDate < DATEADD(DAY, -' + CAST(@days AS NVARCHAR) + ', GETDATE())';
EXEC sp_executesql @sql;", connection);
                    command.Parameters.AddWithValue("@tableName", Config.LogTableName);
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
                        DECLARE @sql NVARCHAR(MAX);

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name=@tableName AND xtype='U')
BEGIN
    SET @sql = 'CREATE TABLE ' + @tableName + ' (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ApplicationName NVARCHAR(100),
        Message NVARCHAR(MAX),
        LogDate DATETIME
    )';
    EXEC sp_executesql @sql;
END", connection);

                    command.Parameters.AddWithValue("@tableName", Config.LogTableName);
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
