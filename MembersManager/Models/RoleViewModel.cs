using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MembersManager.Models
{
    public class RoleViewModel
    {
        public int Id { get; set; }
        public int RecipientId { get; set; }
        
        public string RoleType { get; set; }
        [Required]
        [Display(Name = "Position")]
        public string Position { get; set; }
        [Required]
        [Display(Name = "Company Name")]
        public string Company { get; set; }
    }
}