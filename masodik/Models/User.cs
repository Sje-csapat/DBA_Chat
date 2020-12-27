using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace masodik.Models
{
    public class User
    {
        public int id { get; set; }
        public string username { get; set; }
        public string password { private get; set; }
        public int status { get; set; }
        public int created_at { get; set; }
        public int updated_at { get; set; }

        public User(int id)
        {
            SQLiteConnection dbconn = Globals.Dbconn;
            int conn_status = 0;
            if (dbconn != null && dbconn.State == ConnectionState.Closed)
            {
                conn_status = 1;
                dbconn.Open();
            }

            using var cmd = new SQLiteCommand(dbconn);
            cmd.CommandText = "SELECT * FROM users WHERE id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Prepare();
            SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                this.id = rdr.GetInt32(0);
                username = rdr.GetString(1);
                password = rdr.GetString(2);
                status = rdr.GetInt32(3);
                created_at = Convert.ToInt32(rdr.GetFloat(4));
                if (rdr[5].GetType() != typeof(DBNull))
                {
                    updated_at = Convert.ToInt32(rdr.GetInt32(5));
                }
            }
            if (Convert.ToBoolean(conn_status))
            {
                dbconn.Close();
            }
        }

        public bool save()
        {
            SQLiteConnection dbconn = Globals.Dbconn;
            int conn_status = 0;
            if (dbconn != null && dbconn.State == ConnectionState.Closed)
            {
                conn_status = 1;
                dbconn.Open();
            }
            using var cmd = new SQLiteCommand(dbconn);
            cmd.CommandText = "UPDATE users SET status = @status, updated_at = @updated_at WHERE id = @id";
            cmd.Parameters.AddWithValue("@status", this.status);
            cmd.Parameters.AddWithValue("@updated_at", (int) DateTimeOffset.Now.ToUnixTimeSeconds());
            cmd.Parameters.AddWithValue("@id", this.id);
            cmd.Prepare();

            int modified = 1;
            if(cmd.ExecuteNonQuery() >= 1)
            {
                modified = 1;
            }

            if (Convert.ToBoolean(conn_status))
            {
                dbconn.Close();
            }

            if(modified == 1)
            {
                return true;
            } else
            {
                return false;
            }
        }
    }
}
