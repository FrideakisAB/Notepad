using Microsoft.Data.Sqlite;

namespace DZNotepad
{
    public class DBContext
    {
        private static SqliteConnection connection = null;

        /// <summary>
        /// Выполняет инициализацию, и возвращает контекст базы данных
        /// </summary>
        /// <returns>Контекст базы данных</returns>
        public static SqliteConnection Get()
        {
            if (connection == null)
                connection = new SqliteConnection("Data Source=data.db;Cache=Shared;");
            return connection;
        }

        /// <summary>
        /// Выполняет команду без возвращаемого значения
        /// </summary>
        /// <param name="commandStr">Строка содержащая SQL-запрос</param>
        public static void Command(string commandStr)
        {
            connection?.Close();
            connection = null;
            SqliteCommand command = new SqliteCommand();
            command.Connection = Get();
            command.CommandText = commandStr;
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Выполняет команду с возвратом одного значения
        /// </summary>
        /// <param name="commandStr">Строка содержащая SQL-запрос</param>
        /// <returns>Результат выполнения запроса</returns>
        public static object? CommandScalar(string commandStr)
        {
            connection?.Close();
            connection = null;
            SqliteCommand command = new SqliteCommand();
            command.Connection = Get();
            command.CommandText = commandStr;
            return command.ExecuteScalar();
        }

        /// <summary>
        /// Выполняет команду с возвратом таблицы значений
        /// </summary>
        /// <param name="commandStr">Строка содержащая SQL-запрос</param>
        /// <returns>Результат выполнения запроса</returns>
        public static SqliteDataReader CommandReader(string commandStr)
        {
            SqliteCommand command = new SqliteCommand();
            command.Connection = Get();
            command.CommandText = commandStr;
            return command.ExecuteReader();
        }
    }
}
