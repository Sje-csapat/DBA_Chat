﻿using masodik.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace masodik.Controllers
{
    public class ChatController : Controller
    {
        // GET: HomeController1
        public ActionResult Index()
        {
            System.Diagnostics.Debug.WriteLine(!string.IsNullOrEmpty(HttpContext.Session.GetString("logged_in")));
            //return View("Session");
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("logged_in")))
            {
                ViewData["page"] = "Chat";
                ViewData["session"] = "true";
                ViewBag.Message = HttpContext.Session.GetString("user_username");
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        [HttpGet]
        [Produces("application/json")]
        public IActionResult GetUsers()
        {
            if(string.IsNullOrEmpty(HttpContext.Session.GetString("logged_in")))
            {
                return Json(new { error = "Lost session"} );
            }
            SQLiteConnection dbconn = Elerhato.Dbconn;
            dbconn.Open();
            //System.Diagnostics.Debug.WriteLine(username);

            using var cmd = new SQLiteCommand(dbconn);
            cmd.CommandText = "SELECT id,username FROM users ORDER BY username COLLATE NOCASE ASC";
            cmd.Prepare();
             SQLiteDataReader rdr = cmd.ExecuteReader();

            Dictionary<string, string> users = new Dictionary<string, string>();
            List<Models.User> asd = new List<Models.User>();
            while (rdr.Read())
            {
                Models.User tmp = new Models.User(rdr.GetInt32(0));
                asd.Add(tmp);
                //System.Diagnostics.Debug.WriteLine($"{rdr.GetInt32(0)} {rdr.GetString(1)} {rdr.GetString(2)}");
                users.Add((string)rdr.GetInt32(0).ToString(), rdr.GetString(1));

            }
            dbconn.Close();

            return Json(asd.ToArray());

            /*System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(users));
            //ViewBag.Message = name;
            var felhasznalok = JsonConvert.SerializeObject(users);*/
            //return Ok(users);
        }

        [HttpGet]
        [Produces("application/json")]
        public IActionResult GetMessages(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("logged_in")))
            {
                return Json(new { error = "Lost session" });
            }
            SQLiteConnection dbconn = Elerhato.Dbconn;
            dbconn.Open();
            //System.Diagnostics.Debug.WriteLine(username);

            using var cmd = new SQLiteCommand(dbconn);
            cmd.CommandText = "SELECT * FROM messages WHERE (sender = @sender AND receiver = @receiver) OR (sender = @receiver AND receiver = @sender) ORDER BY created_at DESC LIMIT 25";
            cmd.Parameters.AddWithValue("@sender", HttpContext.Session.GetString("user_id"));
            cmd.Parameters.AddWithValue("@receiver", id);
            cmd.Prepare();
            SQLiteDataReader rdr = cmd.ExecuteReader();

            List<Models.Message> messages = new List<Models.Message>();
            while (rdr.Read())
            {
                Models.Message tmp_msg;
                if(rdr.GetInt32(1) == Convert.ToInt32(HttpContext.Session.GetString("user_id")))
                {
                    tmp_msg = new Models.Message(rdr.GetString(3), rdr.GetInt32(4), true);
                } else
                {
                    tmp_msg = new Models.Message(rdr.GetString(3), rdr.GetInt32(4), false);
                }
                messages.Add(tmp_msg);
            }
            dbconn.Close();
            messages.Reverse();
            return Json(messages.ToArray());
        }

        [HttpPost]
        public ActionResult SendMessage(IFormCollection collection)
        {
            try
            {
                var id=collection["id"];
                System.Diagnostics.Debug.WriteLine(id);

                SQLiteConnection dbconn = Elerhato.Dbconn;
                dbconn.Open();

                using var cmd = new SQLiteCommand(dbconn);
                cmd.CommandText = "INSERT INTO messages (sender, receiver, message, created_at) values(@sender, @receiver, @message, @created_at)";
                cmd.Parameters.AddWithValue("@sender", HttpContext.Session.GetString("user_id"));
                cmd.Parameters.AddWithValue("@receiver", collection["id"]);
                cmd.Parameters.AddWithValue("@message", collection["msg"]);
                cmd.Parameters.AddWithValue("@created_at", (int)DateTimeOffset.Now.ToUnixTimeSeconds());
                cmd.Prepare();
                cmd.ExecuteNonQuery();

                dbconn.Close();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
