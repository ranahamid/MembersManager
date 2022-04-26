using System;
using System.Collections.Generic;
using System.Configuration;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using NLog;
using Services.MMLogger;
using System.Data.Entity.Validation;
using Services.MailUp;
namespace Services.MMSync
{
    public class MMSyncronize
    {
        private List<MMData> members = null;
        private StringBuilder wrongLines = null;
        private StringBuilder importLog = null;
        public MMSyncronize()
        {
            wrongLines = new StringBuilder();
            members = new List<MMData>();
            Log.Init(ConfigurationManager.AppSettings["LogPath"], Log.ToLogLevel(ConfigurationManager.AppSettings["LogLevel"]));
            importLog = new StringBuilder();
        }
        /// <summary>
        /// Do needed stepts to import/sync Memebers
        /// </summary>
        /// <returns></returns>
        public bool Process(string filePath)
        {
            DbHelper db = new DbHelper();
            bool ret = true;
            Log.Info(string.Format("Process started"), this);
            try
            {
                bool bc = StepReadCsvFile(filePath);
                if (bc == false)
                {
                    Log.Info(string.Format("Process failed - read csv {0}"), importLog);
                    db.SaveLog(importLog.ToString());
                    return false;
                }
                bc = StepProcessMembers();
                if (bc == false)
                {
                    Log.Info(string.Format("Process failed - process {0}"), importLog);
                    db.SaveLog(importLog.ToString());
                    return false;
                }
                bc = StepProcessLog();
                Log.Info(string.Format("Process done"), this);
            }
            catch (Exception ex) {
                Log.Error(ex, string.Format("Process error: {0} to {1}", ex.Message, ex.StackTrace));
                ret = false;
            }
            return ret;
        }

