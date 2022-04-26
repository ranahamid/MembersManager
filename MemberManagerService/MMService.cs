using Services.MMLogger;
using Services.MMSync;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MemberManagerService.Job
{
    public partial class MMService : ServiceBase
    {
        FileSystemWatcher myWatcher = null;
        string watchFolder = null;
        public MMService()
        {
            InitializeComponent();
            Log.Init(ConfigurationManager.AppSettings["LogPath"], Log.ToLogLevel(ConfigurationManager.AppSettings["LogLevel"]));
            watchFolder = ConfigurationManager.AppSettings["WatchFolder"];
        }
        protected void FileCreated(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                // a directory
                if (Directory.Exists(e.FullPath)) {
                    Log.Info(string.Format("FolderCreated {0}",e.FullPath), this);
                }
                // a file
                else {
                    Log.Info(string.Format("FileCreated {0}", e.FullPath), this);
                    // process csv
                    MMSyncronize mm = new MMSyncronize();
                    bool ret = mm.Process(e.FullPath);
                    if (ret)
                    {// move file to some temp folder
                        Log.Info(string.Format("FileMoved from {0}", e.FullPath), this);
                        File.Move(e.FullPath, watchFolder + "\\temp\\" + DateTime.Now.ToString("yyyy-MM-dd-") + e.Name);
                        Log.Info(string.Format("FileMoved to {0}", watchFolder + "\\temp\\" + DateTime.Now.ToString("yyyy-MM-dd-") + e.Name), this);
                    }
                }
            }
        }
        protected override void OnStart(string[] args)
        {
            Log.Info(string.Format("OnStart"), this);
            try
            {
                myWatcher = new FileSystemWatcher(watchFolder, ConfigurationManager.AppSettings["WatchFileType"]);
                myWatcher.NotifyFilter = NotifyFilters.LastAccess
                                       | NotifyFilters.LastWrite
                                       | NotifyFilters.FileName
                                       | NotifyFilters.DirectoryName;
                myWatcher.EnableRaisingEvents = true;
                myWatcher.IncludeSubdirectories = false;
                myWatcher.Created += new FileSystemEventHandler(FileCreated);
            }
            catch (Exception ex) {
                Log.Error(ex);
            }
        }

        protected override void OnStop()
        {
            Log.Info(string.Format("OnStop"), this);
        }
    }
}
