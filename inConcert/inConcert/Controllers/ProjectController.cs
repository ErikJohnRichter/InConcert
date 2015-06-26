﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using inConcert.Models;
using System.Collections.Generic;
using inConcert.Helper;


namespace inConcert.Controllers
{
    public class ProjectController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ProjectController()
        {
        }

        public ProjectController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public string Index()
        {
            List<string[]> values = new List<string[]>();
            values.Add(new string[] { "A new project", "Some description" });
            values.Add(new string[] { "Another new project", "Some other description" });
            //DataAccess.DataAccess.Create("projects", new string[] { "name", "description" }, values);        

            DataAccess.DataAccess.Update("projects", new string[] { "name" }, new string[] { "UPDATED LOL" }, new string[] { "id = 4" });
            List<List<object>> result = DataAccess.DataAccess.Read(Build.StringArray("projects"));
            string rString = "";
            rString += result.Count + "<br />";
            foreach (List<object> row in result)
            {
                rString += row.Count + "<br />";
                foreach (object col in row)
                {
                    rString += col.ToString() + "<br />";
                }
            }
            return rString;

        }

        public ActionResult Project(int id)
        {
            if (!Authorized(id))
                return Redirect("/");

            Session["ProjectViewed"] = id;

            Project project = new Project();
            project.calendars = new List<Calendar>();
            project.auths = new List<ProjectAuth>();
            project.description = "This is a static description not being pulled from the database.";
            project.name = "This is a static name";

            List<List<object>> calendarResult = DataAccess.DataAccess.Read(Build.StringArray("Calendars"), Build.StringArray("id"), Build.StringArray("project_id = " + Session["ProjectViewed"]));
            foreach (List<object> calendar in calendarResult)
            {
                Calendar cal = new Calendar();
                cal.id = (int)calendar[0];
                cal.events = new List<Event>();
                List<List<object>> eventResult = DataAccess.DataAccess.Read(Build.StringArray("Events"), Build.StringArray("id", "calendar_id", "title", "description", "time"), Build.StringArray("calendar_id = " + cal.id));
                foreach (List<object> evt in eventResult)
                {
                    Event e = new Event();
                    e.id = (int)evt[0];
                    e.calendarId = (int)evt[1];
                    e.title = (string)evt[2];
                    e.description = (string)evt[3];
                    e.time = (long)evt[4];
                    cal.events.Add(e);
                }
                project.calendars.Add(cal);
            }


            return View("Mock", project);
        }

        private bool Authorized(int projectId)
        {
            List<List<object>> result = DataAccess.DataAccess.Read(Build.StringArray("project_users"), Build.StringArray("*"), Build.StringArray("project_id = " + projectId, "user_id = '" + User.Identity.GetUserId() + "'"));
            return result.Count > 0;
        }

        public ActionResult Tester()
        {
            Project project = new Project();
            project.calendars = new List<Calendar>();
            project.auths = new List<ProjectAuth>();
            project.description = "This is a static description not being pulled from the database.";
            project.name = "This is a static name";

            List<List<object>> calendarResult = DataAccess.DataAccess.Read(Build.StringArray("Calendars"), Build.StringArray("id"), Build.StringArray("id = 1"));
            foreach (List<object> calendar in calendarResult)
            {
                Calendar cal = new Calendar();
                cal.id = (int)calendar[0];
                cal.events = new List<Event>();
                List<List<object>> eventResult = DataAccess.DataAccess.Read(Build.StringArray("Events"), Build.StringArray("id", "calendar_id", "title", "description", "time"), Build.StringArray("calendar_id = " + cal.id));
                foreach (List<object> evt in eventResult)
                {
                    Event e = new Event();
                    e.id = (int)evt[0];
                    e.calendarId = (int)evt[1];
                    e.title = (string)evt[2];
                    e.description = (string)evt[3];
                    e.time = (long)evt[4];
                    cal.events.Add(e);
                }
                project.calendars.Add(cal);
            }


            return View("Mock", project);
        }

