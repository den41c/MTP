using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
namespace stp.Classes
{
    class SqlClient
    {
        //string conntectionString;
        public static SQLiteCommand CreateCommand(string commString)
        {
           
            var conntectionString = @"Data Source = C:\Users\densk\source\repos\stp\stp\sqllite\stp.db; Version = 3";          
            SQLiteConnection con = new SQLiteConnection(conntectionString);

                try
                {
                    con.Open();
                }
                catch
                {
                    
                }
                if (con.State == System.Data.ConnectionState.Open) { }
                var com = con.CreateCommand();
                com.CommandText = commString;
                return com;

        }
    }
}
