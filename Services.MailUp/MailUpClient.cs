using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Services.MailUp
{
    public enum ContentType
    {
        Json,
        Xml
    }

    public class MailUpException : Exception
    {
        private int statusCode;
        public int StatusCode
        {
            set { statusCode = value; }
            get { return statusCode; }
        }

        public MailUpException(int statusCode, String message) : base(message)
        {
            this.StatusCode = statusCode;
        }
    }

    class MailUpClient
    {
        private String logonEndpoint = "https://services.mailup.com/Authorization/OAuth/LogOn";
        public String LogonEndpoint
        {
            get { return logonEndpoint; }
            set { logonEndpoint = value; }
        }

        private String authorizationEndpoint = "https://services.mailup.com/Authorization/OAuth/Authorization";
        public String AuthorizationEndpoint
        {
            get { return authorizationEndpoint; }
            set { authorizationEndpoint = value; }
        }

        private String tokenEndpoint = "https://services.mailup.com/Authorization/OAuth/Token";
        public String TokenEndpoint
        {
            get { return tokenEndpoint; }
            set { tokenEndpoint = value; }
        }

        private String consoleEndpoint = "https://services.mailup.com/API/v1.1/Rest/ConsoleService.svc";
        public String ConsoleEndpoint
        {
            get { return consoleEndpoint; }
            set { consoleEndpoint = value; }
        }

        private String mailstatisticsEndpoint = "https://services.mailup.com/API/v1.1/Rest/MailStatisticsService.svc";
        public String MailstatisticsEndpoint
        {
            get { return mailstatisticsEndpoint; }
            set { mailstatisticsEndpoint = value; }
        }
        private string ClientId { get; set; }

        public string ClientSecret { get; set; }
        public string CallbackUri { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }


        public MailUpClient(String clientId, String clientSecret, String callbackUri)
        {
            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.CallbackUri = callbackUri;
            //LoadToken();
        }
        private String GetContentTypeString(ContentType cType)
        {
            if (cType == ContentType.Json) return "application/json";
            else return "application/xml";
        }
        private String ExtractJsonValue(String json, String name)
        {
            String delim = "\"" + name + "\":\"";
            int start = json.IndexOf(delim) + delim.Length;
            int end = json.IndexOf("\"", start + 1);
            if (end > start && start > -1 && end > -1) return json.Substring(start, end - start);
            else return "";
        }
        public void LogOnWithUsernamePassword(String username, String password)
        {
            this.RetreiveAccessToken(username, password);
        }
        public String RetreiveAccessToken(String login, String password)
        {
            int statusCode = 0;
            try
            {
                CookieContainer cookies = new CookieContainer();

                String body = "client_id=" + ClientId + "&client_secret=" + ClientSecret + "&grant_type=password&username=" + login + "&password=" + password;
                HttpWebRequest wrLogon = (HttpWebRequest)WebRequest.Create(tokenEndpoint);
                wrLogon.CookieContainer = cookies;
                wrLogon.AllowAutoRedirect = false;
                wrLogon.KeepAlive = true;
                wrLogon.Method = "POST";
                wrLogon.ContentType = "application/x-www-form-urlencoded";

                String auth = String.Format("{0}:{1}", ClientId, ClientSecret);
                wrLogon.Headers["Authorization"] = "Basic " + Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(auth));

                byte[] byteArray = Encoding.UTF8.GetBytes(body);
                wrLogon.ContentLength = byteArray.Length;
                Stream dataStream = wrLogon.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                HttpWebResponse tokenResponse = (HttpWebResponse)wrLogon.GetResponse();
                statusCode = (int)tokenResponse.StatusCode;
                Stream objStream = tokenResponse.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                String json = objReader.ReadToEnd();
                tokenResponse.Close();

                AccessToken = ExtractJsonValue(json, "access_token");
                RefreshToken = ExtractJsonValue(json, "refresh_token");

            }
            catch (WebException wex)
            {
                HttpWebResponse wrs = (HttpWebResponse)wex.Response;
                throw new MailUpException((int)wrs.StatusCode, wex.Message);
            }
            catch (Exception ex)
            {
                throw new MailUpException(statusCode, ex.Message);
            }
            return AccessToken;
        }

        public String CallMethod(String url, String verb, String body, ContentType contentType = ContentType.Json)
        {
            return CallMethod(url, verb, body, contentType, true);
        }
        private String CallMethod(String url, String verb, String body, ContentType contentType = ContentType.Json, bool refresh = true)
        {
            String result = "";
            HttpWebResponse callResponse = null;
            int statusCode = 0;
            try
            {

                HttpWebRequest wrLogon = (HttpWebRequest)WebRequest.Create(url);
                wrLogon.AllowAutoRedirect = false;
                wrLogon.KeepAlive = true;
                wrLogon.Method = verb;
                wrLogon.ContentType = GetContentTypeString(contentType);
                wrLogon.ContentLength = 0;
                wrLogon.Accept = GetContentTypeString(contentType);
                wrLogon.Headers.Add("Authorization", "Bearer " + AccessToken);

                if (body != null && body != "")
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes(body);
                    wrLogon.ContentLength = byteArray.Length;
                    Stream dataStream = wrLogon.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }

                callResponse = (HttpWebResponse)wrLogon.GetResponse();
                statusCode = (int)callResponse.StatusCode;
                Stream objStream = callResponse.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                result = objReader.ReadToEnd();
                callResponse.Close();
            }
            catch (WebException wex)
            {
                try
                {
                    HttpWebResponse wrs = (HttpWebResponse)wex.Response;
                    if ((int)wrs.StatusCode == 401 && refresh)
                    {
                        RefreshAccessToken();
                        return CallMethod(url, verb, body, contentType, false);
                    }
                    else throw new MailUpException((int)wrs.StatusCode, wex.Message);
                }
                catch (Exception ex)
                {
                    throw new MailUpException(statusCode, ex.Message);
                }
            }
            catch (Exception ex)
            {
                throw new MailUpException(statusCode, ex.Message);
            }
            return result;
        }
        public String RefreshAccessToken()
        {
            int statusCode = 0;
            try
            {
                HttpWebRequest wrLogon = (HttpWebRequest)WebRequest.Create(tokenEndpoint);
                wrLogon.AllowAutoRedirect = false;
                wrLogon.KeepAlive = true;
                wrLogon.Method = "POST";
                wrLogon.ContentType = "application/x-www-form-urlencoded";

                String body = "client_id=" + ClientId + "&client_secret=" + ClientSecret +
                    "&refresh_token=" + RefreshToken + "&grant_type=refresh_token";
                byte[] byteArray = Encoding.UTF8.GetBytes(body);
                wrLogon.ContentLength = byteArray.Length;
                Stream dataStream = wrLogon.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                HttpWebResponse refreshResponse = (HttpWebResponse)wrLogon.GetResponse();
                statusCode = (int)refreshResponse.StatusCode;
                Stream objStream = refreshResponse.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                String json = objReader.ReadToEnd();
                refreshResponse.Close();

                AccessToken = ExtractJsonValue(json, "access_token");
                RefreshToken = ExtractJsonValue(json, "refresh_token");
            }
            catch (WebException wex)
            {
                HttpWebResponse wrs = (HttpWebResponse)wex.Response;
                throw new MailUpException((int)wrs.StatusCode, wex.Message);
            }
            catch (Exception ex)
            {
                throw new MailUpException(statusCode, ex.Message);
            }
            return AccessToken;
        }

    }
}
