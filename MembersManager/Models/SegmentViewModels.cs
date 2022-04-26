using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MembersManager.Models
{
    public class SegmentViewModels
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Query")]
        public string Query { get; set; }

        [Display(Name = "MailUpGroup")]
        public string MailUpGroupID { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Notes")]
        public string Notes { get; set; }

        [Display(Name = "Visible segment")]
        public bool Visible { get; set; }
    }
}