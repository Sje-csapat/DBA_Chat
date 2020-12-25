using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using masodik.Models;
using System.Data.SQLite;

namespace masodik
{
    public class HomeController : Controller
    {
        
        // GET: HomeController
        public ActionResult Index()
        {
            return View();
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
            System.Diagnostics.Debug.WriteLine(username);

            using var cmd = new SQLiteCommand(dbconn);
            cmd.CommandText = "SELECT * FROM users WHERE `username` = @username";
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Prepare();
            SQLiteDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                //System.Diagnostics.Debug.WriteLine($"{rdr.GetInt32(0)} {rdr.GetString(1)} {rdr.GetString(2)}");
                string db_password = rdr.GetString(2);
                if (db_password == password) {
                    ViewBag.Message = passdata;
                    return View("Proba");
                } else
                {
                    //password mismatch
                    ViewBag.Message = passdata;
                    return View("Proba");
                }
            }
            dbconn.Close();
            ViewBag.Message = passdata;
            return View("Proba");
        }
        public ActionResult Reg(string username, string password, string password2)
        {
            Register passdata = new Register
            {
                username = username,
                password = password,
                password2 = password2
            };
            if (password != password2) return View("Register");

            
            SQLiteConnection dbconn = Globals.Dbconn;
            dbconn.Open();

            using var cmd = new SQLiteCommand(dbconn);
            cmd.CommandText = "INSERT INTO users (username, password, created_at) values(@username, @password, @created_at)";
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.Parameters.AddWithValue("@created_at", (string)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString());
            cmd.Prepare();
            cmd.ExecuteNonQuery();

            dbconn.Close();

            ViewBag.Message = passdata;
            return View("Proba");
        }
    }
}
