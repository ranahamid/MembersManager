using Services.MailUp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MembersManager.Controllers
{
    public class AdminController : BaseController
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DeleteMailupSubscriber()
        {
            if (ConfigurationManager.AppSettings["MailUpEnable"] == "1")
            {
                var mailUp = new MailUp(ConfigurationManager.AppSettings["MailUpClientId"],
                                    ConfigurationManager.AppSettings["MailUpClientSecret"],
                                    ConfigurationManager.AppSettings["MailUpCallbackUri"],ConfigurationManager.AppSettings["MailUpUser"], ConfigurationManager.AppSettings["MailUpPassword"]);
                if (mailUp.accessToken != null)
                {
                    mailUp.UnsubscribeAllFromList(ConfigurationManager.AppSettings["MailUpListId"]);
                }
            }
            return RedirectToAction("Index");
        }


        // GET: Admin/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Admin/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Admin/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Admin/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
