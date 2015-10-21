#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

#endregion

namespace Azure.Updater
{
    public class Main
    {
        public static string updater = string.Format("{0}{1}", Application.StartupPath, @"\Updater.exe");

        public static List<string> info = new List<string>();

        public const string updaterPrefix = "M1234_";
        public const string updateSuccess = "UpdateMe has been successfully updated";
        public const string updateCurrent = "No updates available for UpdateMe";
        public const string updateInfoError = "Error in retrieving UpdateMe information";
        public string downloadUrl = "http://localhost/";
        public string versionfilename = "DebugUpdate.txt";
        public string thisversion;

        private static readonly string processToEnd = "Azure";
        private static readonly string postProcess = string.Format("{0}{1}{2}.exe", Application.StartupPath, @"\", processToEnd);

        public void CheckUpdates()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            thisversion = fvi.FileVersion;
            info = Update.getUpdateInfo(downloadUrl, versionfilename, string.Format("{0}{1}", Application.StartupPath, @"\"), 1);

            if (info == null)
            {
                Console.WriteLine(updateInfoError);
            }
            else
            {
                if (decimal.Parse(info[1]) > decimal.Parse(thisversion))
                {
                    Console.WriteLine(">> Azure Needs to Be Updated, Please Digit a Key to Exit..");
                    Console.ReadLine();
                    DoUpdate();
                    LoadUpdate();
                    Environment.Exit(1);
                }
                else
                {
                    Console.WriteLine(updateCurrent);
                }
            }
        }

        public void LoadUpdate()
        {
            Update.updateMe(updaterPrefix, string.Format("{0}{1}", Application.StartupPath, @"\"));
            unpackCommandline();
        }

        public void DoUpdate()
        {
            Update.installUpdateRestart(info[3], info[4], string.Format("\"{0}\\", Application.StartupPath), processToEnd, postProcess, "updated", updater);
        }

        private void unpackCommandline()
        {
            bool commandPresent = false;
            string tempStr = string.Empty;

            foreach (string arg in Environment.GetCommandLineArgs())
            {
                if (!commandPresent)
                {
                    commandPresent = arg.Trim().StartsWith("/");
                }

                if (commandPresent)
                {
                    tempStr += arg;
                }
            }

            if (commandPresent)
            {
                if (tempStr.Remove(0, 2) == "updated")
                {
                    Console.WriteLine(updateSuccess);
                }
            }
        }
    }
}