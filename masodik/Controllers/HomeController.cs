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
        public ActionResult Log(string username,string password)
        {
            Home passdata = new Home
            {
                username = username,
                password = password
            };
           
                int ido = 5555;
                string connectionstring = "Data Source=C:/Users/salma/Desktop/Adatbazis_alkalmazasok/marak/DBA/masodik/database.db;Version=3;";
                using SQLiteConnection dbconn = new SQLiteConnection(connectionstring);
                dbconn.Open();
                


                SQLiteCommand cmd = new SQLiteCommand("insert into users (username, password, created_at) values( @productId,@email,@ido)", dbconn);
                cmd.Parameters.Add(new SQLiteParameter("@productId", username));
                cmd.Parameters.Add(new SQLiteParameter("@email", password));
                cmd.Parameters.Add(new SQLiteParameter("@ido", ido));

                cmd.ExecuteNonQuery();
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
            

            int ido = 5555;
            string connectionstring = "Data Source=C:/Users/salma/Desktop/Adatbazis_alkalmazasok/marak/DBA/masodik/database.db;Version=3;";
            using SQLiteConnection dbconn = new SQLiteConnection(connectionstring);
            dbconn.Open();



            SQLiteCommand cmd = new SQLiteCommand("insert into users (username, password, created_at) values( @productId,@email,@ido)", dbconn);
            cmd.Parameters.Add(new SQLiteParameter("@productId", username));
            cmd.Parameters.Add(new SQLiteParameter("@email", password));
            cmd.Parameters.Add(new SQLiteParameter("@ido", ido));

            cmd.ExecuteNonQuery();
            dbconn.Close();



            ViewBag.Message = passdata;

            return View("Proba");
        }



    }
}
