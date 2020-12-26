using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Data;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace masodik
{
    public static class Globals
    {
        public static SQLiteConnection Dbconn { get; set; }
        public static SQLiteConnection Dbasdadsconn { get; set; }
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
        public static void Logger(int user_id,string message)
        {
            var dbconn = Globals.Dbconn;
            int conn_status = 0;
            if(dbconn != null && dbconn.State == ConnectionState.Closed)
            {
                conn_status = 1;
                dbconn.Open();
            }
            DateTime now = DateTime.Now;
            string created_at = now.ToString();
            using var cmd = new SQLiteCommand(dbconn); 
            cmd.CommandText = "INSERT INTO Log(user_id,message,created_at) values(@user_id, @message, @created_at)";
            cmd.Parameters.AddWithValue("@user_id", user_id.ToString());
            cmd.Parameters.AddWithValue("@message", message);
            cmd.Parameters.AddWithValue("@created_at", created_at);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            if(Convert.ToBoolean(conn_status))
            {
                dbconn.Close();
            }
        }
        public static string HashPassword(string password, byte[] salt = null, bool needsOnlyHash = false)
        {
            if (salt == null || salt.Length != 16)
            {
                // generate a 128-bit salt using a secure PRNG
                salt = new byte[128 / 8];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }
            }
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
               password: password,
               salt: salt,
               prf: KeyDerivationPrf.HMACSHA256,
               iterationCount: 10000,
               numBytesRequested: 256 / 8));

            if (needsOnlyHash) return hashed;
            // password will be concatenated with salt using ':'
            return $"{hashed}:{Convert.ToBase64String(salt)}";
        }
        public static bool VerifyPassword(string hashedPasswordWithSalt, string passwordToCheck)
        {
            // retrieve both salt and password from 'hashedPasswordWithSalt'
            var passwordAndHash = hashedPasswordWithSalt.Split(':');
            if (passwordAndHash == null || passwordAndHash.Length != 2)
                return false;
            var salt = Convert.FromBase64String(passwordAndHash[1]);
            if (salt == null)
                return false;
            // hash the given password
            var hashOfpasswordToCheck = HashPassword(passwordToCheck, salt, true);
            // compare both hashes
            if (String.Compare(passwordAndHash[0], hashOfpasswordToCheck) == 0)
            {
                return true;
            }
            return false;
        }
    }
}
