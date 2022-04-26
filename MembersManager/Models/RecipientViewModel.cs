using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MembersManager.Models
{
    public class RecipientViewModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "First Name")]
        public string Firstname { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string Lastname { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "MailUpID")]
        public string MailUpID { get; set; }
        public bool OptIn { get; set; }
        public bool Deleted { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }
        [Display(Name = "ExternalId")]
        public string ExternalId { get; set; }
        [Required]
        [Display(Name = "Address 1")]
        public string Address { get; set; }
        [Display(Name = "Address 2")]
        public string Address2 { get; set; }
        [Required]
        [Display(Name = "Post Code")]
        public int? Postcode { get; set; }
        [Required]
        [Display(Name = "City")]
        public string City { get; set; }
        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; }
        [Required]
        [Display(Name = "Phone")]
        public string Phone { get; set; }
        [Required]
        [Display(Name = "Mobile")]
        public string Mobile { get; set; }
    }
}