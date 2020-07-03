using CG.Web.MegaApiClient;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using System.Windows.Forms;
using System.IO.Compression;

namespace WindowsFormsApp1
{
    public partial class bnsStuff : Form
    {
        string BNSPATH= (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\NCWest\\BnS", "BaseDir", null);
        string path = "temp";
        string megaLink = "https://mega.nz/folder/WXhzUZ7Y#XzlqkPa8DU4X8xrILQDdZA";
        string docs= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"BnS");
        public bnsStuff()
        {
            InitializeComponent();
            if (BNSPATH != "")
                textBox1.Text = textBox1.Text + "\nfound BNS PATH: " + BNSPATH;
            else
                textBox1.Text = textBox1.Text + "\nerror: could not find BNS PATH";
            string command = "/C powershell -Command \"$temp=Get-MpPreference;echo $temp.ExclusionPath\"";
            string t=executeCMD(command);
            if(t.Contains("Blade & Soul"))
                changeActive();
        }
        private void changeActive()
        {
            button1.Enabled = !button1.Enabled;
            button2.Enabled = !button2.Enabled;
            checkBox1.Enabled = !checkBox1.Enabled;
            checkBox2.Enabled = !checkBox2.Enabled;
        }
        private string executeCMD(string command)
        {
            string output = "";
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = command;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            output = process.StandardOutput.ReadToEnd();
            return output;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string command = "/C powershell -Command \"Add-MpPreference -ExclusionPath \'"+BNSPATH+"\'\"";
            executeCMD(command);
            changeActive();
        }
        private void directoryCopy(string src, string dest)
        {

            DirectoryInfo dir = new DirectoryInfo(src);
            FileInfo[] files = dir.GetFiles();
            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(dest))
                Directory.CreateDirectory(dest);
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(dest, file.Name);
                file.CopyTo(temppath, true);
            }
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(dest, subdir.Name);
                directoryCopy(subdir.FullName, temppath);
            }
        }
        private void clearFolder(string FolderName,bool deleteRoot)
        {
            DirectoryInfo dir = new DirectoryInfo(FolderName);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                clearFolder(di.FullName,false);
                di.Delete();
            }
            if (deleteRoot)
                dir.Delete();
        }
        private void downloadFiles()
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            textBox1.Text = textBox1.Text + "\r\ndownloading the latest version...";
            MegaApiClient mega = new MegaApiClient();
            mega.LoginAnonymous();
            Uri link = new Uri(megaLink);
            IEnumerable<INode> nodes = mega.GetNodesFromLink(link);
            foreach (INode node in nodes.Where(x => x.Type == NodeType.File))
            {
                if (node.ParentId == "OG4B2D6S" || node.ParentId == "KSpkxCwB"||File.Exists(Path.Combine(path,node.Name)))
                    continue;
                mega.DownloadFile(node, Path.Combine(path, node.Name));
                textBox1.Text = textBox1.Text + ".";
            }

            mega.Logout();
            textBox1.Text = textBox1.Text + "\r\nfinished downloading";
        }
        private void install()
        {
            try
            {
                if (checkBox1.Checked)
                {
                    directoryCopy(Path.Combine(path, "bin"), Path.Combine(BNSPATH, "bin"));
                    textBox1.Text = textBox1.Text + "\r\nsuccesfully installed BNSPATCH for 32bit";
                }
                if (checkBox2.Checked)
                {
                    directoryCopy(Path.Combine(path, "bin64"), Path.Combine(BNSPATH, "bin64"));
                    textBox1.Text = textBox1.Text + "\r\nsuccesfully installed BNSPATCH for 64bit";
                }
                if (checkBox1.Checked || checkBox2.Checked)
                {
                    FileInfo f = new FileInfo(Path.Combine(Path.GetFullPath(path),"patches.xml"));
                    f.CopyTo(Path.Combine(docs,f.Name),true);
                    string addons = Path.Combine(docs, "addons");
                    if (!Directory.Exists(addons))
                        Directory.CreateDirectory(addons);
                    string patches = Path.Combine(docs, "patches");
                    if (!Directory.Exists(patches))
                        Directory.CreateDirectory(patches);
                }
                clearFolder(path, true);
            }
            catch (Exception ex)
            {
                textBox1.Text = textBox1.Text + "\r\nerror";
            }
        }
        private void extract()
        {
            string path2 = Path.Combine(path, "temp");
            DirectoryInfo dir = new DirectoryInfo(path);
            if (!Directory.Exists(path2))
                Directory.CreateDirectory(Path.Combine(path, "temp"));
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (file.Extension == ".zip")
                {
                    ZipFile.ExtractToDirectory(Path.Combine(path, file.FullName), path2);
                    directoryCopy(path2, path);
                    clearFolder(path2, false);
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            string temp = Path.GetFullPath(path);
            string command = "/C powershell -Command \"Add-MpPreference -ExclusionPath \'" + temp + "\'\"";
            executeCMD(command);
            downloadFiles();
            extract();
            install();
        }
    }
}
