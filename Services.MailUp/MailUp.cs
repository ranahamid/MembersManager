using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Services.MMLogger;
using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace Services.MailUp
{
    public class EmailTagDTO
    {
        public Int32 Id { get; set; }
        public String Name { get; set; }
    }
    public class DynamicFieldDTO
    {
        public Int32 Id { get; set; }
        public String Description { get; set; }
    }
    public class EmailDynamicFieldDTO : DynamicFieldDTO
    {
        public String Value { get; set; }
    }
    public class EmailTrackingInfoDTO
    {
        public Boolean Enabled { get; set; }
        public List<String> Protocols { get; set; }
        public String CustomParams { get; set; }

        public String ProtocolsToString()
        {
            String ret = "";
            for (Int32 p = 0; p < Protocols.Count; p++)
            {
                String curProtocol = Protocols[p];

                ret += "" + curProtocol.Replace(":", "") + ":";
                if (p < (Protocols.Count - 1))
                {
                    ret += "|";
                }
            }

            return ret;
        }
        public String CustomParamsToString()
        {
            String ret = "";
            if (!String.IsNullOrEmpty(CustomParams))
            {
                if (CustomParams.StartsWith("?"))
                    ret = CustomParams.Substring(1);
                else
                    ret = CustomParams;
            }
            return ret;
        }
    }

    public class EmailMessageItemDTO
    {
        public Int32 idList { get; set; }
        public Int32 idNL { get; set; }
        public String Subject { get; set; }
        public String Notes { get; set; }
        public String Content { get; set; }
        public List<EmailDynamicFieldDTO> Fields { get; set; }
        public List<EmailTagDTO> Tags { get; set; }
        public Boolean Embed { get; set; }
        public Boolean IsConfirmation { get; set; }
        public EmailTrackingInfoDTO TrackingInfo = new EmailTrackingInfoDTO();
        public String Head { get; set; }
        private String _body;
        public String Body
        {
            get { return _body; }
            set { _body = value; }
        }
        public EmailMessageItemDTO()
        {
            _body = "<body>";
        }
    }

    public class EmailListItem {
        public string Name { get; set; }
        public string Notes { get; set; }
        public string ReplyTo { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string CountryCode { get; set; }
        public string PermissionReminder { get; set; }
        public string WebSiteUrl { get; set; }
        public int IdSettings { get; set; }
        public bool Business { get; set; }
        public bool Customer { get; set; }
        public bool UseDefaultSettings { get; set; }
        
    }
    public class EmailGroupItem
    {
        public string Name { get; set; }
        public string Notes { get; set; }
        public bool Deletable { get; set; }
        public string idGroup { get; set; }
        public string idList { get; set; }
        public string Count { get; set; }

    }

    public class MailUp
    {
        private MailUpClient mail = null;
        public string accessToken= null;

        public MailUp(string clientId, string secret, string callback)
        {
            mail = new MailUpClient(clientId, secret, callback);
        }
        public MailUp(string clientId, string secret, string callback, string user, string password): this(clientId, secret, callback)
        {
            accessToken = Authorize(user, password);
        }

        #region METHODS
        public string Authorize(string user, string pass)
        {
            mail.LogOnWithUsernamePassword(user, pass);
            accessToken = mail.AccessToken;
            return mail.AccessToken;
        }
        public bool SubscribeToGroup(string userName, string email, string groupId, bool confirmationNeeded)
        {
            bool ret = false;
            try
            {
                Log.Info(string.Format("SubscribeToGroup userName={0}, email={1}, groupId={2}, confirmationNeeded={3}", userName, email, groupId, confirmationNeeded), this);
                // Import recipients to group
                string resourceURL = "" + mail.ConsoleEndpoint + "/Console/Group/" + groupId + "/Recipient";
                if (confirmationNeeded)
                    resourceURL += "?ConfirmEmail=true";
                String recipientRequest = "{\"Email\":\"" + email + "\"}";
                string strResult = mail.CallMethod(resourceURL,
                                                     "POST",
                                                     recipientRequest,
                                                     ContentType.Json);
                int importId = int.Parse(strResult);
                Log.Info(string.Format("SubscribeToGroup response {0}", strResult), this);
                ret = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex,string.Format("SubscribeToGroup error {0}", ex.Message));
            }
            return ret;
        }
        public bool SubscribeToVoresmadList(string email, string groupId, string firstName, string lastName, string serviceRequest, int age, string gender, string houseHold, string children, string region, bool confirmationNeeded)
        {
            bool ret = false;
            try
            {
                Log.Info(string.Format("Subscribe email={0}, groupId={1}, firstName={2}, lastName={3}, serviceRequest={4}, age={5}, gender={6}, houseHold={7}, children={8}, region={9}, confirmationNeeded={10}", email, groupId, firstName, lastName, serviceRequest, age, gender, houseHold, children, region, confirmationNeeded), this);
                // Import recipients to group
                string resourceURL = "" + mail.ConsoleEndpoint + "/Console/Group/" + groupId + "/Recipient";
                if (confirmationNeeded)
                    resourceURL += "?ConfirmEmail=true";
                String recipientRequest = "{\"Email\":\"" + email + "\",\"Fields\":[{\"Description\":\"FirstName\",\"Id\":1,\"Value\":\"" + firstName + "\"}," +
                    "{\"Description\":\"LastName\",\"Id\":2,\"Value\":\"" + lastName + "\"}," +
                    "{\"Description\":\"Service Request\",\"Id\":29,\"Value\":\"" + serviceRequest + "\"}," +
                    "{\"Description\":\"Age\",\"Id\":28,\"Value\":\"" + age + "\"}," +
                    "{\"Description\":\"Gender\",\"Id\":10,\"Value\":\"" + gender + "\"}," +
                    "{\"Description\":\"Household\",\"Id\":30,\"Value\":\"" + houseHold + "\"}," +
                    "{\"Description\":\"Children\",\"Id\":31,\"Value\":\"" + children + "\"}," +
                    "{\"Description\":\"Region\",\"Id\":8,\"Value\":\"" + region + "\"}]";
                string strResult = mail.CallMethod(resourceURL,
                                                     "POST",
                                                     recipientRequest,
                                                     ContentType.Json);
                int importId = int.Parse(strResult);

                Log.Info(string.Format("Subscribe response {0}", strResult), this);
                ret = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex,string.Format("Subscribe error {0}", ex.Message));
            }
            return ret;
        }

        public bool UnsubscribeFromGroup(string email, string groupId, string listId)
        {
            bool ret = false;
            try
            {
                Log.Info(string.Format("Unsubscribe email={0}, groupId={1}", email, listId), this);
                String resourceURL = "";
                String strResult = "";
                Object objResult;
                Dictionary<String, Object> items = new Dictionary<String, Object>();

                // Request for recipient in a group
                resourceURL = "" + mail.ConsoleEndpoint + "/Console/Group/" + groupId + "/Recipients";
                strResult = mail.CallMethod(resourceURL,
                                                     "GET",
                                                     null,
                                                     ContentType.Json);
                Log.Info(string.Format("Unsubscribe strResult={0}", strResult), this);

                objResult = new JavaScriptSerializer().DeserializeObject(strResult);

                Console.WriteLine(string.Format("<p>Request for recipient in a group<br/>{0} {1} - OK</p>", "GET", resourceURL));

                items = (Dictionary<String, Object>)objResult;
                Object[] recipients = (Object[])items["Items"];
                for (int i = 0; i < recipients.Length; i++)
                {
                    Dictionary<String, Object> recipient = (Dictionary<String, Object>)recipients[i];
                    if (recipient["Email"].ToString().ToLower() == email.ToLower())
                    {
                        int recipientId = int.Parse(recipient["idRecipient"].ToString());
                        // Pick up a recipient and unsubscribe it
                        
                        //resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + listId + "/Unsubscribe/" + recipientId;
                        resourceURL = "" + mail.ConsoleEndpoint + "/Console/Group/" + groupId + "/Unsubscribe/" + recipientId;

                        strResult = mail.CallMethod(resourceURL,
                                                             "DELETE",
                                                             null,
                                                             ContentType.Json);

                        Log.Info(string.Format("Unsubscribe delete strResult={0}", strResult), this);
                        Log.Info(String.Format("<p>Pick up a recipient and unsubscribe it<br/>{0} {1} - OK</p>", "DELETE", resourceURL), this);
                    }
                }
                ret = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Format("Unsubscribe error {0}", ex.Message));
            }
            return ret;
        }
        public bool UnsubscribeAllRecepientFromGroup(string groupId, string listId)
        {
            bool ret = false;
            try
            {
                Log.Info(string.Format("Unsubscribe {0}, groupId={1}", "All Recipient", listId), this);
                String resourceURL = "";
                String strResult = "";
                Object objResult;
                Dictionary<String, Object> items = new Dictionary<String, Object>();

                // Request for recipient in a group
                resourceURL = "" + mail.ConsoleEndpoint + "/Console/Group/" + groupId + "/Recipients";
                strResult = mail.CallMethod(resourceURL,
                                                     "GET",
                                                     null,
                                                     ContentType.Json);
                Log.Info(string.Format("Unsubscribe strResult={0}", strResult), this);

                objResult = new JavaScriptSerializer().DeserializeObject(strResult);

                Console.WriteLine(string.Format("<p>Request for recipient in a group<br/>{0} {1} - OK</p>", "GET", resourceURL));

                items = (Dictionary<String, Object>)objResult;
                Object[] recipients = (Object[])items["Items"];
                for (int i = 0; i < recipients.Length; i++)
                {
                    Dictionary<String, Object> recipient = (Dictionary<String, Object>)recipients[i];
                    int recipientId = int.Parse(recipient["idRecipient"].ToString());
                    // Pick up a recipient and unsubscribe it

                    //resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + listId + "/Unsubscribe/" + recipientId;
                    resourceURL = "" + mail.ConsoleEndpoint + "/Console/Group/" + groupId + "/Unsubscribe/" + recipientId;

                    strResult = mail.CallMethod(resourceURL,
                                                         "DELETE",
                                                         null,
                                                         ContentType.Json);

                    Log.Info(string.Format("Unsubscribe delete strResult={0}", strResult), this);
                    Log.Info(String.Format("<p>Pick up a recipient and unsubscribe it<br/>{0} {1} - OK</p>", "DELETE", resourceURL), this);

                }
                ret = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Format("Unsubscribe error {0}", ex.Message));
            }
            return ret;
        }

        public bool BulkUnsubscribeFromGroup(string listId, string groupId)
        {
            bool ret = false;
            try
            {
                Log.Info(string.Format("BulkUnsubscribe from groupId={0}", groupId), this);
                String resourceURL = "";
                String strResult = "";
                Dictionary<String, Object> items = new Dictionary<String, Object>();

                resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + listId + "/Group/" + groupId;
                // resourceURL = "" + mail.ConsoleEndpoint + "/Console/Group/" + groupId + "/Unsubscribe/" + recipientId;

                strResult = mail.CallMethod(resourceURL,
                                                     "DELETE",
                                                     null,
                                                     ContentType.Json);

                Log.Info(string.Format("BulkUnsubscribe delete strResult={0}", strResult), this);
                Log.Info(String.Format("<p>Pick up a recipient and unsubscribe it<br/>{0} {1} - OK</p>", "DELETE", resourceURL), this);

            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Format("BulkUnsubscribe error {0}", ex.Message));
            }
            return ret;
        }
        //services.mailup.com/API/v1.1/Rest/ConsoleService.svc/Console/List/1/Group/139/Recipient

        public string GetEmails(string groupId)
        {
            Dictionary<String, Object> items = new Dictionary<String, Object>();
            // Get the list of the existing messages
            string resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + groupId + "/Emails";
            string strResult = mail.CallMethod(resourceURL,
                                                 "GET",
                                                 null,
                                                 ContentType.Json);
            Object objResult = new JavaScriptSerializer().DeserializeObject(strResult);
            items = (Dictionary<String, Object>)objResult;
            Object[] emails = (Object[])items["Items"];
            Dictionary<String, Object> email = (Dictionary<String, Object>)emails[0];
            int emailId = int.Parse(email["idMessage"].ToString());
            Console.WriteLine("subscribe result:{0}", strResult);
            Console.WriteLine("subscribe resourceURL:{0}", resourceURL);
            return "";
        }

        public bool SendNotification(string emailId, string groupId)
        {
            bool ret = false;
            try
            {
                Log.Info(string.Format("SendNotification emailId={0} groupId={1}", emailId, groupId), this);
                // Send email to all recipients in the list
                //POST https://services.mailup.com/API/v1.1/Rest/ConsoleService.svc/Console/Group/{id_Group}/Email/{id_Message}/Send
                string resourceURL = "" + mail.ConsoleEndpoint + "/Console/Group/" + groupId + "/Email/" + emailId + "/Send";
                string strResult = mail.CallMethod(resourceURL,
                                                     "POST",
                                                     null,
                                                     ContentType.Json);
                Log.Info(string.Format("SendNotification response {0}", strResult), this);
                ret = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex,"SendNotification error.");
            }
            return ret;
        }

        public string CreateMessage(string htmlBody, string listId, string subjectText)
        {
            string messageId = null;
            try
            {
                Log.Info(string.Format("CreateMessage listId={0} subjectText={1}", listId, subjectText), this);
                String resourceURL = "";
                String strResult = "";
                Object objResult;
                Dictionary<String, Object> items = new Dictionary<String, Object>();

                resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + listId + "/Email";

                EmailMessageItemDTO dto = new EmailMessageItemDTO();
                dto.Subject = subjectText;
                dto.idList = 1;
                dto.Content = htmlBody;
                dto.Embed = false;
                dto.IsConfirmation = false;
                dto.Fields = new List<EmailDynamicFieldDTO>();
                dto.Notes = "";
                dto.Tags = new List<EmailTagDTO>();
                dto.TrackingInfo = new EmailTrackingInfoDTO()
                {
                    CustomParams = "",
                    Enabled = false,
                    Protocols = new List<String>() { "http", "https" }
                };

                JavaScriptSerializer ser = new JavaScriptSerializer();

                String emailRequest = ser.Serialize(dto);
                strResult = mail.CallMethod(resourceURL,
                                                     "POST",
                                                     emailRequest,
                                                     ContentType.Json);
                objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                items = (Dictionary<String, Object>)objResult;
                Dictionary<String, Object> template = (Dictionary<String, Object>)objResult;
                messageId = template["idMessage"].ToString();
            }
            catch (Exception ex)
            {
                messageId = null;
                Log.Error(ex,"CreateMessage error.");
            }
            return messageId;
        }
        public string UploadImage(string imageData, string imageName, string listId)
        {
            string messageId = null;
            String resourceURL = "";
            String strResult = "";
            Object objResult;
            string imgSrc = "https://www.google.it/images/srpr/logo11w.png";
            Dictionary<String, Object> items = new Dictionary<String, Object>();

            // Create and save "hello" message
            //String message = "&lt;html&gt;&lt;body&gt;&lt;p&gt;Hello&lt;/p&gt;&lt;img src=\\\"" + imgSrc + "\\\"/&gt;&lt;/body&gt;&lt;/html&gt;";
            //            message = "<html><body><p>Hello this is a fresh message</p><img src=\"" + imgSrc + "\" /></body></html>";
            //message = "<html><body><p>Hello this is a fresh message</p></body></html>";

            resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + listId + "/Images";
            String imageRequest = "{\"Base64Data\":\"" + imageData + "\",\"Name\":\"" + imageName + "\"}";
            strResult = mail.CallMethod(resourceURL,
                                                 "POST",
                                                 imageRequest,
                                                 ContentType.Json);

            return messageId;
        }

        #region LIST
        public string CreateList(string listName)
        {
            try
            {
                Log.Info(string.Format("CreateList listName={0}", listName), this);
                String resourceURL = "";
                String strResult = "";
                Object objResult;
                EmailListItem eli = new EmailListItem();
                eli.Name = listName;
                resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/";

                JavaScriptSerializer ser = new JavaScriptSerializer();
                String emailRequest = ser.Serialize(eli);
                strResult = mail.CallMethod(resourceURL,
                                                     "POST",
                                                     emailRequest,
                                                     ContentType.Json);
                objResult = new JavaScriptSerializer().DeserializeObject(strResult);
            }
            catch (Exception ex)
            {
            }            return "";
        }
        public int SubscribeToList(string firstName, string lastName, string email, string phone, string listId, bool confirmationNeeded)
        {
            int ret = -1;
            try
            {
                Log.Info(string.Format("SubscribeToList userName={0}, email={1}, listId={2}, confirmationNeeded={3}", firstName, email, listId, confirmationNeeded), this);
                // Import recipients to group
                string resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + listId + "/Recipient";
                if (confirmationNeeded)
                    resourceURL += "?ConfirmEmail=true";
//                String recipientRequest = "{\"Email\":\"" + email + "\"}";
                String recipientRequest = "{\"Email\":\"" + email + "\",\"MobileNumber\":\"" + phone + "\",\"Fields\":[{\"Description\":\"FirstName\",\"Id\":1,\"Value\":\"" + firstName + "\"},{\"Description\":\"LastName\",\"Id\":2,\"Value\":\"" + lastName + "\"}]}";
                string strResult = mail.CallMethod(resourceURL,
                                        "POST",
                                        recipientRequest,
                                        ContentType.Json);
                int importId = int.Parse(strResult);
                /*
                resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + listId + "/Subscribe/"+importId;
                strResult = mail.CallMethod(resourceURL,
                                        "POST",
                                        null,
                                        ContentType.Json);
                                        */
                Log.Info(string.Format("SubscribeToList response {0}", strResult), this);
                ret = importId;
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Format("SubscribeToList error {0}", ex.Message));
            }
            return ret;
        }
        public bool UnsubscribeFromList(string email, string listId)
        {
            bool ret = false;
            try
            {
                Log.Info(string.Format("UnsubscribeFromList email={0}, listId={1}", email, listId), this);
                String resourceURL = "";
                String strResult = "";
                Object objResult;
                Dictionary<String, Object> items = new Dictionary<String, Object>();

                // Request for recipient in a group
                resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + listId + "/Recipients";
                strResult = mail.CallMethod(resourceURL,
                                                     "GET",
                                                     null,
                                                     ContentType.Json);
                Log.Info(string.Format("UnsubscribeFromList strResult={0}", strResult), this);

                objResult = new JavaScriptSerializer().DeserializeObject(strResult);

                Console.WriteLine(string.Format("<p>Request for recipient in a group<br/>{0} {1} - OK</p>", "GET", resourceURL));

                items = (Dictionary<String, Object>)objResult;
                Object[] recipients = (Object[])items["Items"];
                for (int i = 0; i < recipients.Length; i++)
                {
                    Dictionary<String, Object> recipient = (Dictionary<String, Object>)recipients[i];
                    if (recipient["Email"].ToString().ToLower() == email.ToLower())
                    {
                        int recipientId = int.Parse(recipient["idRecipient"].ToString());
                        // Pick up a recipient and unsubscribe it
                        resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + listId + "/Unsubscribe/" + recipientId;
                        strResult = mail.CallMethod(resourceURL,
                                                             "DELETE",
                                                             null,
                                                             ContentType.Json);
                        Log.Info(string.Format("UnsubscribeFromList delete strResult={0}", strResult), this);
                        Log.Info(String.Format("<p>Pick up a recipient and unsubscribe it<br/>{0} {1} - OK</p>", "DELETE", resourceURL), this);
                    }
                }
                ret = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Format("Unsubscribe error {0}", ex.Message));
            }
            return ret;
        }
        public bool UnsubscribeAllFromList(string listId, string option= "EmailOptins")
        {

            bool ret = false;
            try
            {
                Log.Info(string.Format("UnsubscribeFromList all, listId={0}", listId), this);
                String resourceURL = "";
                String strResult = "";
                Object objResult;
                Dictionary<String, Object> items = new Dictionary<String, Object>();
                // Request for recipient in a group
                resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + listId + "/Recipients/" + option;
                strResult = mail.CallMethod(resourceURL,
                                                     "GET",
                                                     null,
                                                     ContentType.Json);
                Log.Info(string.Format("UnsubscribeFromList strResult={0}", strResult), this);

                objResult = new JavaScriptSerializer().DeserializeObject(strResult);

                Console.WriteLine(string.Format("<p>Request for recipient in a group<br/>{0} {1} - OK</p>", "GET", resourceURL));

                items = (Dictionary<String, Object>)objResult;
                Object[] recipients = (Object[])items["Items"];
                for (int i = 0; i < recipients.Length; i++)
                {
                    if (i > 1) continue;

                    Dictionary<String, Object> recipient = (Dictionary<String, Object>)recipients[i];
                    int recipientId = int.Parse(recipient["idRecipient"].ToString());
                    // Pick up a recipient and unsubscribe it
                    resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + listId + "/Unsubscribe/" + recipientId;
                    strResult = mail.CallMethod(resourceURL,
                                                         "DELETE",
                                                         null,
                                                         ContentType.Json);
                    Log.Info(string.Format("UnsubscribeFromList delete strResult={0}", strResult), this);
                    Log.Info(String.Format("<p>Pick up a recipient and unsubscribe it<br/>{0} {1} - OK</p>", "DELETE", resourceURL), this);

                }
                ret = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Format("Unsubscribe error {0}", ex.Message));
            }
            return ret;
        }
        public bool UnsubscribeFromListWithId(string email, string listId, string recipientId)
        {
            bool ret = false;
            try
            {
                Log.Info(string.Format("UnsubscribeFromListWithId email={0}, listId={1}, recipientId={2}", email, listId, recipientId), this);
                String resourceURL = "";
                String strResult = "";
                Dictionary<String, Object> items = new Dictionary<String, Object>();

                // Pick up a recipient and unsubscribe it
                resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + listId + "/Unsubscribe/" + recipientId;
                strResult = mail.CallMethod(resourceURL,
                                                        "DELETE",
                                                        null,
                                                        ContentType.Json);
                Log.Info(string.Format("UnsubscribeFromListWithId delete strResult={0}", strResult), this);
                Log.Info(String.Format("<p>Pick up a recipient and unsubscribe it<br/>{0} {1} - OK</p>", "DELETE", resourceURL), this);
                ret = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, string.Format("Unsubscribe error {0}", ex.Message));
            }
            return ret;
        }

        #endregion
        #region GROUPS
        public string CreateGroup(string listId, string groupName, string groupNotes = null)
        {
            string groupId = null;
            try
            {
                Log.Info(string.Format("CreateGroup listId={0} groupName={1}",listId, groupName), this);
                String resourceURL = "";
                String strResult = "";
                Object objResult;
                EmailGroupItem egi = new EmailGroupItem();
                egi.Name = groupName;
                egi.Notes = groupNotes;
                egi.Deletable = true;
                resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/"+listId+"/Group";

                JavaScriptSerializer ser = new JavaScriptSerializer();
                String emailRequest = ser.Serialize(egi);
                strResult = mail.CallMethod(resourceURL,
                                                     "POST",
                                                     emailRequest,
                                                     ContentType.Json);
                objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                Dictionary<String, Object> item = (Dictionary<String, Object>)objResult;
                groupId = item["idGroup"].ToString();

            }
            catch (Exception ex)
            {
                Log.Error(ex, "CreateGroup error");
            }
            return groupId;
        }
        public string UpdateGroup(string listId, string groupId, string groupName, string groupNotes = null)
        {
            string result = null;
            try
            {
                Log.Info(string.Format("UpdateGroup listId={0} groupId={1}", listId, groupId), this);
                String resourceURL = "";
                String strResult = "";
                Object objResult;
                EmailGroupItem egi = new EmailGroupItem();
                egi.Name = groupName;
                egi.Notes = groupNotes;
                resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + listId + "/Group/"+groupId;

                JavaScriptSerializer ser = new JavaScriptSerializer();
                String emailRequest = ser.Serialize(egi);
                strResult = mail.CallMethod(resourceURL,
                                                     "PUT",
                                                     emailRequest,
                                                     ContentType.Json);
                objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                Dictionary<String, Object> item = (Dictionary<String, Object>)objResult;
                result = item["idGroup"].ToString();

            }
            catch (Exception ex)
            {
                Log.Error(ex, "UpdateGroup error");
            }
            return result;
        }
        public bool DeleteGroup(string listId, string groupId)
        {
            try
            {
                Log.Info(string.Format("DeleteGroup listId={0} groupId={1} ", listId, groupId), this);
                String resourceURL = "";
                String strResult = "";
                Object objResult;
                resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + listId + "/Group/"+ groupId;

                JavaScriptSerializer ser = new JavaScriptSerializer();
                strResult = mail.CallMethod(resourceURL,
                                                     "DELETE",
                                                     null,
                                                     ContentType.Json);
                objResult = new JavaScriptSerializer().DeserializeObject(strResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DeleteGroup error");
                return false;
            }
            return true;
        }
        public List<EmailGroupItem> GetGroups(string listId)
        {
            List<EmailGroupItem> groups = new List<EmailGroupItem>();
            try
            {
                Log.Info(string.Format("GetGroups listId={0}", listId), this);
                String resourceURL = "";
                String strResult = "";
                Object objResult;
                resourceURL = "" + mail.ConsoleEndpoint + "/Console/List/" + listId + "/Groups";

                JavaScriptSerializer ser = new JavaScriptSerializer();
                strResult = mail.CallMethod(resourceURL,
                                                     "GET",
                                                     null,
                                                     ContentType.Json);
                objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                Dictionary<String, Object> item = (Dictionary<String, Object>)objResult;
                Object[] list = (Object[])item["Items"];
                //todo if we need the list we have to cast or map it
                foreach (Object obj in list)
                {
                    Dictionary<String, Object> itm = (Dictionary<String, Object>)obj;
                    groups.Add(new EmailGroupItem() {
                        Deletable = Convert.ToBoolean(itm["Deletable"]),
                        idGroup = itm["idGroup"].ToString(),
                        idList = itm["idList"].ToString(),
                        Name = itm["Name"].ToString(),
                        Notes = itm["Notes"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CreateGroup error");
            }
            return groups;
        }
        #endregion
        #endregion

    }
}
