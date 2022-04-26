using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MembersManager.Models
{
    public class ProfileListViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string MailUpID { get; set; }
        public bool? OptIn { get; set; }
        public bool? Deleted { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }
        public string ExternalId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public int? Postcode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string CVRnummer { get; set; }
        public string BrugerID { get; set; }
        public int? Medlemsstatus { get; set; }
        public int? Foreningsnummer { get; set; }
        public string Foedselsaar { get; set; }
        public int? HektarDrevet { get; set; }
        public int? AntalAndetKvaeg { get; set; }
        public int? AntalAmmekoeer { get; set; }
        public int? AntaMalkekoeer { get; set; }
        public int? AntalSlagtesvin { get; set; }
        public int? AntalSoeer { get; set; }
        public int? AntalAarssoeer { get; set; }
        public int? AntalPelsdyr { get; set; }
        public int? AntalHoens { get; set; }
        public int? AntalKyllinger { get; set; }
        public string Ecology { get; set; }
        public string Sektion_SSJ { get; set; }
        public string Driftform_planteavl { get; set; }
        public string Driftform_Koed_Koer { get; set; }
        public string Driftform_Mælk { get; set; }
        public string Driftform_Svin { get; set; }
        public string Driftform_Pelsdyr { get; set; }
        public string Driftform_Aeg_Kylling { get; set; }
        public int? Driftstoerrelse_Planteavl { get; set; }
        public int? Driftstoerrelse_Koed_Koer { get; set; }
        public int? Driftfstoerrelse_Mælk { get; set; }
        public int? Driftstoerrelse_Svin { get; set; }
        public int? Driftstoerrelse_Pelsdyr { get; set; }
        public int? Driftstoerrelse_Aeg_Kylling { get; set; }
        public int? AntalSlagtekvaeg { get; set; }
    }
}