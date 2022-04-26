using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using MembersManager.Models;
using MembersManager.Models.Entities;
using Services.MailUp;
using System.Configuration;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data.SqlClient;
using Services.MMLogger;

namespace MembersManager.Controllers
{

    public class ProfileController : BaseController
    {
        public ActionResult Index(int? id)
        {
            if (id == null || id < 1)
                return RedirectToAction("Index", "Home");
            var seg = _dbcontext.Segments.Find(id);
            if(seg==null)
                return RedirectToAction("Index", "Home");

            ProfileViewModel profileViewModel = new ProfileViewModel();
            List<ConditionSetViewModel> conditionList = new List<ConditionSetViewModel>();

            if (seg != null)
            {
                profileViewModel.SegmentId = seg.ID;
                profileViewModel.SegmentName = seg.Name;
                //Get Conditions
                var condition = _dbcontext.ConditionSets.Where(x => x.SegmentId == seg.ID);
                if (condition != null)
                {
                    foreach (var item in condition)
                    {
                        conditionList.Add(new ConditionSetViewModel
                        {
                            SelectColumn = item.ColumnName,
                            SelectFilter = item.FilterId,
                            SearchTerm = item.SearchTerm
                        });
                    }
                    _dbcontext.SaveChanges();
                }
                //end conditions
            }


            profileViewModel.conditionSet = conditionList;
            //drop down list
            profileViewModel.AllFilterOperatorSelectList = GetAllFilterOperatorSelectList();
            profileViewModel.AllBooleanSelectList = GetBooleanSelectList();
            profileViewModel.AllColumnSelectList = GetAllProfileSelectList();
            profileViewModel.AllFilterSelectList = GetAllFilterSelectList();

            profileViewModel.AllBooleanFilterSelectList = GetBooleanFilterSelectList();
            return View(profileViewModel);
        }

        [HttpPost]
        public ActionResult Index(ProfileViewModel model)
        {
            ProfileViewModel profileViewModel = new ProfileViewModel();
            try
            {               
                ModelState.Clear();
                List<ConditionSetViewModel> conditionList = new List<ConditionSetViewModel>();
                var columnType = typeof(Recipient).GetProperties().Select(property => property.PropertyType.FullName).ToArray();
                var columnNames = typeof(Recipient).GetProperties().Select(property => property.Name).ToArray();

                UnsubscribeAllRecepients(model.SegmentId);

                if (ModelState.IsValid && (model.SelectFilter != null || model.SelectBooleanFilter != null))
                {
                    DeleteFromConditionSet(model.SegmentId);
                    string term = string.Empty;
                    int i = 0;
                    if (string.IsNullOrEmpty(model.SelectColumn[0]))
                    {
                        string query = string.Format("{0} ", allQuery);
                        SaveToSegmentTbl(query);
                       // BulkSubscription(query, model.SegmentId);
                    }
                    else
                    {
                        string AllQuery = string.Empty;
                        bool isPrimaryolumns = false;
                        foreach (var SColumn in model.SelectColumn)
                        {
                            var clmName = model.SelectColumn[i];
                            if (string.IsNullOrEmpty(clmName)) continue;

                            if (model.SearchTerm[i] != null && model.SearchTerm[i] != null)
                            {
                                term = model.SearchTerm[i].Trim();
                            }
                            else if (model.BooleanTerm[i] != null && model.BooleanTerm[i] != string.Empty)
                            {
                                term = model.BooleanTerm[i];
                            }

                            string filterText = string.Empty;
                            if (model.SelectFilter[i] != null && model.SelectFilter[i] != string.Empty)
                            {
                                filterText = model.SelectFilter[i];
                            }
                            else if (model.SelectBooleanFilter[i] != null && model.SelectBooleanFilter[i] != string.Empty)
                            {
                                filterText = model.SelectBooleanFilter[i];
                            }



                            
                            if (clmName.Contains("MembersId") || clmName.Contains("MembersPosition") || clmName.Contains("MemberCompany"))
                            {
                                SaveToConditionSet(clmName, filterText, term, model.SegmentId);
                            }
                            else
                            {
                                isPrimaryolumns = true;
                                //GET column Names type
                                int j = 0;
                                foreach (var column in columnNames)
                                {
                                    if (clmName == column)
                                    {
                                        break;
                                    }
                                    j++;
                                }

                                var typeCol = columnType[j];

                                //GET Query
                                if (i == 0)
                                {
                                    AllQuery += GetSearchProfileMembers(clmName, typeCol, filterText, term, mainQuery);
                                }
                                else
                                {
                                    AllQuery += GetSearchProfileMembers(clmName, typeCol, filterText, term, andQuery);
                                }

                                //INSERT to condition db
                                SaveToConditionSet(clmName, filterText, term, model.SegmentId);
                            }
                            i++;
                        }
                        if (isPrimaryolumns == false)
                        {
                            SaveToSegmentTbl(GetDefaultQuery(model.SegmentId));

                        }else 
                        {
                            var finalyquery = AllQuery + GetSegmentQuery(model.SegmentId);
                            SaveToSegmentTbl(finalyquery);
                        }
                      //  BulkSubscription(finalyquery, model.SegmentId);
                        // await Task.Factory.StartNew(() => BulkSubscription(AllQuery, model.SegmentId));
                        //  await BulkSubscription(AllQuery, model.SegmentId);
                    }
                }
                else
                {
                    string query = string.Format("{0} ", allQuery);
                    SaveToSegmentTbl(query);
                   // BulkSubscription(query, model.SegmentId);
                    //  await Task.Factory.StartNew(() => BulkSubscription(query, model.SegmentId));
                }

                if (RouteData.Values["id"] != null)
                {
                    var seg = _dbcontext.Segments.Find(Convert.ToInt32("0" + RouteData.Values["id"]));
                    if (seg != null)
                    {
                        profileViewModel.SegmentId = seg.ID;
                        profileViewModel.SegmentName = seg.Name;
                        //Get Conditions
                        var condition = _dbcontext.ConditionSets.Where(x => x.SegmentId == seg.ID);
                        if (condition != null)
                        {
                            foreach (var item in condition)
                            {
                                conditionList.Add(new ConditionSetViewModel
                                {
                                    SelectColumn = item.ColumnName,
                                    SelectFilter = item.FilterId,
                                    SearchTerm = item.SearchTerm
                                });
                            }
                            _dbcontext.SaveChanges();
                        }
                        //end conditions
                        Log.Info("Condition Save Sucessfully");
                    }
                }

                BulkSubscription(model.SegmentId);

                profileViewModel.conditionSet = conditionList;

                profileViewModel.AllFilterOperatorSelectList = GetAllFilterOperatorSelectList();
                profileViewModel.AllBooleanFilterSelectList = GetBooleanFilterSelectList();
                profileViewModel.AllBooleanSelectList = GetBooleanSelectList();
                profileViewModel.AllColumnSelectList = GetAllProfileSelectList();
                profileViewModel.AllFilterSelectList = GetAllFilterSelectList();
            }            
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return View(profileViewModel);
        }

