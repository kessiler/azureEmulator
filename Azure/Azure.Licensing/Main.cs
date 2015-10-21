#region

using System;
using System.IO;
using System.Management;
using System.Windows.Forms;

#endregion

namespace Azure.Licensing
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private string getUniqueID(string drive)
        {
            if (drive == string.Empty)
            {
                //Find first drive
                foreach (DriveInfo compDrive in DriveInfo.GetDrives())
                {
                    if (compDrive.IsReady)
                    {
                        drive = compDrive.RootDirectory.ToString();
                        break;
                    }
                }
            }

            if (drive.EndsWith(":\\"))
            {
                //C:\ -> C
                drive = drive.Substring(0, drive.Length - 2);
            }

            string volumeSerial = getVolumeSerial(drive);
            string cpuID = getCPUID();

            //Mix them up and remove some useless 0's
            return string.Format("{0}{1}{2}{3}", cpuID.Substring(13), cpuID.Substring(1, 4), volumeSerial, cpuID.Substring(4, 4));
        }

        private string getVolumeSerial(string drive)
        {
            ManagementObject disk = new ManagementObject(string.Format("{0}{1}{2}", @"win32_logicaldisk.deviceid=""", drive, @":"""));
            disk.Get();

            string volumeSerial = disk["VolumeSerialNumber"].ToString();
            disk.Dispose();

            return volumeSerial;
        }

        private string getCPUID()
        {
            string cpuInfo = string.Empty;
            ManagementClass managClass = new ManagementClass("win32_processor");
            ManagementObjectCollection managCollec = managClass.GetInstances();

            foreach (ManagementObject managObj in managCollec)
            {
                if (cpuInfo == "")
                {
                    //Get only the first CPU's ID
                    cpuInfo = managObj.Properties["processorID"].Value.ToString();
                    break;
                }
            }

            return cpuInfo;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtID.Text = getUniqueID("C");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
        }

        private void txtID_TextChanged(object sender, EventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
    }
}