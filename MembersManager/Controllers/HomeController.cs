using MembersManager.Models;
using MembersManager.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
namespace MembersManager.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public  ActionResult Index()
        {
            var members = _dbcontext.Recipients.ToList();
            var activemembers =new ActiveMemberListViewModel();// new List<ActiveMemberListViewModel>();
            var impirtlogs = _dbcontext.MemberImportLogs.ToList();
            if (impirtlogs.Count > 0)
                activemembers.LasteUpdatedDate = _dbcontext.MemberImportLogs.ToList().OrderByDescending(x => x.Date).FirstOrDefault().Date;

            activemembers.TotalActiveMembers = members.Where(x => x.Deleted == false || x.Deleted == null).ToList().Count;
            activemembers.TotalPrimaryMembers = members.Where(x=>x.PrimaryMembers.Count>0).Count();
            activemembers.TotalBoardMembers = members.Where(x=>x.BoardMembers.Count>0).Count();
            activemembers.TotalUnionMembers = members.Where(x => x.UnionMembers.Count>0).Count();
            activemembers.TotalExternalMembers = members.Where(x => x.ExternalMembers.Count > 0).Count();


            activemembers.memberlist.AddRange(members.Where(x => x.OptIn == true || x.OptIn == null).OrderByDescending(y => y.Created).Select(s => new MemberList()
            {
                Id = s.Id,
                FirstName = s.Firstname,
                LastName = s.Lastname,
                Date = s.Created,
                Type = "Latest Members"
            }));

            activemembers.memberlist.AddRange(members.Where(x => x.OptIn == false).OrderByDescending(y => y.Updated).Select(s => new MemberList()
            {
                Id = s.Id,
                FirstName = s.Firstname,
                LastName = s.Lastname,
                Date = s.Created,
                Type = "Opt-Out Members"
            }));

            activemembers.memberlist.AddRange(members.Where(x => x.Deleted == true).OrderByDescending(y => y.Updated).Select(s => new MemberList()
            {
                Id = s.Id,
                FirstName = s.Firstname,
                LastName = s.Lastname,
                Date = s.Created,
                Type = "Deleted Members"
            }));

            



            return View(activemembers);
        }
     
        
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}