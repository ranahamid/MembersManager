using MembersManager.Models;
using MembersManager.Models.Entities;
using Services.MMLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MembersManager.Controllers
{
    public class RoleController : BaseController
    {
        // GET: Role
        public ActionResult Index()
        {
            return View();
        }

        // GET: Role/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Role/Create
        public ActionResult Create(int recipientId, string type)
        {
            ViewBag.RecipientId = recipientId;

            return View(new RoleViewModel() { RecipientId = recipientId, RoleType = type });
        }

        // POST: Role/Create
        [HttpPost]
        public ActionResult Create(int recipientId, string type, RoleViewModel model)
        {
            try
            {
                ViewBag.RecipientId = recipientId;
                if (ModelState.IsValid)
                {
                    if (type == "board")
                    {
                        _dbcontext.BoardMembers.Add(new BoardMember()
                        {
                            Company=model.Company,
                            Position=model.Position,
                            RecipientId= recipientId,
                            Created=DateTime.Now,
                            Updated=DateTime.Now

                        });
                    }
                    else if (type == "union")
                    {
                        _dbcontext.UnionMembers.Add(new UnionMember()
                        {
                            Company = model.Company,
                            Position = model.Position,
                            RecipientId = recipientId,
                            Created = DateTime.Now,
                            Updated = DateTime.Now
                        });
                    }
                    else
                    {
                        _dbcontext.ExternalMembers.Add(new ExternalMember()
                        {
                            Company = model.Company,
                            Position = model.Position,
                            RecipientId = recipientId,
                            Created = DateTime.Now,
                            Updated = DateTime.Now

                        });
                    }

                    _dbcontext.SaveChanges();

                    return RedirectToAction("details", "member", new { id = recipientId });
                }
                else
                {
                    return View();
                }
                
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                 
                return View();
            }
        }

        // GET: Role/Edit/5
        public ActionResult Edit(int recipientId, string type, int id)
        {
            ViewBag.RecipientId = recipientId;
            var role = new RoleViewModel();
            if (type == "board")
            {
                var r = _dbcontext.BoardMembers.Find(id);
                if (r != null)
                {
                    role.Id = r.Id;
                    role.Position = r.Position;
                    role.Company = r.Company;
                    role.RoleType = type;
                }
            }
            else if (type == "union")
            {
                var r = _dbcontext.UnionMembers.Find(id);
                if (r != null)
                {
                    role.Id = r.Id;
                    role.Position = r.Position;
                    role.Company = r.Company;
                    role.RoleType = type;
                }
            }
            else
            {
                var r = _dbcontext.ExternalMembers.Find(id);
                if (r != null)
                {
                    role.Id = r.Id;
                    role.Position = r.Position;
                    role.Company = r.Company;
                    role.RoleType = type;
                }
            }
            return View(role);
        }

        // POST: Role/Edit/5
        [HttpPost]
        public ActionResult Edit(int recipientId, string type, int id, RoleViewModel model)
        {
            try
            {
                ViewBag.RecipientId = recipientId;
                if (ModelState.IsValid)
                {
                    if (type == "board")
                    {
                        var r = _dbcontext.BoardMembers.Find(id);
                        if (r != null)
                        {
                            r.Position = model.Position;
                            r.Company = model.Company;
                            r.Updated = DateTime.Now;
                        }
                    }
                    else if (type == "union")
                    {
                        var r = _dbcontext.UnionMembers.Find(id);
                        if (r != null)
                        {
                            r.Position = model.Position;
                            r.Company = model.Company;
                            r.Updated = DateTime.Now;
                        }
                    }
                    else
                    {
                        var r = _dbcontext.ExternalMembers.Find(id);
                        if (r != null)
                        {
                            r.Position = model.Position;
                            r.Company = model.Company;
                            r.Updated = DateTime.Now;
                        }
                    }
                    _dbcontext.SaveChanges();
                    return RedirectToAction("details","member",new { id= recipientId });
                }
                else
                    return View(model);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return View(model);
            }
        }

        // GET: Role/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Role/Delete/5
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
