using MembersManager.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MembersManager.Models
{

    public class ConditionSetViewModel
    {
        public string SelectColumn { get; set; }
        public string SelectFilter { get; set; }
        public string SearchTerm { get; set; }
    }
    public class ProfileViewModel
    {
        public ProfileViewModel()
        {
            this.SegmentId = 0;
            this.SegmentName = "New Segment";
        }

        public List<ConditionSetViewModel> conditionSet { get; set; }

        public int SegmentId { get; set; }
        public string SegmentName { get; set; }
        public List<Recipient> ProfileData { get; set; }
        // public List<ProfileModel> profileModel { get; set; }

        public List<SelectListItem> AllColumnSelectList = new List<SelectListItem>();
        [Required]
        public List<string> SelectColumn { get; set; }

        public List<SelectListItem> AllFilterSelectList = new List<SelectListItem>();    
        public List<string> SelectFilter { get; set; }

        public List<SelectListItem> AllBooleanSelectList = new List<SelectListItem>();
        public List<string> BooleanTerm{ get; set; }

        public List<SelectListItem> AllFilterOperatorSelectList = new List<SelectListItem>();

        public List<SelectListItem> AllBooleanFilterSelectList = new List<SelectListItem>();     
        public List<string> SelectBooleanFilter { get; set; }

        public List<string> SearchTerm { get; set; }
    }
}