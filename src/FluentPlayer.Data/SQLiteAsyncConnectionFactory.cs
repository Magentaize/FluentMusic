using System;
using System.IO;
using SQLite;

namespace FluentPlayer.Data
{
    public class SQLiteAsyncConnectionFactory
    {
        private SQLiteAsyncConnection _conn;

        public string DatabaseFile;// => Path.Combine(SettingsClient.ApplicationFolder(), ProductInformation.ApplicationName + ".db");
      
        public SQLiteAsyncConnection GetConnection()
        {
            if (_conn != null) return _conn;
            _conn= new SQLiteAsyncConnection(DatabaseFile);
            _conn.SetBusyTimeoutAsync(new TimeSpan(0, 0, 0, 10));
            return _conn;
        }
    }
}