using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.MMSync;
using Services.MailUp;
using System.Configuration;

namespace Test.MMSync
{
    class Program
    {
        static void Main(string[] args)
        {
            Services.MMSync.MMSyncronize mms = new Services.MMSync.MMSyncronize();
            //mms.Process(@"d:\Projects\Jp\MM\0.csv");
            mms.Process(@"d:\Projects\Jp\MM\2s.csv");

            MailUp mp = new MailUp();
//            string auth = mp.Authorize(ConfigurationManager.AppSettings["MailUpUser"], ConfigurationManager.AppSettings["MailUpPassword"]);
            //string id = mp.CreateGroup("4", "mihaitest", "hello");
            //mp.DeleteGroup("4", id);
            //mp.GetGroups("4");
            //mp.UpdateGroup("4", "30", "new jp test 01");
        }
    }
}
