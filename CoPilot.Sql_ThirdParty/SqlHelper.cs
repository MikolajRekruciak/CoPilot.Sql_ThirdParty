using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoPilot.Sql_ThirdParty
{
    public static class SqlHelper
    {
        private const int MaxRetryCount = 3;

        public static int ExecuteNonQuery(string commandText)
        {
            return ExecuteNonQuery(new SqlCommand(commandText));
        }

        public static object ExecuteScalar(string commandText)
        {
            return ExecuteScalar(new SqlCommand(commandText));
        }

        public static void ExecuteReader(string commandText, Action<SqlDataReader> readAction)
        {
            ExecuteReader(new SqlCommand(commandText), readAction);
        }

        public static int ExecuteNonQuery(SqlCommand command)
        {
            return ExecuteWithRetry(() =>
            {
                using (var transaction = command.Connection.BeginTransaction())
                {
                    command.Transaction = transaction;
                    try
                    {
                        int result = command.ExecuteNonQuery();
                        transaction.Commit();
                        return result;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }, command);
        }

        public static object ExecuteScalar(SqlCommand command)
        {
            return ExecuteWithRetry(() =>
            {
                using (var transaction = command.Connection.BeginTransaction())
                {
                    command.Transaction = transaction;
                    try
                    {
                        object result = command.ExecuteScalar();
                        transaction.Commit();
                        return result;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }, command);
        }

        public static void ExecuteReader(SqlCommand command, Action<SqlDataReader> readAction)
        {
            ExecuteWithRetry(() =>
            {
                using (var transaction = command.Connection.BeginTransaction())
                {
                    command.Transaction = transaction;
                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                readAction(reader);
                            }
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }, command);
        }

        public static async Task<int> ExecuteNonQueryAsync(string commandText)
        {
            return await ExecuteNonQueryAsync(new SqlCommand(commandText));
        }

        public static async Task<object> ExecuteScalarAsync(string commandText)
        {
            return await ExecuteScalarAsync(new SqlCommand(commandText));
        }

        public static async Task ExecuteReaderAsync(string commandText, Func<SqlDataReader, Task> readAction)
        {
            await ExecuteReaderAsync(new SqlCommand(commandText), readAction);
        }

        public static async Task<int> ExecuteNonQueryAsync(SqlCommand command)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                using (var transaction = command.Connection.BeginTransaction())
                {
                    command.Transaction = transaction;
                    try
                    {
                        int result = await command.ExecuteNonQueryAsync();
                        transaction.Commit();
                        return result;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }, command);
        }

        public static async Task<object> ExecuteScalarAsync(SqlCommand command)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                using (var transaction = command.Connection.BeginTransaction())
                {
                    command.Transaction = transaction;
                    try
                    {
                        object result = await command.ExecuteScalarAsync();
                        transaction.Commit();
                        return result;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }, command);
        }

        public static async Task ExecuteReaderAsync(SqlCommand command, Func<SqlDataReader, Task> readAction)
        {
            await ExecuteWithRetryAsync(async () =>
            {
                using (var transaction = command.Connection.BeginTransaction())
                {
                    command.Transaction = transaction;
                    try
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                await readAction(reader);
                            }
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }, command);
        }

        private static T ExecuteWithRetry<T>(Func<T> execute, SqlCommand command)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    if (command.Connection == null)
                    {
                        command.Connection = new SqlConnection(Initialize.ConnectionString);
                    }

                    if (command.Connection.State != System.Data.ConnectionState.Open)
                    {
                        command.Connection.Open();
                    }

                    return execute();
                }
                catch (SqlException ex) when (IsTransientError(ex))
                {
                    retryCount++;
                    if (retryCount >= MaxRetryCount)
                    {
                        LogError(ex, command);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, command);
                    throw;
                }
                finally
                {
                    if (command.Connection != null && command.Connection.State == System.Data.ConnectionState.Open)
                    {
                        command.Connection.Close();
                    }
                    command.Dispose();
                }
            }
        }

        private static async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> execute, SqlCommand command)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    if (command.Connection == null)
                    {
                        command.Connection = new SqlConnection(Initialize.ConnectionString);
                    }

                    if (command.Connection.State != System.Data.ConnectionState.Open)
                    {
                        await command.Connection.OpenAsync();
                    }

                    return await execute();
                }
                catch (SqlException ex) when (IsTransientError(ex))
                {
                    retryCount++;
                    if (retryCount >= MaxRetryCount)
                    {
                        LogError(ex, command);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, command);
                    throw;
                }
                finally
                {
                    if (command.Connection != null && command.Connection.State == System.Data.ConnectionState.Open)
                    {
                        command.Connection.Close();
                    }
                    command.Dispose();
                }
            }
        }

        private static void ExecuteWithRetry(Action execute, SqlCommand command)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    if (command.Connection == null)
                    {
                        command.Connection = new SqlConnection(Initialize.ConnectionString);
                    }

                    if (command.Connection.State != System.Data.ConnectionState.Open)
                    {
                        command.Connection.Open();
                    }

                    execute();
                    return;
                }
                catch (SqlException ex) when (IsTransientError(ex))
                {
                    retryCount++;
                    if (retryCount >= MaxRetryCount)
                    {
                        LogError(ex, command);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, command);
                    throw;
                }
                finally
                {
                    if (command.Connection != null && command.Connection.State == System.Data.ConnectionState.Open)
                    {
                        command.Connection.Close();
                    }
                    command.Dispose();
                }
            }
        }

        private static async Task ExecuteWithRetryAsync(Func<Task> execute, SqlCommand command)
        {
            int retryCount = 0;
            while (true)
            {
                try
                {
                    if (command.Connection == null)
                    {
                        command.Connection = new SqlConnection(Initialize.ConnectionString);
                    }

                    if (command.Connection.State != System.Data.ConnectionState.Open)
                    {
                        await command.Connection.OpenAsync();
                    }

                    await execute();
                    return;
                }
                catch (SqlException ex) when (IsTransientError(ex))
                {
                    retryCount++;
                    if (retryCount >= MaxRetryCount)
                    {
                        LogError(ex, command);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, command);
                    throw;
                }
                finally
                {
                    if (command.Connection != null && command.Connection.State == System.Data.ConnectionState.Open)
                    {
                        command.Connection.Close();
                    }
                    command.Dispose();
                }
            }
        }

        private static bool IsTransientError(SqlException ex)
        {
            // Sprawdź kody błędów SQL, które są przejściowe (np. timeout, deadlock)
            foreach (SqlError error in ex.Errors)
            {
                switch (error.Number)
                {
                    case -2: // Timeout
                    case 1205: // Deadlock
                        return true;
                }
            }
            return false;
        }

        private static void LogError(Exception ex, SqlCommand command)
        {
            try
            {
                using (var connection = new SqlConnection(Initialize.ConnectionString))
                {
                    var logCommand = new SqlCommand($@"
                INSERT INTO {Initialize.Config.LogTableName} (ApplicationName, ExceptionMessage, LogDate)
                VALUES (@AppName, @Message, @LogDate)", connection);
                    logCommand.Parameters.AddWithValue("@AppName", $"{Initialize.Config.AppName} - {Environment.UserName}");
                    logCommand.Parameters.AddWithValue("@Message", $"{ex.Message}\n\n{ex.ToString()}\n\nCommandText: {command.CommandText}\n\nParameters: {GetCommandParameters(command)}");
                    logCommand.Parameters.AddWithValue("@LogDate", DateTime.Now);

                    connection.Open();
                    logCommand.ExecuteNonQuery();
                }
            }
            catch (Exception logEx)
            {
                MessageBox.Show("Błąd podczas logowania błędu: " + logEx.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GetCommandParameters(SqlCommand command)
        {
            var parameters = string.Empty;
            foreach (SqlParameter param in command.Parameters)
            {
                parameters += $"{param.ParameterName}={param.Value}, ";
            }
            return parameters.TrimEnd(',', ' ');
        }
    }
}