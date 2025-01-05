using System.Windows;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace CoursFlairy.Data
{
    class DataBase
    {
        private static SqlConnection sqlConnection;

        static DataBase()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.IndexOf(@"\bin")));
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["FlairyDB"].ConnectionString);
        }

        public static void Open()
        {
            if (sqlConnection.State == ConnectionState.Closed)
            {
                sqlConnection.Open();
            }
        }

        public static void Close()
        {
            if (sqlConnection.State == ConnectionState.Open)
            {
                sqlConnection.Close();
            }
        }

        public static SqlConnection GetConnection()
        {
            return sqlConnection;
        }
    }

}
