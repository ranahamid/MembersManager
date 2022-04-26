using MembersManager.Models;
using MembersManager.Models.Entities;
using Services.MailUp;
using Services.MMLogger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MembersManager.Controllers
{
    public class MemberController : BaseController
    {
        // GET: Member
        public ActionResult Index()
        {
            return View();
        }

        // GET: Member/Details/5
        //public ActionResult Details(int id)
        //{
        //    var model = _dbcontext.Recipients.Find(id);
        //    var recipientViewModel = new RecipientViewModel();
        //    if (model == null)
        //        return RedirectToAction("Index");
        //    else
        //    {
        //        return View(new RecipientViewModel()
        //        {
        //            Firstname = model.Firstname,
        //            Lastname = model.Lastname,
        //            Email = model.Email,
        //            Address = model.Address,
        //            Address2 = model.Address2,
        //            City = model.City,
        //            Country = model.Country,
        //            ExternalId = model.ExternalId,
        //            Postcode = (int)model.Postcode,
        //            Phone = model.Phone,
        //            Mobile = model.Mobile,
        //            Created = DateTime.Now,
        //            Updated = DateTime.Now
        //        });
        //    }

        //}
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Profile");
            }
            else
            {
                var recipient = _dbcontext.Recipients.Find(id);
                if (recipient == null)
                    return RedirectToAction("Index", "Profile");

                ViewBag.RecipientId = id;
                return View(GetMemberDetails(recipient));
            }
        }

        [HttpPost]
        public ActionResult Details(MemberProfileViewModel member)
        {
            ViewBag.RecipientId = member.Id;

            UnsubscribeRecepients(member.Id);

            if (member.SelectedClearAll != null && member.SelectedClearAll.ToLower() == "on")
            {
                var optIn = false;
                var result1 = UpdateToRecipientSegments(member.Id, optIn, "");
                if (result1)
                    ViewBag.Message = "You are unsubscribed from the app.";

                Log.Info(member.FirstName + " " + member.LastName + " are unsubscribed from the app.");
            }
            else
            {
                var sgmentList = new StringBuilder();
                var optIn = true;

                if (member.SelectedSegments != null)
                {
                    foreach (var sgment in member.SelectedSegments)
                    {
                        if (sgmentList.ToString() != string.Empty)
                        {
                            sgmentList.Append("," + sgment);
                        }
                        else
                        {
                            sgmentList.Append(sgment);
                        }
                    }
                }
                else
                    optIn = false;

                //subscribe
                if (string.IsNullOrEmpty(sgmentList.ToString()))
                    optIn = false;

                var result1 = UpdateToRecipientSegments(member.Id, optIn, sgmentList.ToString());

                //var result2 = SetProfileOptIn(member.Id, optIn);
                if (result1 && optIn == true)
                {
                    ViewBag.Message = "You are subscribed to the app.";
                    Log.Info(member.FirstName + " " + member.LastName + " are subscribed from the app.");
                }
                else
                {
                    ViewBag.Message = "You are unsubscribed to the app.";
                    Log.Info(member.FirstName + " " + member.LastName + " are unsubscribed from the app.");
                }

            }

            SubscribeRecepients(member.Id);

            return View(GetMemberDetails(_dbcontext.Recipients.Find(member.Id)));

        }

        // GET: Member/Create
        public ActionResult Create()
        {
            return View(new RecipientViewModel());
        }

        // POST: Member/Create
        [HttpPost]
        public ActionResult Create(RecipientViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _dbcontext.Recipients.Add(new Recipient()
                    {
                        Firstname = model.Firstname,
                        Lastname = model.Lastname,
                        Email = model.Email,
                        Address = model.Address,
                        Address2 = model.Address2,
                        City = model.City,
                        Country = model.Country,
                        Postcode = model.Postcode,
                        Phone = model.Phone,
                        Mobile = model.Mobile,
                        Created = DateTime.Now,
                        Updated = DateTime.Now
                    });
                    _dbcontext.SaveChanges();
                    return RedirectToAction("Index", "Profile");
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return View(model);
                }
            }
            return View(model);
        }
        // GET: Member/Create
        public ActionResult BoardMember()
        {
            return View();
        }

        // POST: Member/Create
        [HttpPost]
        public ActionResult BoardMember(FormCollection collection)
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

        // GET: Member/Edit/5
        public ActionResult Edit(int id)
        {
            var model = _dbcontext.Recipients.Find(id);
            //  var recipientViewModel = new RecipientViewModel();
            if (model == null)
                return RedirectToAction("Index", "Profile");
            else
            {
                ViewBag.RecipientId = id;
                var recipient = new RecipientViewModel();
                recipient.Firstname = model.Firstname;
                recipient.Lastname = model.Lastname;
                recipient.Email = model.Email;
                recipient.Address = model.Address;
                recipient.Address2 = model.Address2;
                recipient.City = model.City;
                recipient.Country = model.Country;
                recipient.Postcode = model.Postcode;
                recipient.Phone = model.Phone;
                recipient.Mobile = model.Mobile;
                recipient.Created = model.Created;
                recipient.Updated = model.Updated;
                return View(recipient);
            }
        }

        // POST: Member/Edit/5
        [HttpPost]
        public ActionResult Edit(RecipientViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ViewBag.RecipientId = model.Id;
                    var recipient = _dbcontext.Recipients.Find(model.Id);
                    if (recipient != null)
                    {
                        recipient.Firstname = model.Firstname;
                        recipient.Lastname = model.Lastname;
                        recipient.Email = model.Email;
                        recipient.Address = model.Address;
                        recipient.Address2 = model.Address2;
                        recipient.City = model.City;
                        recipient.Country = model.Country;
                        recipient.Postcode = model.Postcode;
                        recipient.Phone = model.Phone;
                        recipient.Mobile = model.Mobile;
                        recipient.Created = model.Created;
                        recipient.Updated = DateTime.Now;
                        _dbcontext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: Member/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Member/Delete/5
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


        private MemberProfileViewModel GetMemberDetails(Recipient profile, bool IsEmailSubsription = false)
        {
            var member = new MemberProfileViewModel();
            var dics = new Dictionary<string, object>();
            List<SelectListItem> Sgments = new List<SelectListItem>();
            if (profile != null)
            {
                var segment = new List<Segment>();
                if (IsEmailSubsription == true)
                    segment = _dbcontext.Segments.Where(x => x.Visible == true).ToList();
                else
                    segment = _dbcontext.Segments.ToList();

                member.UnSubscribeAll = new SelectListItem()
                {
                    Text = "Unsubscribe from all",
                    Value = "Unsubscribe",
                    //Selected = profile.OptOut == true,
                    Selected = profile.OptIn == false ? true : false,
                };

                member.ClearAll = new SelectListItem()
                {
                    Text = "Opt-out from all communication",
                    Value = "Opt-out",
                    Selected = profile.OptIn == false,
                };

                var SelectedSegments = _dbcontext.RecipientSegments.Where(x => x.RecipientId == profile.Id && (x.OptIn == null || x.OptIn == true)).Select(x => x.SegmentId).ToList();

                foreach (var item in segment)
                {
                    Sgments.Add(new SelectListItem
                    {
                        Text = item.Name,
                        Value = item.ID.ToString(),
                        Selected = SelectedSegments.Contains(item.ID),
                    });
                }

                member.Id = profile.Id;
                member.FirstName = profile.Firstname;
                member.LastName = profile.Lastname;

                var recipient = new RecipientViewModel()
                {
                    Firstname = profile.Firstname,
                    Lastname = profile.Lastname,
                    Email = profile.Email,
                    Address = profile.Address,
                    Address2 = profile.Address2,
                    City = profile.City,
                    Country = profile.Country,
                    Postcode = profile.Postcode,
                    Phone = profile.Phone,
                    Mobile = profile.Mobile,
                    Created = profile.Created,
                    Updated = profile.Updated
                };

                var columnNames = typeof(RecipientViewModel).GetProperties().Select(property => property.Name).ToArray();

                foreach (var c in columnNames)
                {
                    if (c == "PrimaryMembers" || c == "BoardMembers" || c == "UnionMembers" || c == "ExternalMembers" || c == "RecipientSegments")
                        continue;
                    dics.Add(c, recipient.GetType().GetProperty(c).GetValue(recipient));
                }

                member.BoardMemberRoles = _dbcontext.BoardMembers.Where(x => x.RecipientId == profile.Id).Select(s => new RoleViewModel()
                {
                    Id = s.Id,
                    Position = s.Position,
                    Company = s.Company
                }).ToList();
                member.BoardMemberRoles = _dbcontext.BoardMembers.Where(x => x.RecipientId == profile.Id).Select(s => new RoleViewModel()
                {
                    Id = s.Id,
                    Position = s.Position,
                    Company = s.Company
                }).ToList();

                member.UnionMemberRoles = _dbcontext.UnionMembers.Where(x => x.RecipientId == profile.Id).Select(s => new RoleViewModel()
                {
                    Id = s.Id,
                    Position = s.Position,
                    Company = s.Company
                }).ToList();

                member.ExternalMemberRoles = _dbcontext.ExternalMembers.Where(x => x.RecipientId == profile.Id).Select(s => new RoleViewModel()
                {
                    Id = s.Id,
                    Position = s.Position,
                    Company = s.Company
                }).ToList();
            }

            member.Segments = Sgments;
            member.ProfileInfo = dics;

            return member;
        }

        private bool UpdateToRecipientSegments(int Id, bool optIn, string segmentIds)
        {
            var recipientSegments = _dbcontext.RecipientSegments.Where(x => x.RecipientId == Id).ToList();

            if (string.IsNullOrEmpty(segmentIds))
            {
                foreach (var p in recipientSegments)
                {
                    p.OptIn = optIn;
                }

                var profile = _dbcontext.Recipients.Where(x => x.Id == Id).FirstOrDefault();
                profile.OptIn = optIn;
            }
            else
            {
                foreach (var p in recipientSegments)
                {
                    p.OptIn = false;
                }

                var segIds = segmentIds.Split(',').Select(int.Parse).ToList();

                foreach (var p in segIds)
                {
                    var recipientSegment = _dbcontext.RecipientSegments.Where(x => x.RecipientId == Id && x.SegmentId == p).FirstOrDefault();
                    if (recipientSegment == null)
                    {
                        _dbcontext.RecipientSegments.Add(new RecipientSegment()
                        {
                            SegmentId = p,
                            RecipientId = Id,
                            OptIn = optIn,
                            OptInDate = DateTime.Now
                        });

                    }
                    else
                    {
                        recipientSegment.OptIn = optIn;
                        if (optIn == true)
                        {
                            var profile = _dbcontext.Recipients.Where(x => x.Id == Id).FirstOrDefault();
                            profile.OptIn = optIn;
                        }
                    }
                }
            }

            try
            {
                _dbcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return false;
            }
        }

        private bool UpdateToRecipientSegments(int Id, int sgmentList, bool? Optin)
        {
            var profileDtils = _dbcontext.RecipientSegments.Where(x => x.RecipientId == Id).FirstOrDefault();
            if (profileDtils == null)
            {

                //insert
                RecipientSegment profileDetail = new RecipientSegment()
                {
                    RecipientId = Id,
                    SegmentId = sgmentList,
                    OptIn = Optin,
                    OptInDate = DateTime.Now,
                };
                _dbcontext.RecipientSegments.Add(profileDetail);
            }

            else
            {
                //update
                profileDtils.SegmentId = sgmentList;
                // if(Optin!=null)

                profileDtils.OptIn = Optin;


                profileDtils.OptInDate = DateTime.Now;
            }

            try
            {
                _dbcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return false;
            }
        }

        private bool SetProfileOptIn(int Id, bool? Optin)
        {
            var profile = _dbcontext.Recipients.Where(x => x.Id == Id).FirstOrDefault();
            profile.OptIn = Optin;

            try
            {
                _dbcontext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return false;
            }
        }

        private void SubscribeRecepients(int id)
        {
            if (ConfigurationManager.AppSettings["MailUpEnable"] == "1")
            {
                var mailUp = new MailUp(ConfigurationManager.AppSettings["MailUpClientId"],
                                    ConfigurationManager.AppSettings["MailUpClientSecret"],
                                    ConfigurationManager.AppSettings["MailUpCallbackUri"], ConfigurationManager.AppSettings["MailUpUser"], ConfigurationManager.AppSettings["MailUpPassword"]);
                if (mailUp.accessToken != null)
                {
                    var profileDetails = _dbcontext.RecipientSegments.Include("Profiles").Include("Segments").Where(x => x.RecipientId == id).ToList();
                    foreach (var pd in profileDetails)
                    {
                        if (pd.Segment != null && !string.IsNullOrEmpty(pd.Segment.MailUpGroupID) && !string.IsNullOrEmpty(pd.Recipient.MailUpID))
                        {
                            mailUp.SubscribeToGroup(ConfigurationManager.AppSettings["MailUpUser"], pd.Recipient.Email, pd.Segment.MailUpGroupID, false);
                        }
                    }
                }
            }
        }

        private void UnsubscribeRecepients(int id)
        {
            if (ConfigurationManager.AppSettings["MailUpEnable"] == "1")
            {
                var mailUp = new MailUp(ConfigurationManager.AppSettings["MailUpClientId"],
                                    ConfigurationManager.AppSettings["MailUpClientSecret"],
                                    ConfigurationManager.AppSettings["MailUpCallbackUri"], ConfigurationManager.AppSettings["MailUpUser"], ConfigurationManager.AppSettings["MailUpPassword"]);
                if (mailUp.accessToken != null)
                {
                    var profileDetails = _dbcontext.RecipientSegments.Include("Profiles").Include("Segments").Where(x => x.RecipientId == id).ToList();
                    foreach (var pd in profileDetails)
                    {
                        if (pd.Segment != null && !string.IsNullOrEmpty(pd.Segment.MailUpGroupID))
                        {
                            mailUp.UnsubscribeFromGroup(pd.Recipient.Email, pd.Segment.MailUpGroupID, ConfigurationManager.AppSettings["MailUpListId"]);
                        }
                    }
                }
            }
        }
    }
}
