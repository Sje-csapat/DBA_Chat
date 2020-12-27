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
            //return View("Session");
            if ((HttpContext.Session.GetString("logged_in")!=null))
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
        [Route("Log")]
        [HttpPost]
        public ActionResult Log(string username, string password)
        {
            
            Home passdata = new Home
            {
                username = username,
                password = password
            };
            
            SQLiteConnection dbconn = Elerhato.Dbconn;
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
                bool pwd_verify = Elerhato.VerifyPassword(db_password, password);
                if (pwd_verify) {
                    ViewBag.Message = passdata;
                    user.status = 1;
                    user.save();

                    HttpContext.Session.SetString("logged_in", "1");
                    HttpContext.Session.SetString("user_id", user.id.ToString());
                    HttpContext.Session.SetString("session_username", user.username);
                    isLoggedIn = 1;
                    
                    Elerhato.Logger(user.id, "Login succes");
                } else
                {
                    //password mismatch
                    Elerhato.Logger(user.id, "Failed login - password mismatch");
                    ViewBag.Message = "Rossz jelszo vagy user";
                    isLoggedIn = 0;
                }
            }
            dbconn.Close();
            var name = HttpContext.Session.GetString("session_username");
            
            if (isLoggedIn == 1)
            {
                ViewBag.Message = name;
                return RedirectToAction("Index", "Chat");
            }
            else
            {
                return View("Login");
            }
            
        }

        [Route("Reg")]
        [HttpPost]
        public ActionResult Reg(string username, string password, string password2)
        {
            string hashed = Elerhato.HashPassword(password, null, false);

            Register passdata = new Register
            {
                username = hashed,
                password = password,
                password2 = password2
            };
            if (password != password2)
            {
                ViewBag.Message = "Nem eggyezik a jelszo";
                return View("Register");
            }


            try
            {
                HttpContext.Session.SetString("session_username", username);

                SQLiteConnection dbconn = Elerhato.Dbconn;
                dbconn.Open();

                using var cmd = new SQLiteCommand(dbconn);
                cmd.CommandText = "INSERT INTO users (username, password, created_at) values(@username, @password, @created_at)";
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", hashed);
                cmd.Parameters.AddWithValue("@created_at", (string)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString());
                cmd.Prepare();
                cmd.ExecuteNonQuery();

                dbconn.Close();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ViewBag.Message = "Letezo felhasznalo";
                return View("Register");
            }
        }

        [Route("Logout")]
        [HttpGet]
        public ActionResult Logout()
        {
            var id = HttpContext.Session.GetString("user_id");
            Models.User tmp_user = new Models.User(Convert.ToInt32(id));
            Elerhato.Logger(tmp_user.id, "User logged out");
            tmp_user.status = 0;
            tmp_user.save();
            HttpContext.Session.Clear();
            return View("Login");
        }
    }
}