        public ActionResult CreateEvent(string title, string description, string time)
        {
            List<string[]> values = new List<string[]>();
            values.Add(Build.StringArray(title, description, time));

            DataAccess.DataAccess.Create("Events", Build.StringArray("title, description, time"), values);
            return Redirect("Tester");
        }

        public ActionResult CalendarTest()
        {
            List<List<object>> result = DataAccess.DataAccess.Read(Build.StringArray("Calendars", "Events"), Build.StringArray("Events.title", "Events.description", "Events.time"), Build.StringArray("Calendars.project_id = 2", "Events.calendar_id = Calendars.id"));
            string rString = "";
            rString += result.Count + "<br />";
            Calendar calendar = new Calendar();
            calendar.events = new List<Event>();
            foreach (List<object> row in result)
            {
                Event e = new Event();
                e.title = (string)row[0];
                e.description = (string)row[1];
                e.time = (long)row[2];
                calendar.events.Add(e);
                rString += row.ToString() + "<br />";

                foreach (object col in row)
                {
                    rString += col.ToString() + "<br />";
                }
            }
            return View("Calendar", calendar);
        }

        public string Test()
        {

            return User.Identity.GetUserId();
        }

        public List<List<object>> SpecialTables()
        {
            List<string> tables = new List<string> { "Projects", "Events", "Messages", "Users" };
            List<List<object>> myList = new List<List<object>>();
            foreach (string table in tables)
            {
                myList.Add(new List<object> { table });
            }
            return myList;
        }

        public void TableSearch(object table, string keyword)
        {
            List<object> hitsByID = new List<object>();
            List<List<object>> columnslist = DataAccess.DataAccess.ListColumns(table.ToString());
            foreach (List<object> columns in columnslist)
            {
                foreach (object column in columns)
                {
                    List<List<object>> queryResults = DataAccess.DataAccess.Read(Build.StringArray(table.ToString()), null, Build.StringArray(column.ToString() + " LIKE '%" + keyword + "%'"));

                    foreach (List<object> row in queryResults)
                    { 
                        if (!hitsByID.Contains(row[0]))
                        {
                            int i = 0;
                            hitsByID.Add(row[0]);
                            foreach (object col in row)
                            {
                                i++;
                            }
                        }

                    }
                }
            }
        }

        public ActionResult Search(string keyword, List<string> tablesToSearch = null)
        {
            
            List<List<object>> tablesWillSearch = new List<List<object>>();

            if (tablesToSearch == null)
            {
                tablesWillSearch = DataAccess.DataAccess.ListTables();
            }
            else
            {
                foreach (string table in tablesToSearch)
                {
                    List<object> list = new List<object> { table };
                    tablesWillSearch.Add(list);
                }
            }

            foreach (dynamic tables in tablesWillSearch)
            {
                if (!(tables[0].ToString().StartsWith("Asp") || tables[0].ToString().StartsWith("__")))
                {
                    TableSearch(tables[0],keyword);
                }
            }
            
            return View();
        }

        public Chat Chat()
        {
            Chat chat = new Chat();
            List<List<object>> result = DataAccess.DataAccess.Read(Build.StringArray("messages"), Build.StringArray("*"));
            chat.messages = new List<Message>();

            foreach (List<object> row in result)
            {
                Message msg = new Message();
                msg.from = (string)row[1];
                msg.to = (string)row[2];
                msg.body = (string)row[3];
                msg.project = (string)row[4];
                msg.time = (DateTime)row[5];
                chat.messages.Add(msg);
            }
            return chat;
        }

        public ActionResult ChatRoom()
        {
            return View("ChatRoom", Chat());
        }

        public ActionResult CreateMessage()
        {
            return View("CreateMessage");
        }

        public ActionResult GenerateMessage(Chat chat)
        {
            if (chat.message.to == null)
            {
                chat.message.to = "No recipient specified";
            }

            if (chat.message.from == null)
            {
                chat.message.from = "no sender specified";
            }

            if (chat.message.body == null)
            {
                chat.message.body = "no text provided";
            }

            if (chat.message.project == null)
            {
                chat.message.project = "test";
            }

            InsertToMessageTable.UsingChatModel(chat);
            return View("ChatRoom", Chat());
        }
    }
}
