using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace masodik
{
    public static class Globals
    {
        public static SQLiteConnection Dbconn { get; set; }
        public static void TestDBConnection(string contentRoot)
        {
            string dabasestr = contentRoot + "/database.db";
            System.Diagnostics.Debug.WriteLine(dabasestr);

            if (File.Exists(dabasestr))
            {
                string connectionstring = "Data Source=" + dabasestr + ";Version=3;";
                Dbconn = new SQLiteConnection(connectionstring);
                System.Diagnostics.Debug.WriteLine("DB Works fine");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Database not found, shutting down!");
                Environment.Exit(1);
            }
        }
    }
}