        public bool StepReadCsvFile(string filePath)
        {
            char[] delimiters = new char[] { ';' };
            string[] parts = null;
            try
            {
                Log.Info(string.Format("StepReadCsvFile started for {0}", filePath), this);
                using (TextFieldParser parser = new TextFieldParser(filePath, ASCIIEncoding.GetEncoding("iso-8859-1")))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(";");
                    bool skip_header = true;
                    while (!parser.EndOfData)
                    {
                        string line = parser.ReadLine();
                        if (line == null) break;
                        if (skip_header)
                        {
                            skip_header = false;
                            continue;
                        }
                        parts = line.Split(delimiters);
                        //todo process....
                        //Log.Debug(string.Format("[{0}, [{1}]", DateTime.Now.Ticks, parts[0]));
                        MMData user = CreateFromRow(parts);
                        if (user == null)
                        {
                            wrongLines.AppendLine(string.Join("", parts));
                        }
                        else
                            members.Add(user);
                    }
                }
                if (wrongLines.Length> 0)
                    Log.Info(string.Format("wrong lines\r\n{0}", wrongLines.ToString()), this);
                Log.Info(string.Format("StepReadCsvFile done"), this);

            }
            catch (Exception ex)
            {
                importLog.AppendFormat("failed to read cvs data {0} - {1}",ex.Message, ex.StackTrace);
                Log.Error(ex);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Process members
        /// </summary>
        /// <returns></returns>
        public bool StepProcessMembers()
        {
            Log.Info(string.Format("StepProcessMembers started"), this);
            int nNew = 0,nUpdated = 0, nDeleted = 0, nError=0;
            bool mailUpDisable = ConfigurationManager.AppSettings["MailUpDisable"] == null || ConfigurationManager.AppSettings["MailUpDisable"].ToString() == "0" ? true : false;
            StringBuilder sb = new StringBuilder();
            // we have the loaded list. we should start to process it.
            // teremin which is new / update / remove or bad_member_data
            DbHelper db = new DbHelper();
            Services.MailUp.MailUp mp = new Services.MailUp.MailUp();
            string auth = mp.Authorize(ConfigurationManager.AppSettings["MailUpUser"], ConfigurationManager.AppSettings["MailUpPassword"]);
            List<MMData> fromDb = db.GetAllMembers();
            // do comparations
            for (int i = fromDb.Count - 1; i >= 0; i--)
            {
                for (int j = members.Count - 1; j >= 0; j--)
                {
                    if (members[j].ExternalId == fromDb[i].ExternalId)
                    {
                        members[j].Id = fromDb[i].Id;
                        members[j].Created = fromDb[i].Created;
                        members[j].Updated = fromDb[i].Updated;
                        members[j].MailUpId = fromDb[i].MailUpId;
                        members[j].Deleted = fromDb[i].Deleted;
                        members[j].OptIn = fromDb[i].OptIn;
                        //members[j] = fromDb[i];
                        members[j].Status = MMStatus.Updated;
                        Log.Debug("Remove {0}", fromDb[i].Email);
                        if (members[j].Deleted == true)
                            members[j].Status = MMStatus.UpdateAndSubscribe;
                        fromDb.RemoveAt(i);
                        break;
                    }
                }
            }
            // check empty emails and duplicates
            CheckInvalidEmptyOrDuplicates();
#if DEBUG
            Log.Debug("from cvs");
            members.ForEach(x => Log.Debug(x.Firstname+ " - " + x.Status + " - " + x.Email));
            Log.Debug("from db");
            fromDb.ForEach(x => Log.Debug(x.Firstname + " - " + x.Status + " - " + x.Email));
#endif
            #region INSERT / UPDATE
            foreach (MMData user in members)
            {
                try
                {
                    if (user.Status == MMStatus.New)
                    {
                        int id = db.InsertMember(user);
                        user.Id = id;
                        sb.AppendFormat("new {0} with id={1}\r\n", user.Firstname, user.Email);
                        nNew++;
                    }
                    else if (user.Status == MMStatus.Updated || user.Status == MMStatus.UpdateAndSubscribe || user.Status == MMStatus.EmailEmpty)
                    {
                        if (user.Status == MMStatus.UpdateAndSubscribe)
                            user.Deleted = false;
                        int id = db.UpdateMember(user);
                        sb.AppendFormat("update {0} with id={1}\r\n", user.Firstname, user.Email);
                        nUpdated++;
                    }
                    else
                        sb.AppendFormat("other {0} with status={1}\r\n", user.Firstname, user.Status);
                }
                catch (DbEntityValidationException e)
                {
                    nError++;
                    Log.Info(string.Format("error for {0} {1} {2}",user.ExternalId, user.Firstname, user.Email));
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Log.Info(" Exception Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                       sb.AppendFormat(" Exception Entity of type \"{0}\" in state \"{1}\" has the following validation errors:\r\n",
                           eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Log.Info("- Property: \"{0}\", Value: \"{1}\", Error: \"{2}\"",
                                ve.PropertyName,
                                eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName),
                                ve.ErrorMessage);
                            sb.AppendFormat("- Property: \"{0}\", Value: \"{1}\", Error: \"{2}\"\r\n",
                                ve.PropertyName,
                                eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName),
                                ve.ErrorMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    nError++;
                    Log.Error(ex, string.Format("error m {0} {1} {2} ", user.Firstname, user.Email, ex.Message));
                    sb.AppendFormat("error {0} with error={1}\r\n", user.Email, ex.Message);
                }
            }
            #endregion
            #region remove inactive members
            foreach (MMData user in fromDb)
            {
                try
                {
                    if (user.Status == MMStatus.FromDb)
                    {
                        int id = db.UpdateMemberToDeleted(user);
                        sb.AppendFormat("remove {0} with id={1}\r\n", user.Email, id);
                        nDeleted++;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, string.Format("error d {0} {1} {2} ", user.Firstname, user.Email, ex.Message));
                    sb.AppendFormat("error {0} with error={1}\r\n", user.Email, ex.Message);
                }
            }
            #endregion

            if (!mailUpDisable)
            {
                #region mailup subscribe
                //mailUp 
                foreach (MMData user in members)
                {
                    try
                    {
                        if (user.Status == MMStatus.New || user.Status == MMStatus.UpdateAndSubscribe)
                        {
                            int id = mp.SubscribeToList(user.Firstname, user.Lastname, user.Email, user.Phone, ConfigurationManager.AppSettings["MailUpListId"], false);
                            sb.AppendFormat("mailup new {0} - {1} with id={2}\r\n", user.Firstname, user.Email, id);
                            if (id > 0)
                            {
                                user.MailUpId = id.ToString();
                                db.UpdateMember(user);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, string.Format("mailup error {0} {1} {2} ", user.Firstname, user.Email, ex.Message));
                        sb.AppendFormat("mailup error {0} with error={1}\r\n", user.Email, ex.Message);
                    }
                }

                #endregion
                #region mailup unsubscribe
                foreach (MMData user in fromDb)
                {
                    try
                    {
                        if (user.Status == MMStatus.FromDb)
                        {
                            bool id = mp.UnsubscribeFromListWithId(user.Email, ConfigurationManager.AppSettings["MailUpListId"], user.MailUpId);
                            sb.AppendFormat("remove {0} with id={1}\r\n", user.Email, id);
                            nDeleted++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, string.Format("error d {0} {1} {2} ", user.Firstname, user.Email, ex.Message));
                        sb.AppendFormat("error {0} with error={1}\r\n", user.Email, ex.Message);
                    }
                }
                #endregion
            }
            importLog.AppendFormat("New members={0}, Updated members={1}, Inactive members={2}, Errors={3}",nNew, nUpdated, nDeleted, nError);
            Log.Info(importLog.ToString(), this);
            Log.Debug(sb.ToString(), this);
            Log.Info(string.Format("StepProcessMembers done"), this);
            return true;
        }
        public bool StepProcessLog()
        {
            try
            {
                DbHelper db = new DbHelper();
                db.SaveLog(importLog.ToString());
            }catch(Exception ex)
            {
                Log.Error(ex, string.Format("StepProcessLog [{0}]", importLog));
            }
            return true;
        }
        private void CheckInvalidEmptyOrDuplicates()
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, string> duplicates = new Dictionary<string, string>();
            int empty = 0, dupl = 0, invalid = 0;
            //foreach (MMData user in members)
            for (int j = members.Count - 1; j >= 0; j--)
            {
                MMData user = members[j];
                if (string.IsNullOrEmpty(user.Email) || string.IsNullOrWhiteSpace(user.Email))
                {
                    user.Status = MMStatus.EmailEmpty;
                    sb.AppendFormat("empty; {0};{1};{2};{3};{4}\r\n", user.ExternalId, user.Firstname, user.Lastname, user.Phone, user.Email);
                    empty++;
                }else if (IsValidEmail(user.Email) == false)
                {
                    sb.AppendFormat("invalid; {0};{1};{2};{3};{4}\r\n", user.ExternalId, user.Firstname, user.Lastname, user.Phone, user.Email);
                    invalid++;
                }
                else
                {
                    if (duplicates.ContainsKey(user.Email))
                    {
                        user.Status = MMStatus.EmailDuplicate;
                        sb.AppendFormat("duplicate; {0};{1};{2};{3};{4}\r\n", user.ExternalId, user.Firstname, user.Lastname, user.Phone, user.Email);
                        dupl++;
                    }
                    else
                        duplicates.Add(user.Email, user.Email);
                    
                }
            }
            Log.Info(string.Format("wrong emails from {0} empty={1} invalid={2} dupplicated={3} \r\n{4}",members.Count(),empty, invalid, dupl ,sb.ToString()), this);
        }
        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        private MMData CreateFromRow(string[] row)
        {
            MMData user = new MMData();
            try {
                user.ExternalId = row[(int)MMEnums.ExternalId];
                user.Firstname = row[(int)MMEnums.Firstname];
                user.Lastname = row[(int)MMEnums.Lastname];
                user.Address = row[(int)MMEnums.Address];
                user.Address2 = row[(int)MMEnums.Address2];
                user.Postcode = ConvertFromNullableInt(row[(int)MMEnums.Postcode]);
                user.City = row[(int)MMEnums.City];
                user.Country = row[(int)MMEnums.Country];
                user.Phone = row[(int)MMEnums.Phone];
                user.Mobile = row[(int)MMEnums.Mobile];
                user.Email = row[(int)MMEnums.Email];
                user.CVRnummer = row[(int)MMEnums.CVRnummer];
                user.BrugerID = row[(int)MMEnums.BrugerID];
                user.Medlemsstatus = ConvertFromNullableInt(row[(int)MMEnums.Medlemsstatus]);
                user.Foreningsnummer = ConvertFromNullableInt(row[(int)MMEnums.Foreningsnummer]);
                user.Foedselsaar = row[(int)MMEnums.Foedselsaar];
                user.HektarDrevet = ConvertFromNullableInt(row[(int)MMEnums.HektarDrevet]);
                user.AntalAmmekoeer = ConvertFromNullableInt(row[(int)MMEnums.AntalAmmekoeer]);
                user.AntalAndetKvaeg = ConvertFromNullableInt(row[(int)MMEnums.AntalAndetKvaeg]);
                user.AntalSlagtekvaeg = ConvertFromNullableInt(row[(int)MMEnums.AntalSlagtekvaeg]);
                user.AntaMalkekoeer = ConvertFromNullableInt(row[(int)MMEnums.AntaMalkekoeer]);
                user.AntalSlagtesvin = ConvertFromNullableInt(row[(int)MMEnums.AntalSlagtesvin]);
                user.AntalSoeer = ConvertFromNullableInt(row[(int)MMEnums.AntalSoeer]);
                user.AntalAarssoeer = ConvertFromNullableInt(row[(int)MMEnums.AntalAarssoeer]);
                user.AntalPelsdyr = ConvertFromNullableInt(row[(int)MMEnums.AntalPelsdyr]);
                user.AntalHoens = ConvertFromNullableInt(row[(int)MMEnums.AntalHoens]);
                user.AntalKyllinger = ConvertFromNullableInt(row[(int)MMEnums.AntalKyllinger]);
                user.Ecology = row[(int)MMEnums.Ecology];
                user.Sektion_SSJ = row[(int)MMEnums.Sektion_SSJ];
                user.Driftform_planteavl = row[(int)MMEnums.Driftform_planteavl];
                user.Driftform_Koed_Koer = row[(int)MMEnums.Driftform_Koed_Koer];
                user.Driftform_Mælk = row[(int)MMEnums.Driftform_Mælk];
                user.Driftform_Svin = row[(int)MMEnums.Driftform_Svin];
                user.Driftform_Pelsdyr = row[(int)MMEnums.Driftform_Pelsdyr];
                user.Driftform_Aeg_Kylling = row[(int)MMEnums.Driftform_Aeg_Kylling];
                user.Driftstoerrelse_Planteavl = ConvertFromNullableInt(row[(int)MMEnums.Driftstoerrelse_Planteavl]);
                user.Driftstoerrelse_Koed_Koer = ConvertFromNullableInt(row[(int)MMEnums.Driftstoerrelse_Koed_Koer]);
                user.Driftfstoerrelse_Mælk = ConvertFromNullableInt(row[(int)MMEnums.Driftfstoerrelse_Mælk]);
                user.Driftstoerrelse_Svin = ConvertFromNullableInt(row[(int)MMEnums.Driftstoerrelse_Svin]);
                user.Driftstoerrelse_Pelsdyr = ConvertFromNullableInt(row[(int)MMEnums.Driftstoerrelse_Pelsdyr]);
                user.Driftstoerrelse_Aeg_Kylling = ConvertFromNullableInt(row[(int)MMEnums.Driftstoerrelse_Aeg_Kylling]);
                user.Created = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Log.Error(ex,string.Format("CreateFromRow {0}", ex.Message));
            }
            return user;
        }

        private int ConvertFromNullableInt(string s)
        {
            int x;
            int.TryParse(s, out x);
            return x;
        }

    }
}
