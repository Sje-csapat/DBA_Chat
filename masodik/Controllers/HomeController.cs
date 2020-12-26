using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using masodik.Models;
using System.Data.SQLite;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Newtonsoft.Json;

namespace masodik
{
    public class HomeController : Controller
    {

        // GET: HomeController
        
        public ActionResult Index()
        {
            
            System.Diagnostics.Debug.WriteLine(!string.IsNullOrEmpty(HttpContext.Session.GetString("is_logged_in")));
            //return View("Session");
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("is_logged_in")))
            {
                //redirect or view
                return RedirectToAction("Proba");
            }
            else
            {
                return RedirectToAction(nameof(Login));
            }
            
        }

        [Route("login")]
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [Route("register")]
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [Route("login")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        public ActionResult Log(string username, string password)
        {
            
            Home passdata = new Home
            {
                username = username,
                password = password
            };
            
            SQLiteConnection dbconn = Globals.Dbconn;
            dbconn.Open();
            //System.Diagnostics.Debug.WriteLine(username);

            using var cmd = new SQLiteCommand(dbconn);
            cmd.CommandText = "SELECT * FROM users WHERE `username` = @username";
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Prepare();
            SQLiteDataReader rdr = cmd.ExecuteReader();

            int isLoggedIn = 0;
            while (rdr.Read())
            {
                //System.Diagnostics.Debug.WriteLine($"{rdr.GetInt32(0)} {rdr.GetString(1)} {rdr.GetString(2)}");
                string db_password = rdr.GetString(2);
                bool pwd_verify = Globals.VerifyPassword(db_password, password);
                if (pwd_verify) {
                    ViewBag.Message = passdata;
                    HttpContext.Session.SetString("is_logged_in", "1");
                    HttpContext.Session.SetString("user_id", rdr.GetInt32(0).ToString());
                    HttpContext.Session.SetString("user_username", rdr.GetString(1).ToString());
                    isLoggedIn = 1;
                    Globals.Logger(rdr.GetInt32(0), "Login succes");
                } else
                {
                    //password mismatch

                    Globals.Logger(rdr.GetInt32(0), "Failed login - password mismatch");
                    
                    
                    isLoggedIn = 0;
                }
            }
            dbconn.Close();
            var name = HttpContext.Session.GetString("user_username");
            ViewBag.Message = name;
            if (1==isLoggedIn)
            {
                return RedirectToAction("Proba");
            }
            else
            {
                return View("Login");
            }
            
        }
        public ActionResult Reg(string username, string password, string password2)
        {
            string hashed = Globals.HashPassword(password, null, false);
            

            bool visszafejtve = Globals.VerifyPassword(hashed, "KIKasdI");
            System.Diagnostics.Debug.WriteLine(visszafejtve);

            Register passdata = new Register
            {
                username = hashed,
                password = password,
                password2 = password2
            };
            if (password != password2) return View("Register");

            HttpContext.Session.SetString("user_username",username);
            SQLiteConnection dbconn = Globals.Dbconn;
            dbconn.Open();

            using var cmd = new SQLiteCommand(dbconn);
            cmd.CommandText = "INSERT INTO users (username, password, created_at) values(@username, @password, @created_at)";
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", hashed);
            cmd.Parameters.AddWithValue("@created_at", (string)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString());
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            dbconn.Close();

            
            return RedirectToAction("Proba");
        }

        public IActionResult Proba()
        {
            SQLiteConnection dbconn = Globals.Dbconn;
            dbconn.Open();
            //System.Diagnostics.Debug.WriteLine(username);

            using var cmd = new SQLiteCommand(dbconn);
            cmd.CommandText = "SELECT id,username FROM users ORDER BY username COLLATE NOCASE ASC";
            cmd.Prepare();
            SQLiteDataReader rdr = cmd.ExecuteReader();

            Dictionary<Int32, string> users =new Dictionary<Int32, string>();
            while (rdr.Read())
            {
                //System.Diagnostics.Debug.WriteLine($"{rdr.GetInt32(0)} {rdr.GetString(1)} {rdr.GetString(2)}");
                users.Add(rdr.GetInt32(0), rdr.GetString(1));

            }
            dbconn.Close();
            System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(users));
            //ViewBag.Message = name;

            var felhasznalok = JsonConvert.SerializeObject(users);

            return Ok(felhasznalok);


        }
    }
}
