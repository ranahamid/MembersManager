using MembersManager.Models;
using MembersManager.Models.Entities;
using Services.MMLogger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MembersManager.Controllers
{
    public class BaseController : Controller
    {
        /*
               SELECT * FROM Profiles  AS prof 
                FULL OUTER JOIN ProfileDetails AS pdetails ON prof.id = pdetails.ProfileId  
                WHERE 
		                prof.Id = 5 AND (prof.OptIn= 1 OR prof.OptIn IS NULL) 
		                AND (pdetails.OptIn= 1 AND 		
			                (
			                pdetails.SegmentId LIKE '%,3,%'
		                   OR pdetails.SegmentId LIKE '3,%'
		                   OR pdetails.SegmentId LIKE '%,3'
		                    OR pdetails.SegmentId ='3'
			                )		
		                ) 
                ORDER BY prof.Id

        */
        public MemberManagerEntities _dbcontext = new MemberManagerEntities();
        //public string allQuery = "SELECT * FROM Profiles  AS prof" +
        //    " LEFT JOIN ProfileDetails AS pdetails ON prof.id = pdetails.ProfileId" +
        //    " WHERE (prof.OptIn = 1 OR prof.OptIn IS NULL) AND" +
        //    " ( pdetails.OptIn= 1 OR pdetails.OptIn IS NULL) ORDER BY  prof.Id";
        public string allQuery = "SELECT * FROM Recipients as prof ORDER BY Id";


        public string mainQuery = "SELECT * FROM Recipients  AS prof LEFT JOIN RecipientSegments AS pdetails ON prof.id = pdetails.RecipientId  WHERE";
        public string andQuery = " AND ";
        //   public string lastQuery = " AND (prof.OptIn= 1 OR prof.OptIn IS NULL) AND (pdetails.OptIn= 1 OR pdetails.OptIn IS NULL) ORDER BY prof.Id";

        public string GetDefaultQuery(int segmentId)
        {
            return "SELECT * FROM Recipients  AS prof" +
             " LEFT JOIN RecipientSegments AS pdetails ON prof.id = pdetails.RecipientId" +
             " WHERE (prof.OptIn = 1 OR prof.OptIn IS NULL) AND" +
             " ( pdetails.OptIn= 1 OR pdetails.OptIn IS NULL) and pdetails.SegmentId=" + segmentId + " ORDER BY  prof.Id";
        }
        public string GetSegmentQuery(int segmentId)
        {            
           return string.Format(" AND (prof.OptIn= 1 OR prof.OptIn IS NULL) AND (pdetails.OptIn= 1 OR pdetails.OptIn  IS NULL) AND  pdetails.SegmentId = {0} ORDER BY prof.Id", segmentId);           
        }

        public BaseController()
        {
            ViewBag.Menu = _dbcontext.Segments.Select(s => new SegmentViewModels()
            {
                Id = s.ID,
                Name = s.Name,
                MailUpGroupID = s.MailUpGroupID,
                Query = s.Query
            }).ToList();

            Log.Init(ConfigurationManager.AppSettings["LogPath"], Log.ToLogLevel(ConfigurationManager.AppSettings["LogLevel"]));
        }
    }
}