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

        public ActionResult Index()
        {
            //return View("Session");
            if ((HttpContext.Session.GetString("logged_in") != null))
            {
                ViewData["session"] = "true";
                return RedirectToAction("Index", "Chat");
            }
            else
            {
                ViewData["session"] = null;
                return RedirectToAction(nameof(Login));
            }
        }

        [Route("login")]
        [HttpGet]
        public ActionResult Login()
        {
            ViewData["session"] = null;
            ViewData["page"] = "Login";
            return View();
        }

        [Route("register")]
        [HttpGet]
        public ActionResult Register()
        {
            ViewData["session"] = null;
            ViewData["page"] = "Register";
            return View();
        }

        [Route("Log")]
        [HttpPost]
        public ActionResult Log(string username, string password)
        {
            
            SQLiteConnection dbconn = Elerhato.Dbconn;
            dbconn.Open();
            //System.Diagnostics.Debug.WriteLine(username);

            using var cmd = new SQLiteCommand(dbconn);
            cmd.CommandText = "SELECT id, password FROM users WHERE `username` = @username";
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Prepare();
            SQLiteDataReader rdr = cmd.ExecuteReader();

            int isLoggedIn = 0;
            List<string> errors = new List<string>();
            if(rdr.HasRows)
            {
                while (rdr.Read())
                {
                    Models.User user = new Models.User(rdr.GetInt32(0));
                    //System.Diagnostics.Debug.WriteLine($"{rdr.GetInt32(0)} {rdr.GetString(1)} {rdr.GetString(2)}");
                    string db_password = rdr.GetString(1);
                    bool pwd_verify = Elerhato.VerifyPassword(db_password, password);
                    if (pwd_verify)
                    {
                        user.status = 1;
                        user.save();

                        HttpContext.Session.SetString("logged_in", "1");
                        HttpContext.Session.SetString("user_id", user.id.ToString());
                        HttpContext.Session.SetString("user_username", user.username);
                        isLoggedIn = 1;

                        Elerhato.Logger(user.id, "Login succes");
                    }
                    else
                    {
                        //password mismatch
                        Elerhato.Logger(user.id, "Failed login - password mismatch");
                        errors.Add("Hibás felhasználónév vagy jelszó!");
                        isLoggedIn = 0;
                    }
                }
            } else
            {
                errors.Add("Hibás felhasználónév vagy jelszó!");
            }
            
            dbconn.Close();
            if (isLoggedIn == 1)
            {
                return RedirectToAction(nameof(Index));
                //return RedirectToAction("Index", "Chat");
            }
            else
            {
                ViewData["page"] = "Login";
                if(errors.Count() >= 1)
                {
                    ViewData["session"] = null;
                    ViewData["page"] = "Login";
                    ViewBag.AlertType = "danger";
                    ViewBag.AlertMsg = "Az űrlap beküldése során a következő hibákat találtuk:\r\n" + string.Join("\r\n", errors);
                }
                //ViewData["errors"] = "Login";
                return View("Login");
            }
        }

        [Route("Reg")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reg(string username, string password, string password2)
        {
            
            List<string> errors = new List<string>();

            if ( (username == null || password == null || password2 == null) || (username == "" || password == "" || password2 == ""))
            {
                errors.Add("Üres bemenet!");
            }

            if (password != password2)
            {
                errors.Add("A megadott jelszavak nem egyeznek meg!");
            }

            if(errors.Count() == 0) {
                try
                {
                    string hashed = Elerhato.HashPassword(password, null, false);

                    SQLiteConnection dbconn = Elerhato.Dbconn;
                    dbconn.Open();

                    using var cmd = new SQLiteCommand(dbconn);
                    cmd.CommandText = "INSERT INTO users (username, password, status, created_at) values(@username, @password, @status, @created_at)";
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", hashed);
                    cmd.Parameters.AddWithValue("@status", "0");
                    cmd.Parameters.AddWithValue("@created_at", (string)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString());
                    cmd.Prepare();
                    cmd.ExecuteNonQuery();

                    dbconn.Close();

                    ViewData["session"] = null;
                    ViewData["page"] = "Login";
                    ViewBag.AlertType = "success";
                    ViewBag.AlertMsg = "Sikeres regisztráció! Mostmár beléphetsz!";
                    return View("Login");
                }
                catch
                {
                    errors.Add("Ilyen nevű felhasználó már létezik!");
                }
            }

            if(errors.Count() > 0)
            {
                ViewBag.AlertType = "danger";
                ViewBag.AlertMsg = "Az űrlap beküldése során a következő hibákat találtuk:\r\n" + string.Join("\r\n", errors);
            }
            ViewData["session"] = null;
            ViewData["page"] = "Register";
            return View("Register");
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

            return RedirectToAction(nameof(Index));
        }
    }
}
