using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MembersManager.Models;
using MembersManager.Models.Entities;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Services.MailUp;
using System.Configuration;
using System.Data.SqlClient;
using Services.MMLogger;

namespace MembersManager.Controllers
{
    public class SegmentController : BaseController
    {
        public ActionResult Index(int? id)
        {
            ProfileViewModel profileViewModel = new ProfileViewModel();
            List<ConditionSetViewModel> conditionList = new List<ConditionSetViewModel>();

            if (id > 0)
            {
                var seg = _dbcontext.Segments.Find(id);

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
        public async Task<ActionResult> Index(ProfileViewModel model)
        {
            ProfileViewModel profileViewModel = new ProfileViewModel();
            ModelState.Clear();
            List<ConditionSetViewModel> conditionList = new List<ConditionSetViewModel>();
            var columnType = typeof(Profile).GetProperties().Select(property => property.PropertyType.FullName).ToArray();
            var columnNames = typeof(Profile).GetProperties().Select(property => property.Name).ToArray();

            UnsubscribeRecepients(model.SegmentId);

            if (ModelState.IsValid && (model.SelectFilter != null || model.SelectBooleanFilter != null))
            {
                DeleteFromConditionSet(model.SegmentId);
                string term = string.Empty;
                int i = 0;
                if (string.IsNullOrEmpty(model.SelectColumn[0]))
                {
                    string query = string.Format("{0} ", GetDefaultQuery(model.SegmentId));
                    await Task.Factory.StartNew(() => BulkSubscription(allQuery, model.SegmentId));

                    SaveToSegmentTbl(query);

                }
                else
                {
                    string AllQuery = string.Empty;
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
                        i++;
                    }
                    var finalyquery = AllQuery + GetSegmentQuery(model.SegmentId);
                    SaveToSegmentTbl(finalyquery);
                    await Task.Factory.StartNew(() => BulkSubscription(AllQuery, model.SegmentId));
                    //  await BulkSubscription(AllQuery, model.SegmentId);
                }
            }
            else
            {
                string query = string.Format("{0} ", allQuery);
                SaveToSegmentTbl(query);
                //await BulkSubscription(query, model.SegmentId);
                await Task.Factory.StartNew(() => BulkSubscription(query, model.SegmentId));
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
                }
            }

            profileViewModel.conditionSet = conditionList;

            profileViewModel.AllFilterOperatorSelectList = GetAllFilterOperatorSelectList();
            profileViewModel.AllBooleanFilterSelectList = GetBooleanFilterSelectList();
            profileViewModel.AllBooleanSelectList = GetBooleanSelectList();
            profileViewModel.AllColumnSelectList = GetAllProfileSelectList();
            profileViewModel.AllFilterSelectList = GetAllFilterSelectList();
            return View(profileViewModel);
        }

        [HttpPost]
        public ActionResult LoadData(int? id)
        {
            IQueryable<Profile> profile;
            if (id != null)
            {
                string query = GetQueryFromSegment(id);
                if (query != null && query != string.Empty)
                {
                    profile = _dbcontext.Profiles.SqlQuery(query).AsQueryable();
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
                ExternalId = s.ExternalId,
                Firstname = s.Firstname,
                Lastname = s.Lastname,
                Address = s.Address,
                Address2 = s.Address2,
                Postcode = s.Postcode,
                City = s.City,
                Country = s.Country,
                Phone = s.Phone,
                Mobile = s.Mobile,
                CVRnummer = s.CVRnummer,
                BrugerID = s.BrugerID,
                Medlemsstatus = s.Medlemsstatus,
                Foreningsnummer = s.Foreningsnummer,
                Foedselsaar = s.Foedselsaar,
                HektarDrevet = s.HektarDrevet,
                AntalAndetKvaeg = s.AntalAndetKvaeg,
                AntalAmmekoeer = s.AntalAmmekoeer,
                AntaMalkekoeer = s.AntaMalkekoeer,
                AntalSlagtesvin = s.AntalSlagtesvin,
                AntalSoeer = s.AntalSoeer,
                AntalAarssoeer = s.AntalAarssoeer,
                AntalPelsdyr = s.AntalPelsdyr,
                AntalHoens = s.AntalHoens,
                AntalKyllinger = s.AntalKyllinger,
                Ecology = s.Ecology,
                Sektion_SSJ = s.Sektion_SSJ,
                Driftform_planteavl = s.Driftform_planteavl,
                Driftform_Koed_Koer = s.Driftform_Koed_Koer,
                Driftform_Mælk = s.Driftform_Mælk,
                Driftform_Svin = s.Driftform_Svin,
                Driftform_Pelsdyr = s.Driftform_Pelsdyr,
                Driftform_Aeg_Kylling = s.Driftform_Aeg_Kylling,
                Driftstoerrelse_Planteavl = s.Driftstoerrelse_Planteavl,
                Driftstoerrelse_Koed_Koer = s.Driftstoerrelse_Koed_Koer,
                Driftfstoerrelse_Mælk = s.Driftfstoerrelse_Mælk,
                Driftstoerrelse_Svin = s.Driftstoerrelse_Svin,
                Driftstoerrelse_Pelsdyr = s.Driftstoerrelse_Pelsdyr,
                Driftstoerrelse_Aeg_Kylling = s.Driftstoerrelse_Aeg_Kylling,
                AntalSlagtekvaeg = s.AntalSlagtekvaeg
            }).ToList();
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult Details(string id)
        {
            return View();
        }


        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(SegmentViewModels model)
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
                        model.MailUpGroupID = mailUp.CreateGroup(ConfigurationManager.AppSettings["MailUpListId"], model.Name);
                    }
                }

