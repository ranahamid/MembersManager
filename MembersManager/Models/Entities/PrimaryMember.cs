//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MembersManager.Models.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class PrimaryMember
    {
        public int Id { get; set; }
        public string ExternalId { get; set; }
        public Nullable<bool> Deleted { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<System.DateTime> Updated { get; set; }
        public int RecipientId { get; set; }
        public string CVRnummer { get; set; }
        public string BrugerID { get; set; }
        public Nullable<int> Medlemsstatus { get; set; }
        public Nullable<int> Foreningsnummer { get; set; }
        public string Foedselsaar { get; set; }
        public Nullable<int> HektarDrevet { get; set; }
        public Nullable<int> AntalAndetKvaeg { get; set; }
        public Nullable<int> AntalAmmekoeer { get; set; }
        public Nullable<int> AntaMalkekoeer { get; set; }
        public Nullable<int> AntalSlagtesvin { get; set; }
        public Nullable<int> AntalSoeer { get; set; }
        public Nullable<int> AntalAarssoeer { get; set; }
        public Nullable<int> AntalPelsdyr { get; set; }
        public Nullable<int> AntalHoens { get; set; }
        public Nullable<int> AntalKyllinger { get; set; }
        public string Ecology { get; set; }
        public string Sektion_SSJ { get; set; }
        public string Driftform_planteavl { get; set; }
        public string Driftform_Koed_Koer { get; set; }
        public string Driftform_Mælk { get; set; }
        public string Driftform_Svin { get; set; }
        public string Driftform_Pelsdyr { get; set; }
        public string Driftform_Aeg_Kylling { get; set; }
        public Nullable<int> Driftstoerrelse_Planteavl { get; set; }
        public Nullable<int> Driftstoerrelse_Koed_Koer { get; set; }
        public Nullable<int> Driftfstoerrelse_Mælk { get; set; }
        public Nullable<int> Driftstoerrelse_Svin { get; set; }
        public Nullable<int> Driftstoerrelse_Pelsdyr { get; set; }
        public Nullable<int> Driftstoerrelse_Aeg_Kylling { get; set; }
        public Nullable<int> AntalSlagtekvaeg { get; set; }
    
        public virtual Recipient Recipient { get; set; }
    }
}