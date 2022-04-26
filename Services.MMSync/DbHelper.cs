using Services.MMDb;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.MMSync
{
    public class DbHelper
    {
        public int InsertMember(MMData user)
        {
            Int32 id = 0;
            MMDb.Profile profile = new MMDb.Profile {
                Address = user.Address,
                Address2 = user.Address2,
                AntalAarssoeer = user.AntalAarssoeer,
                AntalAmmekoeer = user.AntalAmmekoeer,
                AntalAndetKvaeg = user.AntalAndetKvaeg,
                AntalHoens = user.AntalHoens,
                AntalKyllinger = user.AntalKyllinger,
                AntalPelsdyr = user.AntalPelsdyr,
                AntalSlagtekvaeg = user.AntalSlagtekvaeg,
                AntalSlagtesvin = user.AntalSlagtesvin,
                AntalSoeer = user.AntalSoeer,
                AntaMalkekoeer = user.AntaMalkekoeer,
                BrugerID = user.BrugerID,
                City = user.City,
                Country = user.Country,
                Created = user.Created,
                CVRnummer = user.CVRnummer,
                Driftform_Aeg_Kylling = user.Driftform_Aeg_Kylling,
                Driftform_Koed_Koer = user.Driftform_Koed_Koer,
                Driftform_Mælk = user.Driftform_Mælk,
                Driftform_Pelsdyr = user.Driftform_Pelsdyr,
                Driftform_planteavl = user.Driftform_planteavl,
                Driftform_Svin = user.Driftform_Svin,
                Driftfstoerrelse_Mælk = user.Driftfstoerrelse_Mælk,
                Driftstoerrelse_Aeg_Kylling = user.Driftstoerrelse_Aeg_Kylling,
                Driftstoerrelse_Koed_Koer = user.Driftstoerrelse_Koed_Koer,
                Driftstoerrelse_Pelsdyr = user.Driftstoerrelse_Pelsdyr,
                Driftstoerrelse_Planteavl = user.Driftstoerrelse_Planteavl,
                Driftstoerrelse_Svin = user.Driftstoerrelse_Svin,
                Ecology = user.Ecology,
                Email = user.Email,
                ExternalId = user.ExternalId,
                Firstname = user.Firstname,
                Foedselsaar = user.Foedselsaar,
                Foreningsnummer = user.Foreningsnummer,
                HektarDrevet = user.HektarDrevet,
                Lastname = user.Lastname,
                Medlemsstatus = user.Medlemsstatus,
                Mobile = user.Mobile,
                Phone = user.Phone,
                Postcode = user.Postcode,
                Sektion_SSJ = user.Sektion_SSJ
            };
            MembersEntities me = new MembersEntities();
            me.Profiles.Add(profile);
            id = me.SaveChanges();
            return (int)profile.Id;
        }
        public int UpdateMember(MMData user)
        {
            Int32 id = 0;
            MMDb.Profile profile = new MMDb.Profile
            {
                Id = user.Id,
                Updated = DateTime.UtcNow,
                Created = user.Created,
                MailUpID = user.MailUpId,
                Deleted = user.Deleted,
                OptIn = user.OptIn,
                Address = user.Address,
                Address2 = user.Address2,
                AntalAarssoeer = user.AntalAarssoeer,
                AntalAmmekoeer = user.AntalAmmekoeer,
                AntalAndetKvaeg = user.AntalAndetKvaeg,
                AntalHoens = user.AntalHoens,
                AntalKyllinger = user.AntalKyllinger,
                AntalPelsdyr = user.AntalPelsdyr,
                AntalSlagtekvaeg = user.AntalSlagtekvaeg,
                AntalSlagtesvin = user.AntalSlagtesvin,
                AntalSoeer = user.AntalSoeer,
                AntaMalkekoeer = user.AntaMalkekoeer,
                BrugerID = user.BrugerID,
                City = user.City,
                Country = user.Country,
                CVRnummer = user.CVRnummer,
                Driftform_Aeg_Kylling = user.Driftform_Aeg_Kylling,
                Driftform_Koed_Koer = user.Driftform_Koed_Koer,
                Driftform_Mælk = user.Driftform_Mælk,
                Driftform_Pelsdyr = user.Driftform_Pelsdyr,
                Driftform_planteavl = user.Driftform_planteavl,
                Driftform_Svin = user.Driftform_Svin,
                Driftfstoerrelse_Mælk = user.Driftfstoerrelse_Mælk,
                Driftstoerrelse_Aeg_Kylling = user.Driftstoerrelse_Aeg_Kylling,
                Driftstoerrelse_Koed_Koer = user.Driftstoerrelse_Koed_Koer,
                Driftstoerrelse_Pelsdyr = user.Driftstoerrelse_Pelsdyr,
                Driftstoerrelse_Planteavl = user.Driftstoerrelse_Planteavl,
                Driftstoerrelse_Svin = user.Driftstoerrelse_Svin,
                Ecology = user.Ecology,
                Email = user.Email,
                ExternalId = user.ExternalId,
                Firstname = user.Firstname,
                Foedselsaar = user.Foedselsaar,
                Foreningsnummer = user.Foreningsnummer,
                HektarDrevet = user.HektarDrevet,
                Lastname = user.Lastname,
                Medlemsstatus = user.Medlemsstatus,
                Mobile = user.Mobile,
                Phone = user.Phone,
                Postcode = user.Postcode,
                Sektion_SSJ = user.Sektion_SSJ
            };
            MembersEntities me = new MembersEntities();
            //me.Profiles.Attach(profile);
            me.Entry(profile).State = System.Data.Entity.EntityState.Modified;
            id = me.SaveChanges();
            return (int)id;
        }
        public int UpdateMemberToDeleted(MMData user)
        {
            Int32 id = 0;
            MMDb.Profile profile = new MMDb.Profile
            {
                Id = user.Id,
                Updated = DateTime.UtcNow,
                Deleted = true,
                Created = user.Created,
                Address = user.Address,
                Address2 = user.Address2,
                AntalAarssoeer = user.AntalAarssoeer,
                AntalAmmekoeer = user.AntalAmmekoeer,
                AntalAndetKvaeg = user.AntalAndetKvaeg,
                AntalHoens = user.AntalHoens,
                AntalKyllinger = user.AntalKyllinger,
                AntalPelsdyr = user.AntalPelsdyr,
                AntalSlagtekvaeg = user.AntalSlagtekvaeg,
                AntalSlagtesvin = user.AntalSlagtesvin,
                AntalSoeer = user.AntalSoeer,
                AntaMalkekoeer = user.AntaMalkekoeer,
                BrugerID = user.BrugerID,
                City = user.City,
                Country = user.Country,
                CVRnummer = user.CVRnummer,
                Driftform_Aeg_Kylling = user.Driftform_Aeg_Kylling,
                Driftform_Koed_Koer = user.Driftform_Koed_Koer,
                Driftform_Mælk = user.Driftform_Mælk,
                Driftform_Pelsdyr = user.Driftform_Pelsdyr,
                Driftform_planteavl = user.Driftform_planteavl,
                Driftform_Svin = user.Driftform_Svin,
                Driftfstoerrelse_Mælk = user.Driftfstoerrelse_Mælk,
                Driftstoerrelse_Aeg_Kylling = user.Driftstoerrelse_Aeg_Kylling,
                Driftstoerrelse_Koed_Koer = user.Driftstoerrelse_Koed_Koer,
                Driftstoerrelse_Pelsdyr = user.Driftstoerrelse_Pelsdyr,
                Driftstoerrelse_Planteavl = user.Driftstoerrelse_Planteavl,
                Driftstoerrelse_Svin = user.Driftstoerrelse_Svin,
                Ecology = user.Ecology,
                Email = user.Email,
                ExternalId = user.ExternalId,
                Firstname = user.Firstname,
                Foedselsaar = user.Foedselsaar,
                Foreningsnummer = user.Foreningsnummer,
                HektarDrevet = user.HektarDrevet,
                Lastname = user.Lastname,
                Medlemsstatus = user.Medlemsstatus,
                Mobile = user.Mobile,
                Phone = user.Phone,
                Postcode = user.Postcode,
                Sektion_SSJ = user.Sektion_SSJ
            };
            MembersEntities me = new MembersEntities();
            //me.Profiles.Attach(profile);
            me.Entry(profile).State = System.Data.Entity.EntityState.Modified;
            id = me.SaveChanges();
            return (int)id;
        }
        public List<MMData> GetAllMembers()
        {
            List<MMData> mmdata = new List<MMData>();
            MembersEntities me = new MembersEntities();
            mmdata = me.Profiles.Select(user => new MMData()
            {
                Id = user.Id,
                Status = MMStatus.FromDb,
                Address = user.Address,
                Address2 = user.Address2,
                AntalAarssoeer = user.AntalAarssoeer,
                AntalAmmekoeer = user.AntalAmmekoeer,
                AntalAndetKvaeg = user.AntalAndetKvaeg,
                AntalHoens = user.AntalHoens,
                AntalKyllinger = user.AntalKyllinger,
                AntalPelsdyr = user.AntalPelsdyr,
                AntalSlagtekvaeg = user.AntalSlagtekvaeg,
                AntalSlagtesvin = user.AntalSlagtesvin,
                AntalSoeer = user.AntalSoeer,
                AntaMalkekoeer = user.AntaMalkekoeer,
                BrugerID = user.BrugerID,
                City = user.City,
                Country = user.Country,
                CVRnummer = user.CVRnummer,
                Driftform_Aeg_Kylling = user.Driftform_Aeg_Kylling,
                Driftform_Koed_Koer = user.Driftform_Koed_Koer,
                Driftform_Mælk = user.Driftform_Mælk,
                Driftform_Pelsdyr = user.Driftform_Pelsdyr,
                Driftform_planteavl = user.Driftform_planteavl,
                Driftform_Svin = user.Driftform_Svin,
                Driftfstoerrelse_Mælk = user.Driftfstoerrelse_Mælk,
                Driftstoerrelse_Aeg_Kylling = user.Driftstoerrelse_Aeg_Kylling,
                Driftstoerrelse_Koed_Koer = user.Driftstoerrelse_Koed_Koer,
                Driftstoerrelse_Pelsdyr = user.Driftstoerrelse_Pelsdyr,
                Driftstoerrelse_Planteavl = user.Driftstoerrelse_Planteavl,
                Driftstoerrelse_Svin = user.Driftstoerrelse_Svin,
                Ecology = user.Ecology,
                Email = user.Email,
                ExternalId = user.ExternalId,
                Firstname = user.Firstname,
                Foedselsaar = user.Foedselsaar,
                Foreningsnummer = user.Foreningsnummer,
                HektarDrevet = user.HektarDrevet,
                Lastname = user.Lastname,
                Medlemsstatus = user.Medlemsstatus,
                Mobile = user.Mobile,
                Phone = user.Phone,
                Postcode = user.Postcode,
                Sektion_SSJ = user.Sektion_SSJ,
                Created = user.Created??DateTime.UtcNow,
                MailUpId = user.MailUpID,
                Deleted = user.Deleted,
                OptIn  =user.OptIn,
                Updated = user.Created ?? DateTime.UtcNow

            }).ToList();

            return mmdata;
        }

        public int SaveLog(string log)
        {
            Int32 id = 0;
            MMDb.MemberImportLog logs = new MMDb.MemberImportLog
            {
                Date = DateTime.UtcNow,
                Message = log
            };
            MembersEntities me = new MembersEntities();
            me.MemberImportLogs.Add(logs);
            id = me.SaveChanges();
            return (int)id;
        }
}
}
