using MembersManager.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MembersManager.Models
{

    public class ActiveMemberListViewModel
    {
        public ActiveMemberListViewModel()
        {
            memberlist = new List<MemberList>();
        }
        public DateTime? LasteUpdatedDate { get; set; }
        public int TotalActiveMembers { get; set; }
        public int TotalPrimaryMembers { get; set; }
        public int TotalBoardMembers { get; set; }
        public int TotalUnionMembers { get; set; }
        public int TotalExternalMembers { get; set; }

        public List<MemberList> memberlist { get; set; }
    }

    public class MemberList
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Date { get; set; }
        public string Type { get; set; }
    }
}