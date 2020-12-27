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
            cmd.CommandText = "SELECT id, password FROM users WHERE `username` = @username";
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Prepare();
            SQLiteDataReader rdr = cmd.ExecuteReader();

            int isLoggedIn = 0;
            while (rdr.Read())
            {
                Models.User user = new Models.User(rdr.GetInt32(0));
                //System.Diagnostics.Debug.WriteLine($"{rdr.GetInt32(0)} {rdr.GetString(1)} {rdr.GetString(2)}");
                string db_password = rdr.GetString(1);
                bool pwd_verify = Globals.VerifyPassword(db_password, password);
                if (pwd_verify) {
                    ViewBag.Message = passdata;
                    user.status = 1;
                    user.save();

                    HttpContext.Session.SetString("is_logged_in", "1");
                    HttpContext.Session.SetString("user_id", user.id.ToString());
                    HttpContext.Session.SetString("user_username", user.username);
                    isLoggedIn = 1;
                    
                    Globals.Logger(user.id, "Login succes");
                } else
                {
                    //password mismatch
                    Globals.Logger(user.id, "Failed login - password mismatch");
                    isLoggedIn = 0;
                }
            }
            dbconn.Close();
            var name = HttpContext.Session.GetString("user_username");
            ViewBag.Message = name;
            if (isLoggedIn == 1)
            {
                return RedirectToAction("Index", "Chat");
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
    }
}