        [HttpPost]
        public ActionResult LoadData(int? id)
        {
            IQueryable<Recipient> profile;
            
            if (id != null)
            {
                var segment = _dbcontext.Segments.Where(x => x.ID == id).FirstOrDefault();
                if (segment != null && !string.IsNullOrEmpty(segment.Query))
                {
                    profile = _dbcontext.Recipients.SqlQuery(segment.Query).AsQueryable();
                                        
                    var cs = segment.ConditionSets.Where(x => x.ColumnName.Contains("MembersId") || x.ColumnName.Contains("MembersPosition") || x.ColumnName.Contains("MemberCompany")).ToList();
                    if (cs.Count > 0)
                        profile = GetProfileByRole(profile, cs);

                }
                else
                {
                    profile = GetAllProfileData();
                }
            }
            else
            {
                profile = GetAllProfileData();
            }

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                profile = profile.OrderBy(x => sortColumn + " " + sortColumnDir);
            }

            totalRecords = profile.Count();
            var data = profile.Skip(skip).Take(pageSize).Select(s => new ProfileListViewModel()
            {
                Id = s.Id,
                Email = s.Email,
                MailUpID = s.MailUpID,
                OptIn = s.OptIn,
                Deleted = s.Deleted,
                Created = s.Created,
                Updated = s.Updated,
                Firstname = s.Firstname,
                Lastname = s.Lastname,
                Address = s.Address,
                Address2 = s.Address2,
                Postcode = s.Postcode,
                City = s.City,
                Country = s.Country,
                Phone = s.Phone,
                Mobile = s.Mobile
               
            }).ToList();
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);
        }

        private IQueryable<Recipient> GetProfileByRole(IQueryable<Recipient> profile, List<ConditionSet> conditionSets)
        {
          //  IQueryable<Recipient> profile1 = Enumerable.Empty<Recipient>().AsQueryable();
            List<Recipient> profile1 = new List<Recipient>();
            var boardIds = new List<int>();
            var unionIds = new List<int>();
            var externalIds = new List<int>();
            var boardQuery = "";
            var unionQuery = "";
            var externalQuery = "";
            var q = "";
            foreach (var c in conditionSets)
            {
                if (c.ColumnName.Contains("BoardMembers"))
                {
                    boardQuery += GetMemberRoleQuery(c.ColumnName.Replace("BoardMembers", ""), "string", c.FilterId, c.SearchTerm, "");
                }
                if (c.ColumnName.Contains("UnionMembers"))
                {
                    unionQuery += GetMemberRoleQuery(c.ColumnName.Replace("UnionMembers", ""), "string", c.FilterId, c.SearchTerm, "");
                }
                if (c.ColumnName.Contains("ExternalMembers"))
                {
                    externalQuery += GetMemberRoleQuery(c.ColumnName.Replace("ExternalMembers", ""), "string", c.FilterId, c.SearchTerm, "");
                }
            }

            if (!string.IsNullOrEmpty(boardQuery))
            {
                boardIds.AddRange(_dbcontext.BoardMembers.SqlQuery("select* from BoardMembers as prof where" + boardQuery).ToList().Select(x => x.Id));
            }
            if (!string.IsNullOrEmpty(unionQuery))
            {
                unionIds.AddRange(_dbcontext.UnionMembers.SqlQuery("select* from UnionMembers as prof where" + boardQuery).ToList().Select(x => x.Id));
                // unionQuery = "select* from BoardMembers as prof" + unionQuery;
            }
            if (!string.IsNullOrEmpty(externalQuery))
            {
                externalIds.AddRange(_dbcontext.ExternalMembers.SqlQuery("select* from ExternalMembers as prof where" + externalQuery).ToList().Select(x => x.Id));
                // externalQuery = "select* from BoardMembers as prof" + externalQuery;
            }

            foreach (var pid in boardIds)
            {
                profile1.AddRange(profile.Where(x => x.BoardMembers.Select(s => s.Id).ToList().Contains(pid)));
            }


            return profile1.AsQueryable();
        }

        public string GetMemberRoleQuery(string SelectColumn, string typeCol, string SelectFilter, dynamic SearchTerm, string firstQuery)
        {
            //check date time
            if ((SelectColumn == "Created" || SelectColumn == "Updated") && (SelectFilter != "11" || SelectFilter != "12"))
            {
                if (SearchTerm != "null" && SearchTerm != "")
                    SearchTerm = DateTime.Parse(SearchTerm);
                else
                    SearchTerm = null;
            }
            bool isInt = false;
            bool isString = false;
            bool isDateTime = false;
            bool isBool = false;
            if (typeCol.Contains("Int32"))
                isInt = true;
            else if (typeCol.Contains("String"))
                isString = true;
            else if (typeCol.Contains("DateTime"))
                isDateTime = true;
            else if (typeCol.Contains("Boolean"))
                isBool = true;



            string Query = string.Empty;

            if (SelectFilter == "1")
            {
                if (isInt)
                    Query = string.Format("{0} prof.{1} = {2} ", firstQuery, SelectColumn, SearchTerm);
                else
                    //equal
                    Query = string.Format("{0} prof.{1} = '{2}' ", firstQuery, SelectColumn, SearchTerm);
            }
            else if (SelectFilter == "2")
            {
                if (isInt)
                    Query = string.Format("{0} prof.{1} != {2} ", firstQuery, SelectColumn, SearchTerm);
                else
                    //not equal
                    Query = string.Format("{0} prof.{1} != '{2}' ", firstQuery, SelectColumn, SearchTerm);
            }
            else if (SelectFilter == "3")
            {
                //in
                string[] words = SearchTerm.Split(',');
                string terms = string.Empty;
                var index = 0;
                foreach (string word in words)
                {
                    if (index > 0)
                    {
                        terms = terms + ",";
                    }
                    index++;
                    if (isInt)
                        terms = terms + string.Format("{0}", word);
                    else
                        terms = terms + string.Format("'{0}'", word);
                }
                Query = string.Format("{0} prof.{1} IN ({2}) ", firstQuery, SelectColumn, terms);
            }
            else if (SelectFilter == "4")
            {
                //not in
                string[] words = SearchTerm.Split(',');
                string terms = string.Empty;
                var index = 0;
                foreach (string word in words)
                {
                    if (index > 0)
                    {
                        terms = terms + ",";
                    }
                    index++;
                    if (isInt)
                        terms = terms + string.Format("{0}", word);
                    else
                        terms = terms + string.Format("'{0}'", word);
                }
                Query = string.Format("{0} prof.{1} NOT IN ({2}) ", firstQuery, SelectColumn, terms);
            }
            else if (SelectFilter == "5")
            {
                //less
                if (isInt)
                    Query = string.Format("{0} prof.{1} < {2} ", firstQuery, SelectColumn, SearchTerm);
                else
                    Query = string.Format("{0} prof.{1} < '{2}' ", firstQuery, SelectColumn, SearchTerm);
            }

            else if (SelectFilter == "6")
            {
                //less or equal
                if (isInt)
                    Query = string.Format("{0} prof.{1} <= {2} ", firstQuery, SelectColumn, SearchTerm);
                else
                    Query = string.Format("{0} prof.{1} <= '{2}' ", firstQuery, SelectColumn, SearchTerm);
            }
            else if (SelectFilter == "7")
            {
                //greater
                if (isInt)
                    Query = string.Format("{0} prof.{1} > {2} ", firstQuery, SelectColumn, SearchTerm);
                else
                    Query = string.Format("{0} prof.{1} > '{2}' ", firstQuery, SelectColumn, SearchTerm);
            }
            else if (SelectFilter == "8")
            {
                //greater or equal
                if (isInt)
                    Query = string.Format("{0} prof.{1} >= '{2}' ", firstQuery, SelectColumn, SearchTerm);
                else
                    Query = string.Format("{0} prof.{1} >= '{2}' ", firstQuery, SelectColumn, SearchTerm);
            }

            else if (SelectFilter == "9")
            {
                //between
                string[] words = SearchTerm.Split(',');
                string term1 = (words[0] != null) ? words[0] : string.Empty;
                string term2 = (words[1] != null) ? words[1] : words[0];
                if (isInt)
                    Query = string.Format("{0} prof.{1} BETWEEN {2} AND {3} ", firstQuery, SelectColumn, term1, term2);
                else
                    Query = string.Format("{0} prof.{1} BETWEEN '{2}' AND '{3}' ", firstQuery, SelectColumn, term1, term2);
            }
            else if (SelectFilter == "10")
            {
                //not between
                string[] words = SearchTerm.Split(',');
                string term1 = (words[0] != null) ? words[0] : string.Empty;
                string term2 = (words[1] != null) ? words[1] : words[0];
                if (isInt)
                    Query = string.Format("{0} prof.{1} NOT BETWEEN {2} AND {3} ", firstQuery, SelectColumn, term1, term2);
                else
                    Query = string.Format("{0} prof.{1} NOT BETWEEN '{2}' AND '{3}' ", firstQuery, SelectColumn, term1, term2);
            }
            else if (SelectFilter == "11")
            {
                //IS NULL               
                Query = string.Format("{0} prof.{1} IS NULL ", firstQuery, SelectColumn);
            }
            else if (SelectFilter == "12")
            {
                //IS NOT NULL
                Query = string.Format("{0} prof.{1} IS NOT NULL ", firstQuery, SelectColumn);
            }
            else
            {
                Query = string.Format("{0} ", firstQuery);
            }


            return Query;
        }

        public ActionResult Details(int? id)
        {
            return View(GetMemberDetails(_dbcontext.Recipients.Find(id)));
        }

        [HttpPost]
        public ActionResult Details(MemberProfileViewModel member)
        {
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

        public ActionResult ManageEmailSubsription(string email)
        {
            return View(GetMemberDetails(_dbcontext.Recipients.Where(x => x.Email == email).FirstOrDefault(),true));
        }

        [HttpPost]
        public ActionResult ManageEmailSubsription(MemberProfileViewModel member)
        {
            UnsubscribeRecepients(member.Id);

            if ((member.SelectedClearAll != null && member.SelectedClearAll.ToLower() == "on") || (member.SelectedUnSubscribe != null && member.SelectedUnSubscribe.ToLower() == "on"))
            {
                var optIn = false;
                var result1 = UpdateToRecipientSegments(member.Id, optIn, "");
                if (result1 && member.SelectedUnSubscribe.ToLower() == "on")
                {
                    ViewBag.Message = "You are unsubscribed from all segments.";
                    Log.Info(member.FirstName + " " + member.LastName + " are unsubscribed from all segments.");
                }
                else
                {
                    ViewBag.Message = "You are unsubscribed from the app.";
                    Log.Info(member.FirstName + " " + member.LastName + " are unsubscribed from the app.");
                }
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
                    Log.Info(member.FirstName + " " + member.LastName + " are subscribed to the app.");
                }
                else
                {
                    ViewBag.Message = "You are unsubscribed to the app.";
                    Log.Info(member.FirstName + " " + member.LastName + " are unsubscribed from the app.");
                }
            }            

            SubscribeRecepients(member.Id);

            return View(GetMemberDetails(_dbcontext.Recipients.Find(member.Id),true));

        }

       
        public ActionResult Create()
        {
            return View();
        }

        // GET: Segment/Create
        [HttpPost]
        public ActionResult Create(SegmentViewModels model)
        {

            return View();
        }

        // GET: Segment/Edit/5
        public ActionResult Edit(string id)
        {

            return View();
        }

        // GET: Segment/Delete/5
        public ActionResult Delete(string id)
        {

            return View();
        }

        protected override void Dispose(bool disposing)
        {

            base.Dispose(disposing);
        }

        private MemberProfileViewModel GetMemberDetails(Recipient profile, bool IsEmailSubsription=false)
        {
            var member = new MemberProfileViewModel();
            var dics = new Dictionary<string, object>();
            List<SelectListItem> Sgments = new List<SelectListItem>();
            if (profile != null)
            {
                var segment = new List<Segment>();
                if (IsEmailSubsription == true)
                    segment = _dbcontext.Segments.Where(x=>x.Visible==true).ToList();
                else
                    segment = _dbcontext.Segments.ToList();

                member.UnSubscribeAll = new SelectListItem()
                {
                    Text = "Unsubscribe from all",
                    Value = "Unsubscribe",
                    //Selected = profile.OptOut == true,
                    Selected = profile.OptIn==false?true:false,
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
                var columnNames = typeof(Recipient).GetProperties().Select(property => property.Name).ToArray();

                foreach (var c in columnNames)
                {
                    if (c == "PrimaryMembers" || c == "BoardMembers" || c == "UnionMembers" || c == "ExternalMembers" || c == "RecipientSegments")
                        continue;
                    dics.Add(c, profile.GetType().GetProperty(c).GetValue(profile));
                }
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


        #region DownloadSegment

        [HttpGet]
        public ActionResult DownloadSegmentProfiles(int txtId)
        {
            ExcelPackage excel = new ExcelPackage();
            var segment = _dbcontext.Segments.Find(txtId);
            try
            {              
                var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                FormateExcelFile(workSheet);
                var columnNames = typeof(Recipient).GetProperties().Select(property => property.Name).ToArray();
                SetExcelColumns(columnNames, workSheet);

                var dataList = new List<object[]>();
                var profiles = _dbcontext.Recipients.SqlQuery(segment == null ? allQuery : segment.Query).ToList();

                foreach (var profile in profiles)
                {
                    dataList.Add(new object[]   {
                    profile.Id,
                    profile.Email,
                    profile.MailUpID,
                    profile.OptIn,
                    profile.Deleted,
                    profile.Created,
                    profile.Updated,
                    profile.Firstname,
                    profile.Lastname,
                    profile.Address,
                    profile.Address2,
                    profile.Postcode,
                    profile.City,
                    profile.Country,
                    profile.Phone,
                    profile.Mobile
                    //,
                    //profile.CVRnummer,
                    //profile.BrugerID,
                    //profile.Medlemsstatus,
                    //profile.Foreningsnummer,
                    //profile.Foedselsaar,
                    //profile.HektarDrevet,
                    //profile.AntalAndetKvaeg,
                    //profile.AntalAmmekoeer,
                    //profile.AntaMalkekoeer,
                    //profile.AntalSlagtesvin,
                    //profile.AntalSoeer,
                    //profile.AntalAarssoeer,
                    //profile.AntalPelsdyr,
                    //profile.AntalHoens,
                    //profile.AntalKyllinger,
                    //profile.Ecology,
                    //profile.Sektion_SSJ,
                    //profile.Driftform_planteavl,
                    //profile.Driftform_Koed_Koer,
                    //profile.Driftform_Mælk,
                    //profile.Driftform_Svin,
                    //profile.Driftform_Pelsdyr,
                    //profile.Driftform_Aeg_Kylling,
                    //profile.Driftstoerrelse_Planteavl,
                    //profile.Driftstoerrelse_Koed_Koer,
                    //profile.Driftfstoerrelse_Mælk,
                    //profile.Driftstoerrelse_Svin,
                    //profile.Driftstoerrelse_Pelsdyr,
                    //profile.Driftstoerrelse_Aeg_Kylling,
                    //profile.AntalSlagtekvaeg
                });
                }

                WriteDataInExcel(dataList, workSheet);
                Log.Info("Excell file downloaded");

            }catch(Exception ex)
            {
                Log.Error(ex);
            }

            return File(excel.GetAsByteArray(), "text/csv", "Segment_" + (segment == null ? "All" : segment.Name) + "_Profiles_" + DateTime.Today.ToString("mm-dd-YY") + ".xlsx");
        }

        private void FormateExcelFile(ExcelWorksheet workSheet)
        {
            workSheet.TabColor = System.Drawing.Color.Black;
            workSheet.DefaultRowHeight = 12;
            workSheet.Row(1).Height = 20;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(1).Style.Font.Bold = true;
        }

        private void SetExcelColumns(string[] columns, ExcelWorksheet workSheet)
        {
            for (var i = 0; i < columns.Length - 1; i++)
            {
                workSheet.Cells[1, i + 1].Value = columns[i];
                workSheet.Column(i + 1).AutoFit();
            }
        }

        private void WriteDataInExcel(List<object[]> datas, ExcelWorksheet workSheet)
        {
            var recordIndex = 2;
            foreach (var d in datas)
            {
                for (var i = 0; i < d.Length - 1; i++)
                {
                    workSheet.Cells[recordIndex, i + 1].Value = d[i];
                }
                recordIndex++;
            }
        }

        #endregion

        public void BulkSubscription(int segmentId)
        {
            try
            {
                if (segmentId > 0)
                {
                    //    var profiles = _dbcontext.Profiles.SqlQuery(query).ToList();
                    //    //_dbcontext.Database.ExecuteSqlCommand("delete ProfileDetails where SegmentId=" + segmentId);

                    //    foreach (var p in profiles)
                    //    {
                    //        _dbcontext.Database.ExecuteSqlCommand("insert into ProfileDetails(ProfileId,SegmentId,OptIn,OptInDate) values(" + p.Id + "," + segmentId + ",1,GetDate())");
                    //    }



                    //   var s = "";
                    //   if (query.TrimEnd() == "SELECT * FROM Profiles as prof ORDER BY  Id")
                    //   {
                    //       query = query.Replace("SELECT * FROM", "SELECT  distinct(prof.id) as Id FROM");
                    //       _dbcontext.Database.ExecuteSqlCommand("UpdateRecipientSegments @SegmentId, @OptIn, @Query, @isDefault",
                    //new SqlParameter("@SegmentId", segmentId),
                    //             new SqlParameter("@OptIn",  1),
                    //             new SqlParameter("@isDefault", 1),
                    //             new SqlParameter("@Query", query));

                    //   }
                    //   else
                    //   {
                    //       query = query.Replace("SELECT * FROM", "SELECT  distinct(prof.id) as Id FROM");
                    //       _dbcontext.Database.ExecuteSqlCommand("UpdateRecipientSegments @SegmentId, @OptIn, @Query,@isDefault",
                    //            new SqlParameter("@SegmentId", SqlDbType.Int, segmentId),
                    //             new SqlParameter("@OptIn", SqlDbType.Bit, 1),
                    //             new SqlParameter("@isDefault", SqlDbType.Bit,0),
                    //             new SqlParameter("@Query", query));


                    //   }
                    //List<ProfileDetail> SelectedProfiles = new List<ProfileDetail>();
                    // _dbcontext.Database.ExecuteSqlCommand("update ProfileDetails set IsValid=0 where SegmentId="+ segmentId);
                    //_dbcontext.Database.ExecuteSqlCommand(s);

                    //query = query.Replace("SELECT * FROM", "SELECT prof.id as Id FROM");
                    //query = query.Replace("ORDER BY prof.Id", "");

                    _dbcontext.Database.ExecuteSqlCommand("UpdateRecipientSegments @SegmentId",
                    new SqlParameter("@SegmentId", segmentId));
                    //new SqlParameter("@OptIn", 1),
                    //new SqlParameter("@Query", query));
                    //   _dbcontext.Database.ExecuteSqlCommand("update ProfileDetails set OptIn=0 where IsValid=0 and SegmentId=" + segmentId);
                    Log.Info("Subscription all members successfull");
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex);
            }
        }
        public bool SetProfileMembersOptIn(int ProfileId, bool? optIn)
        {
            var profile = _dbcontext.Recipients.Where(x => x.Id == ProfileId).FirstOrDefault();
            if (profile != null)
            {
                profile.OptIn = optIn;
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

        private void UnsubscribeAllRecepients(int segmentid)
        {
            try
            {
                if (ConfigurationManager.AppSettings["MailUpEnable"] == "1")
                {
                    var mailUp = new MailUp(ConfigurationManager.AppSettings["MailUpClientId"],
                                    ConfigurationManager.AppSettings["MailUpClientSecret"],
                                    ConfigurationManager.AppSettings["MailUpCallbackUri"], ConfigurationManager.AppSettings["MailUpUser"], ConfigurationManager.AppSettings["MailUpPassword"]);
                    if (mailUp.accessToken != null)
                    {
                        var segments = _dbcontext.Segments.Where(x => x.ID == segmentid).FirstOrDefault();
                        if (segments != null && !string.IsNullOrEmpty(segments.MailUpGroupID))
                        {
                            mailUp.UnsubscribeAllRecepientFromGroup(segments.MailUpGroupID, ConfigurationManager.AppSettings["MailUpListId"]);
                            Log.Info("Unsubscribe All Recepient From Group: " + segments.Name);
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public void DeleteFromConditionSet(int SegmentId)
        {
            var condition = _dbcontext.ConditionSets.Where(x => x.SegmentId == SegmentId);
            if (condition != null)
            {
                foreach (var item in condition)
                {
                    _dbcontext.ConditionSets.Remove(item);
                }
                _dbcontext.SaveChanges();
            }
        }

        public void SaveToConditionSet(string SelectColumn, string filterText, string term, int SegmentId)
        {
            ConditionSet condition = new ConditionSet()
            {
                SegmentId = SegmentId,
                ColumnName = SelectColumn,
                FilterId = filterText,
                SearchTerm = term
            };
            _dbcontext.ConditionSets.Add(condition);
            _dbcontext.SaveChanges();
        }

        public string GetSearchProfileMembers(string SelectColumn, string typeCol, string SelectFilter, dynamic SearchTerm, string firstQuery)
        {
            //check date time
            if ((SelectColumn == "Created" || SelectColumn == "Updated") && (SelectFilter != "11" || SelectFilter != "12"))
            {
                if (SearchTerm != "null" && SearchTerm != "")
                    SearchTerm = DateTime.Parse(SearchTerm);
                else
                    SearchTerm = null;
            }
            bool isInt = false;
            bool isString = false;
            bool isDateTime = false;
            bool isBool = false;
            if (typeCol.Contains("Int32"))
                isInt = true;
            else if (typeCol.Contains("String"))
                isString = true;
            else if (typeCol.Contains("DateTime"))
                isDateTime = true;
            else if (typeCol.Contains("Boolean"))
                isBool = true;



            string Query = string.Empty;

            if (SelectFilter == "1")
            {
                if (isInt)
                    Query = string.Format("{0} prof.{1} = {2} ", firstQuery, SelectColumn, SearchTerm);
                else
                    //equal
                    Query = string.Format("{0} prof.{1} = '{2}' ", firstQuery, SelectColumn, SearchTerm);
            }
            else if (SelectFilter == "2")
            {
                if (isInt)
                    Query = string.Format("{0} prof.{1} != {2} ", firstQuery, SelectColumn, SearchTerm);
                else
                    //not equal
                    Query = string.Format("{0} prof.{1} != '{2}' ", firstQuery, SelectColumn, SearchTerm);
            }
            else if (SelectFilter == "3")
            {
                //in
                string[] words = SearchTerm.Split(',');
                string terms = string.Empty;
                var index = 0;
                foreach (string word in words)
                {
                    if (index > 0)
                    {
                        terms = terms + ",";
                    }
                    index++;
                    if (isInt)
                        terms = terms + string.Format("{0}", word);
                    else
                        terms = terms + string.Format("'{0}'", word);
                }
                Query = string.Format("{0} prof.{1} IN ({2}) ", firstQuery, SelectColumn, terms);
            }
            else if (SelectFilter == "4")
            {
                //not in
                string[] words = SearchTerm.Split(',');
                string terms = string.Empty;
                var index = 0;
                foreach (string word in words)
                {
                    if (index > 0)
                    {
                        terms = terms + ",";
                    }
                    index++;
                    if (isInt)
                        terms = terms + string.Format("{0}", word);
                    else
                        terms = terms + string.Format("'{0}'", word);
                }
                Query = string.Format("{0} prof.{1} NOT IN ({2}) ", firstQuery, SelectColumn, terms);
            }
            else if (SelectFilter == "5")
            {
                //less
                if (isInt)
                    Query = string.Format("{0} prof.{1} < {2} ", firstQuery, SelectColumn, SearchTerm);
                else
                    Query = string.Format("{0} prof.{1} < '{2}' ", firstQuery, SelectColumn, SearchTerm);
            }

            else if (SelectFilter == "6")
            {
                //less or equal
                if (isInt)
                    Query = string.Format("{0} prof.{1} <= {2} ", firstQuery, SelectColumn, SearchTerm);
                else
                    Query = string.Format("{0} prof.{1} <= '{2}' ", firstQuery, SelectColumn, SearchTerm);
            }
            else if (SelectFilter == "7")
            {
                //greater
                if (isInt)
                    Query = string.Format("{0} prof.{1} > {2} ", firstQuery, SelectColumn, SearchTerm);
                else
                    Query = string.Format("{0} prof.{1} > '{2}' ", firstQuery, SelectColumn, SearchTerm);
            }
            else if (SelectFilter == "8")
            {
                //greater or equal
                if (isInt)
                    Query = string.Format("{0} prof.{1} >= '{2}' ", firstQuery, SelectColumn, SearchTerm);
                else
                    Query = string.Format("{0} prof.{1} >= '{2}' ", firstQuery, SelectColumn, SearchTerm);
            }

            else if (SelectFilter == "9")
            {
                //between
                string[] words = SearchTerm.Split(',');
                string term1 = (words[0] != null) ? words[0] : string.Empty;
                string term2 = (words[1] != null) ? words[1] : words[0];
                if (isInt)
                    Query = string.Format("{0} prof.{1} BETWEEN {2} AND {3} ", firstQuery, SelectColumn, term1, term2);
                else
                    Query = string.Format("{0} prof.{1} BETWEEN '{2}' AND '{3}' ", firstQuery, SelectColumn, term1, term2);
            }
            else if (SelectFilter == "10")
            {
                //not between
                string[] words = SearchTerm.Split(',');
                string term1 = (words[0] != null) ? words[0] : string.Empty;
                string term2 = (words[1] != null) ? words[1] : words[0];
                if (isInt)
                    Query = string.Format("{0} prof.{1} NOT BETWEEN {2} AND {3} ", firstQuery, SelectColumn, term1, term2);
                else
                    Query = string.Format("{0} prof.{1} NOT BETWEEN '{2}' AND '{3}' ", firstQuery, SelectColumn, term1, term2);
            }
            else if (SelectFilter == "11")
            {
                //IS NULL               
                Query = string.Format("{0} prof.{1} IS NULL ", firstQuery, SelectColumn);
            }
            else if (SelectFilter == "12")
            {
                //IS NOT NULL
                Query = string.Format("{0} prof.{1} IS NOT NULL ", firstQuery, SelectColumn);
            }
            else
            {
                Query = string.Format("{0} ", firstQuery);
            }


            return Query;
        }

        public void SaveToSegmentTbl(string Query)
        {
            if (Query != string.Empty)
            {
                //db add
                if (RouteData.Values["id"] == null)
                {
                    Segment segment = new Segment()
                    {
                        Name = DateTime.Now.ToString(), //need to initialize
                        Query = Query,
                        MailUpGroupID = null,
                    };
                    _dbcontext.Segments.Add(segment);
                }
                else
                {
                    //update
                    var seg = _dbcontext.Segments.Find(Convert.ToInt32("0" + RouteData.Values["id"]));
                    if (seg != null)
                    {
                        seg.Query = Query;
                    }
                }
                try
                {
                    _dbcontext.SaveChanges();
                    Log.Info("Query Save successfull");
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }

        public string GetQueryFromSegment(int? id)
        {
            var statement = _dbcontext.Segments.Where(x => x.ID == id).FirstOrDefault();
            if (statement != null && statement.Query != null)
            {
                return statement.Query;
            }
            return null;
        }

        public IQueryable<Recipient> GetAllProfileData()
        {
            string query = string.Format("{0} ", allQuery);
            return _dbcontext.Recipients.SqlQuery(query).AsQueryable();

        }

        public List<SelectListItem> GetAllProfileSelectList()
        {
            List<SelectListItem> entities = new List<SelectListItem>();
            var columnNames = typeof(Recipient).GetProperties().Select(property => property.Name).ToArray();
            var group = new SelectListGroup { Name = "Primary Member" };
            foreach (var column in columnNames)
            {
                if (column.ToString() == "PrimaryMembers" || column.ToString() == "RecipientSegments")
                    continue;
                
                if (column.ToString() == "BoardMembers")
                {
                    group = new SelectListGroup { Name = "Board Member" };
                    entities.Add(new SelectListItem() { Value = "BoardMembersId", Text = "Id", Group= group });
                    entities.Add(new SelectListItem() { Value = "BoardMembersPosition", Text = "Position", Group = group });
                    entities.Add(new SelectListItem() { Value = "BoardMemberCompany", Text = "Company", Group = group });
                }
                else if (column.ToString() == "UnionMembers")
                {
                    group = new SelectListGroup { Name = "Union Member" };
                    entities.Add(new SelectListItem() { Value = "UnionMembersId", Text = "Id", Group = group });
                    entities.Add(new SelectListItem() { Value = "UnionMembersPosition", Text = "Position", Group = group });
                    entities.Add(new SelectListItem() { Value = "UnionMemberCompany", Text = "Company", Group = group });

                }
                else if (column.ToString() == "ExternalMembers")
                {
                    group = new SelectListGroup { Name = "External Member" };
                    entities.Add(new SelectListItem() { Value = "ExternalMembersId", Text = "Id", Group = group });
                    entities.Add(new SelectListItem() { Value = "ExternalMembersPosition", Text = "Position", Group = group });
                    entities.Add(new SelectListItem() { Value = "ExternalMemberCompany", Text = "Company", Group = group });
                }
                else
                {
                    entities.Add(new SelectListItem() { Value = column.ToString(), Text = column.ToString(), Group = group });
                }
            }
            return entities;
        }

        public List<SelectListItem> GetBooleanSelectList()
        {
            List<SelectListItem> entities = new List<SelectListItem>();

            entities.Add(new SelectListItem() { Text = "True", Value = "0" });
            entities.Add(new SelectListItem() { Text = "False", Value = "1" });

            return entities;
        }

        public List<SelectListItem> GetAllFilterSelectList()
        {
            List<SelectListItem> entities = new List<SelectListItem>();

            entities.Add(new SelectListItem() { Text = "equal", Value = "1" });
            entities.Add(new SelectListItem() { Text = "not equal", Value = "2" });
            entities.Add(new SelectListItem() { Text = "in", Value = "3" });
            entities.Add(new SelectListItem() { Text = "not in", Value = "4" });
            entities.Add(new SelectListItem() { Text = "less", Value = "5" });
            entities.Add(new SelectListItem() { Text = "less or equal", Value = "6" });
            entities.Add(new SelectListItem() { Text = "greater", Value = "7" });
            entities.Add(new SelectListItem() { Text = "greater or equal", Value = "8" });
            entities.Add(new SelectListItem() { Text = "between", Value = "9" });
            entities.Add(new SelectListItem() { Text = "not between", Value = "10" });
            entities.Add(new SelectListItem() { Text = "is null", Value = "11" });
            entities.Add(new SelectListItem() { Text = "is not null", Value = "12" });

            return entities;
        }

        public List<SelectListItem> GetAllFilterOperatorSelectList()
        {
            List<SelectListItem> entities = new List<SelectListItem>();

            entities.Add(new SelectListItem() { Text = "=", Value = "1" });
            entities.Add(new SelectListItem() { Text = "!=", Value = "2" });
            entities.Add(new SelectListItem() { Text = "in", Value = "3" });
            entities.Add(new SelectListItem() { Text = "not in", Value = "4" });
            entities.Add(new SelectListItem() { Text = "<", Value = "5" });
            entities.Add(new SelectListItem() { Text = "<=", Value = "6" });
            entities.Add(new SelectListItem() { Text = ">", Value = "7" });
            entities.Add(new SelectListItem() { Text = ">=", Value = "8" });
            entities.Add(new SelectListItem() { Text = "between", Value = "9" });
            entities.Add(new SelectListItem() { Text = "not between", Value = "10" });
            entities.Add(new SelectListItem() { Text = "= NULL", Value = "11" });
            entities.Add(new SelectListItem() { Text = "!= NULL", Value = "12" });

            return entities;
        }

        public List<SelectListItem> GetBooleanFilterSelectList()
        {
            List<SelectListItem> entities = new List<SelectListItem>();

            entities.Add(new SelectListItem() { Text = "equal", Value = "1" });
            entities.Add(new SelectListItem() { Text = "not equal", Value = "2" });
            entities.Add(new SelectListItem() { Text = "is null", Value = "11" });
            entities.Add(new SelectListItem() { Text = "is not null", Value = "12" });

            return entities;
        }

        #region ActivePrfile
        public ActionResult ActiveProfile(int? id)
        {
            if (id == 0) ViewBag.Title = "Latest members";
            else if (id == 1) ViewBag.Title = "Latest opt-out members";
            else if (id == 2) ViewBag.Title = "Latest deleted members from DLI ";
            return View();
        }

        [HttpPost]
        public ActionResult LoadActiveProfile(int id)
        {
            var profile = GetActiveProfile(id);
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = profile.Count();

            var data = profile.Skip(skip).Take(pageSize).Select(s => new ProfileListViewModel()
            {
                Id = s.Id,
                Email = s.Email,
                MailUpID = s.MailUpID,
                OptIn = s.OptIn,
                Deleted = s.Deleted,
                Created = s.Created,
                Updated = s.Updated,
                Firstname = s.Firstname,
                Lastname = s.Lastname,
                Address = s.Address,
                Address2 = s.Address2,
                Postcode = s.Postcode,
                City = s.City,
                Country = s.Country,
                Phone = s.Phone,
                Mobile = s.Mobile,
                //CVRnummer = s.CVRnummer,
                //BrugerID = s.BrugerID,
                //Medlemsstatus = s.Medlemsstatus,
                //Foreningsnummer = s.Foreningsnummer,
                //Foedselsaar = s.Foedselsaar,
                //HektarDrevet = s.HektarDrevet,
                //AntalAndetKvaeg = s.AntalAndetKvaeg,
                //AntalAmmekoeer = s.AntalAmmekoeer,
                //AntaMalkekoeer = s.AntaMalkekoeer,
                //AntalSlagtesvin = s.AntalSlagtesvin,
                //AntalSoeer = s.AntalSoeer,
                //AntalAarssoeer = s.AntalAarssoeer,
                //AntalPelsdyr = s.AntalPelsdyr,
                //AntalHoens = s.AntalHoens,
                //AntalKyllinger = s.AntalKyllinger,
                //Ecology = s.Ecology,
                //Sektion_SSJ = s.Sektion_SSJ,
                //Driftform_planteavl = s.Driftform_planteavl,
                //Driftform_Koed_Koer = s.Driftform_Koed_Koer,
                //Driftform_Mælk = s.Driftform_Mælk,
                //Driftform_Svin = s.Driftform_Svin,
                //Driftform_Pelsdyr = s.Driftform_Pelsdyr,
                //Driftform_Aeg_Kylling = s.Driftform_Aeg_Kylling,
                //Driftstoerrelse_Planteavl = s.Driftstoerrelse_Planteavl,
                //Driftstoerrelse_Koed_Koer = s.Driftstoerrelse_Koed_Koer,
                //Driftfstoerrelse_Mælk = s.Driftfstoerrelse_Mælk,
                //Driftstoerrelse_Svin = s.Driftstoerrelse_Svin,
                //Driftstoerrelse_Pelsdyr = s.Driftstoerrelse_Pelsdyr,
                //Driftstoerrelse_Aeg_Kylling = s.Driftstoerrelse_Aeg_Kylling,
                //AntalSlagtekvaeg = s.AntalSlagtekvaeg
            }).ToList();

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DownloadActivities(int txtId)
        {
            ExcelPackage excel = new ExcelPackage();
            var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
            FormateExcelFile(workSheet);
            var columnNames = typeof(Recipient).GetProperties().Select(property => property.Name).ToArray();
            SetExcelColumns(columnNames, workSheet);

            var dataList = new List<object[]>();
            //  var segment = _dbcontext.Segments.Find(txtId);
            var profiles = GetActiveProfile(txtId);

            foreach (var profile in profiles)
            {
                dataList.Add(new object[]   {
                    profile.Id,
                    profile.Email,
                    profile.MailUpID,
                    profile.OptIn,
                    profile.Deleted,
                    profile.Created,
                    profile.Updated,
                    profile.Firstname,
                    profile.Lastname,
                    profile.Address,
                    profile.Address2,
                    profile.Postcode,
                    profile.City,
                    profile.Country,
                    profile.Phone,
                    profile.Mobile
                    //,
                    //profile.CVRnummer,
                    //profile.BrugerID,
                    //profile.Medlemsstatus,
                    //profile.Foreningsnummer,
                    //profile.Foedselsaar,
                    //profile.HektarDrevet,
                    //profile.AntalAndetKvaeg,
                    //profile.AntalAmmekoeer,
                    //profile.AntaMalkekoeer,
                    //profile.AntalSlagtesvin,
                    //profile.AntalSoeer,
                    //profile.AntalAarssoeer,
                    //profile.AntalPelsdyr,
                    //profile.AntalHoens,
                    //profile.AntalKyllinger,
                    //profile.Ecology,
                    //profile.Sektion_SSJ,
                    //profile.Driftform_planteavl,
                    //profile.Driftform_Koed_Koer,
                    //profile.Driftform_Mælk,
                    //profile.Driftform_Svin,
                    //profile.Driftform_Pelsdyr,
                    //profile.Driftform_Aeg_Kylling,
                    //profile.Driftstoerrelse_Planteavl,
                    //profile.Driftstoerrelse_Koed_Koer,
                    //profile.Driftfstoerrelse_Mælk,
                    //profile.Driftstoerrelse_Svin,
                    //profile.Driftstoerrelse_Pelsdyr,
                    //profile.Driftstoerrelse_Aeg_Kylling,
                    //profile.AntalSlagtekvaeg
                });
            }

            WriteDataInExcel(dataList, workSheet);

            if (txtId == 0) return File(excel.GetAsByteArray(), "text/csv", "Latest memberss_" + DateTime.Today.ToString("mm-dd-YY") + ".xlsx");
            else if (txtId == 1) return File(excel.GetAsByteArray(), "text/csv", "Latest opt-out members_" + DateTime.Today.ToString("mm-dd-YY") + ".xlsx");
            else if (txtId == 2) return File(excel.GetAsByteArray(), "text/csv", "Latest deleted members from DLI_" + DateTime.Today.ToString("mm-dd-YY") + ".xlsx");
            else return File(excel.GetAsByteArray(), "text/csv", "Blank_" + DateTime.Today.ToString("mm-dd-YY") + ".xlsx");

            // return;
        }

        private List<Recipient> GetActiveProfile(int id)
        {
            var profile = new List<Recipient>();
            if (id == 0) profile = _dbcontext.Recipients.Where(x => x.OptIn == true || x.OptIn == null).OrderByDescending(y => y.Created).ToList();
            else if (id == 1) profile = _dbcontext.Recipients.Where(x => x.OptIn == false).OrderByDescending(y => y.Updated).ToList();
            else if (id == 2) profile = _dbcontext.Recipients.Where(x => x.Deleted == true).OrderByDescending(y => y.Updated).ToList();

            return profile;
        }
        #endregion ActivePrfile
    }
}