                var seg = new Segment()
                {
                    Name = model.Name,
                    MailUpGroupID = model.MailUpGroupID,
                    Query = model.Query,
                    Notes=model.Notes,
                    Visible=model.Visible
                };
                _dbcontext.Segments.Add(seg);

                _dbcontext.SaveChanges();

                Log.Info(string.Format("Segment '{0}' create successfully", seg.Name));

                return RedirectToAction("Index", "Profile", new { @id = seg.ID });
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            LoadMenu();

            return View(model);
        }


        public ActionResult Edit(int id)
        {
            return View(_dbcontext.Segments.Where(x => x.ID == id).Select(s => new SegmentViewModels()
            {
                Id = s.ID,
                Name = s.Name,
                MailUpGroupID = s.MailUpGroupID,
                Query = s.Query,
                Notes=s.Notes,
                Visible=s.Visible
            }).FirstOrDefault());
        }

        [HttpPost]
        public ActionResult Edit(SegmentViewModels model)
        {
            try
            {
                var seg = _dbcontext.Segments.Find(model.Id);
                if (seg != null)
                {
                    if (ConfigurationManager.AppSettings["MailUpEnable"] == "1")
                    {
                        var mailUp = new MailUp(ConfigurationManager.AppSettings["MailUpClientId"],
                                    ConfigurationManager.AppSettings["MailUpClientSecret"],
                                    ConfigurationManager.AppSettings["MailUpCallbackUri"], ConfigurationManager.AppSettings["MailUpUser"], ConfigurationManager.AppSettings["MailUpPassword"]);
                        if (mailUp.accessToken != null)
                        {
                            if (string.IsNullOrEmpty(seg.MailUpGroupID))
                                seg.MailUpGroupID = mailUp.CreateGroup(ConfigurationManager.AppSettings["MailUpListId"], model.Name);
                            else
                                mailUp.UpdateGroup(ConfigurationManager.AppSettings["MailUpListId"], seg.MailUpGroupID, model.Name);
                        }
                    }

                    seg.Notes = model.Notes;
                    seg.Visible = model.Visible;
                    seg.Name = model.Name;
                    _dbcontext.SaveChanges();

                    Log.Info(string.Format("Segment '{0}' Updated successfully", seg.Name));

                    return RedirectToAction("Index", "Profile", new { @id = seg.ID });
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex);
            }


            LoadMenu();

            return View(model);
        }


        public ActionResult Delete(int id)
        {
            return View(_dbcontext.Segments.Where(x => x.ID == id).Select(s => new SegmentViewModels()
            {
                Id = s.ID,
                Name = s.Name,
                MailUpGroupID = s.MailUpGroupID,
                Query = s.Query
            }).FirstOrDefault());
        }

        [HttpPost]
        public ActionResult Delete(int id, string action)
        {
            try
            {
                var seg = _dbcontext.Segments.Include("ProfileDetails").Where(x => x.ID == id).FirstOrDefault();
                if (seg != null)
                {
                    if (ConfigurationManager.AppSettings["MailUpEnable"] == "1")
                    {
                        var mailUp = new MailUp(ConfigurationManager.AppSettings["MailUpClientId"],
                                    ConfigurationManager.AppSettings["MailUpClientSecret"],
                                    ConfigurationManager.AppSettings["MailUpCallbackUri"], ConfigurationManager.AppSettings["MailUpUser"], ConfigurationManager.AppSettings["MailUpPassword"]);
                        if (mailUp.accessToken != null && !string.IsNullOrEmpty(seg.MailUpGroupID))
                        {
                            mailUp.DeleteGroup(ConfigurationManager.AppSettings["MailUpListId"], seg.MailUpGroupID);
                        }
                    }

                    _dbcontext.Database.ExecuteSqlCommand("delete from Segments where ID=" + id);

                    Log.Info(string.Format("Segment {0} Update successfully", seg.Name));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
               
            }
            LoadMenu();

            return View(new SegmentViewModels());
        }

        protected override void Dispose(bool disposing)
        {

            base.Dispose(disposing);
        }


        //#region DownloadSegment

        //[HttpGet]
        //public ActionResult DownloadSegmentProfiles(int txtId)
        //{
        //    ExcelPackage excel = new ExcelPackage();
        //    var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
        //    FormateExcelFile(workSheet);
        //    var columnNames = typeof(Profile).GetProperties().Select(property => property.Name).ToArray();
        //    SetExcelColumns(columnNames, workSheet);

        //    var dataList = new List<object[]>();
        //    var segment = _dbcontext.Segments.Find(txtId);
        //    var profiles = _dbcontext.Profiles.SqlQuery(segment == null ? allQuery : segment.Query).ToList();

        //    foreach (var profile in profiles)
        //    {
        //        dataList.Add(new object[]   {
        //            profile.Id,
        //            profile.Email,
        //            profile.MailUpID,
        //            profile.OptIn,
        //            profile.Deleted,
        //            profile.Created,
        //            profile.Updated,
        //            profile.ExternalId,
        //            profile.Firstname,
        //            profile.Lastname,
        //            profile.Address,
        //            profile.Address2,
        //            profile.Postcode,
        //            profile.City,
        //            profile.Country,
        //            profile.Phone,
        //            profile.Mobile,
        //            profile.CVRnummer,
        //            profile.BrugerID,
        //            profile.Medlemsstatus,
        //            profile.Foreningsnummer,
        //            profile.Foedselsaar,
        //            profile.HektarDrevet,
        //            profile.AntalAndetKvaeg,
        //            profile.AntalAmmekoeer,
        //            profile.AntaMalkekoeer,
        //            profile.AntalSlagtesvin,
        //            profile.AntalSoeer,
        //            profile.AntalAarssoeer,
        //            profile.AntalPelsdyr,
        //            profile.AntalHoens,
        //            profile.AntalKyllinger,
        //            profile.Ecology,
        //            profile.Sektion_SSJ,
        //            profile.Driftform_planteavl,
        //            profile.Driftform_Koed_Koer,
        //            profile.Driftform_Mælk,
        //            profile.Driftform_Svin,
        //            profile.Driftform_Pelsdyr,
        //            profile.Driftform_Aeg_Kylling,
        //            profile.Driftstoerrelse_Planteavl,
        //            profile.Driftstoerrelse_Koed_Koer,
        //            profile.Driftfstoerrelse_Mælk,
        //            profile.Driftstoerrelse_Svin,
        //            profile.Driftstoerrelse_Pelsdyr,
        //            profile.Driftstoerrelse_Aeg_Kylling,
        //            profile.AntalSlagtekvaeg
        //        });
        //    }

        //    WriteDataInExcel(dataList, workSheet);

        //    return File(excel.GetAsByteArray(), "text/csv", "Segment_" + (segment == null ? "All" : segment.Name) + "_Profiles_" + DateTime.Today.ToString("mm-dd-YY") + ".xlsx");

        //    // return;
        //}

        //private void FormateExcelFile(ExcelWorksheet workSheet)
        //{
        //    workSheet.TabColor = System.Drawing.Color.Black;
        //    workSheet.DefaultRowHeight = 12;
        //    workSheet.Row(1).Height = 20;
        //    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //    workSheet.Row(1).Style.Font.Bold = true;
        //}
        //private void SetExcelColumns(string[] columns, ExcelWorksheet workSheet)
        //{
        //    for (var i = 0; i < columns.Length - 1; i++)
        //    {
        //        workSheet.Cells[1, i + 1].Value = columns[i];
        //        workSheet.Column(i + 1).AutoFit();
        //    }
        //}

        //private void WriteDataInExcel(List<object[]> datas, ExcelWorksheet workSheet)
        //{
        //    var recordIndex = 2;
        //    foreach (var d in datas)
        //    {
        //        for (var i = 0; i < d.Length - 1; i++)
        //        {
        //            workSheet.Cells[recordIndex, i + 1].Value = d[i];
        //        }
        //        recordIndex++;
        //    }
        //}

        //#endregion

        //public void BulkSubscription(string query, int segmentId)
        //{
        //    //query = query.Replace("SELECT * FROM", "SELECT  prof.id as Id FROM");
        //    //List<ProfileDetail> SelectedProfiles = new List<ProfileDetail>();
        //    //_dbcontext.Database.ExecuteSqlCommand("UpdateProfileDetails @SegmentId, @OptIn, @Query",
        //    //new SqlParameter("@SegmentId", segmentId),
        //    //new SqlParameter("@OptIn", 1),
        //    //new SqlParameter("@Query", query));
        //}
        ////public bool BulkSubscription(string query, int segmentId)
        ////{
        ////    List<ProfileDetail> SelectedProfiles = new List<ProfileDetail>();         
        ////    //insert
        ////    var profileMembers = _dbcontext.Profiles.SqlQuery(query).ToList();
        ////    if (profileMembers != null)
        ////    {
        ////        foreach (var member in profileMembers)
        ////        {
        ////            //set profile members table optIn= true
        ////            if (member.OptIn == false || member.OptIn == null)
        ////            {
        ////                var optIn = true;
        ////                SetProfileMembersOptIn(member.Id, optIn);
        ////            }

        ////            var profile = _dbcontext.ProfileDetails.Where(x => x.ProfileId == member.Id).FirstOrDefault();
        ////            SelectedProfiles.Add(profile); //2
        ////            if (profile == null)
        ////            {
        ////                //insert
        ////                ProfileDetail profileDetail = new ProfileDetail()
        ////                {
        ////                    ProfileId = member.Id,
        ////                    SegmentId = segmentId.ToString(),
        ////                    OptIn = true,
        ////                    OptInDate = DateTime.Now,
        ////                };
        ////                SelectedProfiles.Add(profileDetail); //1
        ////                _dbcontext.ProfileDetails.Add(profileDetail);
        ////            }
        ////            else
        ////            {
        ////                //get segment
        ////                var segmentList = profile.SegmentId;
        ////                if (segmentList != null)
        ////                {
        ////                    var st = segmentList.Split(',');
        ////                    int[] SelectedSegments = st.Select(int.Parse).ToArray();
        ////                    StringBuilder sgmentList = new StringBuilder();
        ////                    if (!SelectedSegments.Contains(segmentId))
        ////                    {
        ////                        //insert this segment                     
        ////                        foreach (var sgment in SelectedSegments)
        ////                        {
        ////                            if (sgmentList.ToString() != string.Empty)
        ////                            {
        ////                                sgmentList.Append("," + sgment);
        ////                            }
        ////                            else
        ////                            {
        ////                                sgmentList.Append(sgment);
        ////                            }
        ////                        }
        ////                        if (sgmentList.ToString() != string.Empty)
        ////                        {
        ////                            sgmentList.Append("," + segmentId);
        ////                        }
        ////                        profile.SegmentId = sgmentList.ToString();
        ////                    }
        ////                    else
        ////                    {
        ////                        //update segment in profiledetails
        ////                        profile.SegmentId = segmentList;
        ////                    }
        ////                    profile.OptIn = true;
        ////                }
        ////                else
        ////                {
        ////                    //null, so insert only single
        ////                    profile.SegmentId = segmentId.ToString();
        ////                    profile.OptIn = true;
        ////                }
        ////            }

        ////            try
        ////            {
        ////                _dbcontext.SaveChanges();

        ////            }
        ////            catch (Exception e)
        ////            {
        ////                var error = e.ToString();

        ////            }

        ////            SubscribeRecepients(member.Id);
        ////        }
        ////    }

        ////    //to remove
        ////    var AllprofileDetails = _dbcontext.ProfileDetails.ToList();
        ////    var otherProfiles = AllprofileDetails.Except(SelectedProfiles);
        ////    foreach (var profDetail in otherProfiles)
        ////    {
        ////        //get segment
        ////        var segmentList = profDetail.SegmentId;
        ////        if (segmentList != null)
        ////        {
        ////            var st = segmentList.Split(',');
        ////            int[] SelectedSegments = st.Select(int.Parse).ToArray();
        ////            StringBuilder sgmentList = new StringBuilder();
        ////            if (SelectedSegments.Contains(segmentId))
        ////            {
        ////                //insert this segment                     
        ////                foreach (var sgment in SelectedSegments)
        ////                {
        ////                    if (sgment == segmentId)
        ////                    {
        ////                        continue;
        ////                    }
        ////                    else if (sgmentList.ToString() != string.Empty)
        ////                    {
        ////                        sgmentList.Append("," + sgment);
        ////                    }
        ////                    else
        ////                    {
        ////                        sgmentList.Append(sgment);
        ////                    }
        ////                }
        ////                if (sgmentList.ToString() == string.Empty)
        ////                {
        ////                    profDetail.SegmentId = null;
        ////                    //Segment is null
        ////                    //set opt In = false
        ////                    var optIn = false;
        ////                    profDetail.OptIn = optIn;
        ////                    //set profile members table optIn= false
        ////                    SetProfileMembersOptIn(profDetail.ProfileId, optIn);
        ////                }
        ////                else
        ////                {
        ////                    profDetail.SegmentId = sgmentList.ToString();
        ////                    profDetail.OptIn = true;
        ////                }
        ////            }
        ////            else
        ////            {
        ////                //update segment in profiledetails
        ////                profDetail.SegmentId = segmentList;
        ////            }

        ////        }
        ////        else
        ////        {
        ////            //nothing to do                   
        ////        }
        ////        try
        ////        {
        ////            _dbcontext.SaveChanges();

        ////        }
        ////        catch (Exception e)
        ////        {
        ////            var error = e.ToString();

        ////        }
        ////    }


        ////    return true;
        ////}

        //public bool SetProfileMembersOptIn(int ProfileId, bool? optIn)
        //{
        //    var profile = _dbcontext.Profiles.Where(x => x.Id == ProfileId).FirstOrDefault();
        //    if (profile != null)
        //    {
        //        profile.OptIn = optIn;
        //    }

        //    try
        //    {
        //        _dbcontext.SaveChanges();
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        var error = e.ToString();
        //        return false;
        //    }
        //}

        //private void SubscribeRecepients(int id)
        //{
        //    if (ConfigurationManager.AppSettings["MailUpEnable"] == "1")
        //    {
        //        var mailUp = new MailUp();
        //        if (mailUp.accessToken != null)
        //        {
        //            var profileDetails = _dbcontext.ProfileDetails.Include("Profiles").Include("Segments").Where(x => x.ProfileId == id).ToList();
        //            foreach (var pd in profileDetails)
        //            {
        //                if (pd.Segment != null && !string.IsNullOrEmpty(pd.Segment.MailUpGroupID))
        //                {
        //                    mailUp.SubscribeToGroup(ConfigurationManager.AppSettings["MailUpUser"], pd.Profile.Email, pd.Segment.MailUpGroupID, false);
        //                }
        //            }
        //        }
        //    }
        //}

        //private void UnsubscribeRecepients(int segmentid)
        //{
        //    if (ConfigurationManager.AppSettings["MailUpEnable"] == "1")
        //    {
        //        var mailUp = new MailUp();
        //        if (mailUp.accessToken != null)
        //        {
        //            var segments = _dbcontext.Segments.Where(x => x.ID == segmentid).ToList();
        //            foreach (var seg in segments)
        //            {
        //                if (!string.IsNullOrEmpty(seg.MailUpGroupID))
        //                    mailUp.UnsubscribeAllRecepientFromGroup(seg.MailUpGroupID, ConfigurationManager.AppSettings["MailUpListId"]);
        //            }
        //        }
        //    }
        //}

        //public void DeleteFromConditionSet(int SegmentId)
        //{
        //    var condition = _dbcontext.ConditionSets.Where(x => x.SegmentId == SegmentId);
        //    if (condition != null)
        //    {
        //        foreach (var item in condition)
        //        {
        //            _dbcontext.ConditionSets.Remove(item);
        //        }
        //        _dbcontext.SaveChanges();
        //    }
        //}

        //public void SaveToConditionSet(string SelectColumn, string filterText, string term, int SegmentId)
        //{
        //    ConditionSet condition = new ConditionSet()
        //    {
        //        SegmentId = SegmentId,
        //        ColumnName = SelectColumn,
        //        FilterId = filterText,
        //        SearchTerm = term
        //    };
        //    _dbcontext.ConditionSets.Add(condition);
        //    _dbcontext.SaveChanges();
        //}

        //public string GetSearchProfileMembers(string SelectColumn, string typeCol, string SelectFilter, dynamic SearchTerm, string firstQuery)
        //{
        //    //check date time
        //    if ((SelectColumn == "Created" || SelectColumn == "Updated") && (SelectFilter != "11" || SelectFilter != "12"))
        //    {
        //        if (SearchTerm != "null" && SearchTerm != "")
        //            SearchTerm = DateTime.Parse(SearchTerm);
        //        else
        //            SearchTerm = null;
        //    }
        //    bool isInt = false;
        //    bool isString = false;
        //    bool isDateTime = false;
        //    bool isBool = false;
        //    if (typeCol.Contains("Int32"))
        //        isInt = true;
        //    else if (typeCol.Contains("String"))
        //        isString = true;
        //    else if (typeCol.Contains("DateTime"))
        //        isDateTime = true;
        //    else if (typeCol.Contains("Boolean"))
        //        isBool = true;



        //    string Query = string.Empty;

        //    if (SelectFilter == "1")
        //    {
        //        if (isInt)
        //            Query = string.Format("{0} prof.{1} = {2} ", firstQuery, SelectColumn, SearchTerm);
        //        else
        //            //equal
        //            Query = string.Format("{0} prof.{1} = '{2}' ", firstQuery, SelectColumn, SearchTerm);
        //    }
        //    else if (SelectFilter == "2")
        //    {
        //        if (isInt)
        //            Query = string.Format("{0} prof.{1} != {2} ", firstQuery, SelectColumn, SearchTerm);
        //        else
        //            //not equal
        //            Query = string.Format("{0} prof.{1} != '{2}' ", firstQuery, SelectColumn, SearchTerm);
        //    }
        //    else if (SelectFilter == "3")
        //    {
        //        //in
        //        string[] words = SearchTerm.Split(',');
        //        string terms = string.Empty;
        //        var index = 0;
        //        foreach (string word in words)
        //        {
        //            if (index > 0)
        //            {
        //                terms = terms + ",";
        //            }
        //            index++;
        //            if (isInt)
        //                terms = terms + string.Format("{0}", word);
        //            else
        //                terms = terms + string.Format("'{0}'", word);
        //        }
        //        Query = string.Format("{0} prof.{1} IN ({2}) ", firstQuery, SelectColumn, terms);
        //    }
        //    else if (SelectFilter == "4")
        //    {
        //        //not in
        //        string[] words = SearchTerm.Split(',');
        //        string terms = string.Empty;
        //        var index = 0;
        //        foreach (string word in words)
        //        {
        //            if (index > 0)
        //            {
        //                terms = terms + ",";
        //            }
        //            index++;
        //            if (isInt)
        //                terms = terms + string.Format("{0}", word);
        //            else
        //                terms = terms + string.Format("'{0}'", word);
        //        }
        //        Query = string.Format("{0} prof.{1} NOT IN ({2}) ", firstQuery, SelectColumn, terms);
        //    }
        //    else if (SelectFilter == "5")
        //    {
        //        //less
        //        if (isInt)
        //            Query = string.Format("{0} prof.{1} < {2} ", firstQuery, SelectColumn, SearchTerm);
        //        else
        //            Query = string.Format("{0} prof.{1} < '{2}' ", firstQuery, SelectColumn, SearchTerm);
        //    }

        //    else if (SelectFilter == "6")
        //    {
        //        //less or equal
        //        if (isInt)
        //            Query = string.Format("{0} prof.{1} <= {2} ", firstQuery, SelectColumn, SearchTerm);
        //        else
        //            Query = string.Format("{0} prof.{1} <= '{2}' ", firstQuery, SelectColumn, SearchTerm);
        //    }
        //    else if (SelectFilter == "7")
        //    {
        //        //greater
        //        if (isInt)
        //            Query = string.Format("{0} prof.{1} > {2} ", firstQuery, SelectColumn, SearchTerm);
        //        else
        //            Query = string.Format("{0} prof.{1} > '{2}' ", firstQuery, SelectColumn, SearchTerm);
        //    }
        //    else if (SelectFilter == "8")
        //    {
        //        //greater or equal
        //        if (isInt)
        //            Query = string.Format("{0} prof.{1} >= '{2}' ", firstQuery, SelectColumn, SearchTerm);
        //        else
        //            Query = string.Format("{0} prof.{1} >= '{2}' ", firstQuery, SelectColumn, SearchTerm);
        //    }

        //    else if (SelectFilter == "9")
        //    {
        //        //between
        //        string[] words = SearchTerm.Split(',');
        //        string term1 = (words[0] != null) ? words[0] : string.Empty;
        //        string term2 = (words[1] != null) ? words[1] : words[0];
        //        if (isInt)
        //            Query = string.Format("{0} prof.{1} BETWEEN {2} AND {3} ", firstQuery, SelectColumn, term1, term2);
        //        else
        //            Query = string.Format("{0} prof.{1} BETWEEN '{2}' AND '{3}' ", firstQuery, SelectColumn, term1, term2);
        //    }
        //    else if (SelectFilter == "10")
        //    {
        //        //not between
        //        string[] words = SearchTerm.Split(',');
        //        string term1 = (words[0] != null) ? words[0] : string.Empty;
        //        string term2 = (words[1] != null) ? words[1] : words[0];
        //        if (isInt)
        //            Query = string.Format("{0} prof.{1} NOT BETWEEN {2} AND {3} ", firstQuery, SelectColumn, term1, term2);
        //        else
        //            Query = string.Format("{0} prof.{1} NOT BETWEEN '{2}' AND '{3}' ", firstQuery, SelectColumn, term1, term2);
        //    }
        //    else if (SelectFilter == "11")
        //    {
        //        //IS NULL               
        //        Query = string.Format("{0} prof.{1} IS NULL ", firstQuery, SelectColumn);
        //    }
        //    else if (SelectFilter == "12")
        //    {
        //        //IS NOT NULL
        //        Query = string.Format("{0} prof.{1} IS NOT NULL ", firstQuery, SelectColumn);
        //    }
        //    else
        //    {
        //        Query = string.Format("{0} ", firstQuery);
        //    }


        //    return Query;
        //}

        //public void SaveToSegmentTbl(string Query)
        //{
        //    if (Query != string.Empty)
        //    {
        //        //db add
        //        if (RouteData.Values["id"] == null)
        //        {
        //            Segment segment = new Segment()
        //            {
        //                Name = DateTime.Now.ToString(), //need to initialize
        //                Query = Query,
        //                MailUpGroupID = null,
        //            };
        //            _dbcontext.Segments.Add(segment);
        //        }
        //        else
        //        {
        //            //update
        //            var seg = _dbcontext.Segments.Find(Convert.ToInt32("0" + RouteData.Values["id"]));
        //            if (seg != null)
        //            {
        //                seg.Query = Query;
        //            }
        //        }
        //        try
        //        {
        //            _dbcontext.SaveChanges();
        //        }
        //        catch (Exception e)
        //        {
        //            var error = e.ToString();
        //        }
        //    }
        //}

        //public string GetQueryFromSegment(int? id)
        //{
        //    var statement = _dbcontext.Segments.Where(x => x.ID == id).FirstOrDefault();
        //    if (statement != null && statement.Query != null)
        //    {
        //        return statement.Query;
        //    }
        //    return null;
        //}

        //public IQueryable<Profile> GetAllProfileData()
        //{
        //    string query = string.Format("{0} ", allQuery);
        //    return _dbcontext.Profiles.SqlQuery(query).AsQueryable();

        //}

        //public List<SelectListItem> GetAllProfileSelectList()
        //{
        //    List<SelectListItem> entities = new List<SelectListItem>();
        //    var columnNames = typeof(Profile).GetProperties().Select(property => property.Name).ToArray();

        //    foreach (var column in columnNames)
        //    {
        //        entities.Add(new SelectListItem()
        //        {
        //            Value = column.ToString(),
        //            Text = column.ToString(),
        //        }
        //      );
        //    }
        //    return entities;
        //}

        //public List<SelectListItem> GetBooleanSelectList()
        //{
        //    List<SelectListItem> entities = new List<SelectListItem>();

        //    entities.Add(new SelectListItem() { Text = "True", Value = "0" });
        //    entities.Add(new SelectListItem() { Text = "False", Value = "1" });

        //    return entities;
        //}

        //public List<SelectListItem> GetAllFilterSelectList()
        //{
        //    List<SelectListItem> entities = new List<SelectListItem>();

        //    entities.Add(new SelectListItem() { Text = "equal", Value = "1" });
        //    entities.Add(new SelectListItem() { Text = "not equal", Value = "2" });
        //    entities.Add(new SelectListItem() { Text = "in", Value = "3" });
        //    entities.Add(new SelectListItem() { Text = "not in", Value = "4" });
        //    entities.Add(new SelectListItem() { Text = "less", Value = "5" });
        //    entities.Add(new SelectListItem() { Text = "less or equal", Value = "6" });
        //    entities.Add(new SelectListItem() { Text = "greater", Value = "7" });
        //    entities.Add(new SelectListItem() { Text = "greater or equal", Value = "8" });
        //    entities.Add(new SelectListItem() { Text = "between", Value = "9" });
        //    entities.Add(new SelectListItem() { Text = "not between", Value = "10" });
        //    entities.Add(new SelectListItem() { Text = "is null", Value = "11" });
        //    entities.Add(new SelectListItem() { Text = "is not null", Value = "12" });

        //    return entities;
        //}

        //public List<SelectListItem> GetAllFilterOperatorSelectList()
        //{
        //    List<SelectListItem> entities = new List<SelectListItem>();

        //    entities.Add(new SelectListItem() { Text = "=", Value = "1" });
        //    entities.Add(new SelectListItem() { Text = "!=", Value = "2" });
        //    entities.Add(new SelectListItem() { Text = "in", Value = "3" });
        //    entities.Add(new SelectListItem() { Text = "not in", Value = "4" });
        //    entities.Add(new SelectListItem() { Text = "<", Value = "5" });
        //    entities.Add(new SelectListItem() { Text = "<=", Value = "6" });
        //    entities.Add(new SelectListItem() { Text = ">", Value = "7" });
        //    entities.Add(new SelectListItem() { Text = ">=", Value = "8" });
        //    entities.Add(new SelectListItem() { Text = "between", Value = "9" });
        //    entities.Add(new SelectListItem() { Text = "not between", Value = "10" });
        //    entities.Add(new SelectListItem() { Text = "= NULL", Value = "11" });
        //    entities.Add(new SelectListItem() { Text = "!= NULL", Value = "12" });

        //    return entities;
        //}

        //public List<SelectListItem> GetBooleanFilterSelectList()
        //{
        //    List<SelectListItem> entities = new List<SelectListItem>();

        //    entities.Add(new SelectListItem() { Text = "equal", Value = "1" });
        //    entities.Add(new SelectListItem() { Text = "not equal", Value = "2" });
        //    entities.Add(new SelectListItem() { Text = "is null", Value = "11" });
        //    entities.Add(new SelectListItem() { Text = "is not null", Value = "12" });

        //    return entities;
        //}

       

        private void LoadMenu()
        {
            ViewBag.Menu = _dbcontext.Segments.Select(s => new SegmentViewModels()
            {
                Id = s.ID,
                Name = s.Name,
                MailUpGroupID = s.MailUpGroupID,
                Query = s.Query
            }).ToList();
        }

       

    }
}
