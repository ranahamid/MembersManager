using MembersManager.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MembersManager.Models
{

    public class MemberProfileViewModel
    {
        public MemberProfileViewModel()
        {
            this.BoardMemberRoles = new List<RoleViewModel>();
            this.UnionMemberRoles = new List<RoleViewModel>();
            this.ExternalMemberRoles = new List<RoleViewModel>();
        }
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public List<SelectListItem> Segments { get; set; }

        public Dictionary<string, object> ProfileInfo = new Dictionary<string, object>();
        //addition
        public List<int> SelectedSegments { get; set; }

        public SelectListItem ClearAll { get; set; }

        public string SelectedClearAll { get; set; }

        public SelectListItem UnSubscribeAll { get; set; }

        public string SelectedUnSubscribe { get; set; }

        public List<RoleViewModel> BoardMemberRoles { get; set; }

        public List<RoleViewModel> UnionMemberRoles { get; set; }

        public List<RoleViewModel> ExternalMemberRoles { get; set; }
    }
}