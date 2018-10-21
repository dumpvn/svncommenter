using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Security;
using Microsoft.VisualBasic;

using PMCore.Security;
using PMCore.Console;

using SVNCommenter.Classes;

namespace SVNCommenter
{
    public partial class Form1 : Form
    {
        #region Variables
        
        ProjectDA pDA;
        Form frmPopup;
        List<Project> projects;
        DataSet xmlDS = new DataSet();

        string dbLocation = "";
        string consoleOutput = "";
        string url = "";
        int revNumber = -1;
        int ticker = 0;
        int mode = 0;
        int lastIndex = 0;
        string hookScript = "";

        #endregion

        public Form1()
        {
            InitializeComponent();
            ConsoleReader.OutputChanged += ConsoleReader_OutputChanged;
            ConsoleReader.ErrorOccured += ConsoleReader_ErrorOccured;
        }

        #region Form Events

        private void Form1_Load(object sender, EventArgs e)
        {
            dbLocation = Application.StartupPath + @"\projects.xml";
            pDA = new ProjectDA(dbLocation);
            frmPopup = new Form();
            frmPopup.ClientSize = new System.Drawing.Size(276, 160);
            SetupHookScript();

            if (!File.Exists(dbLocation))
            {
                CreateNewDB();
                LoadProjects();
            }
            else
            {
                if (DoLogin())
                {
                    LoadProjects();
                }
                else
                {
                    MessageBox.Show("Sorry.. without an Admin password the projects cannot be loaded.");
                    Application.Exit();
                }
            }
        }

        private void lstProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Clear Grids
            ClearOutput();

            lastIndex = lstProjects.SelectedIndex;
            rtbStatus.Text = string.Format("Getting Logs for {0} ", lstProjects.Text);
            SVNGetRepoLog();
        }

        private void dgvLogEntries_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvLogEntries.RowCount > 0 && dgvLogEntries.SelectedRows.Count > 0)
            {
                DataGridViewRow dgvRow = dgvLogEntries.SelectedRows[0];

                if (dgvRow.Cells.Count > 0)
                {
                    rtbComment.Text = dgvRow.Cells[2].Value.ToString();
                    revNumber = Convert.ToInt32(dgvLogEntries.SelectedRows[0].Cells[3].Value);

                    rtbStatus.Text = string.Format("Viewing {0} Log for Revision {1}", lstProjects.Text, revNumber);

                    DisplayRevisionFiles();
                }
            }
        }

        private void btnSaveComment_Click(object sender, EventArgs e)
        {
            SVNChangeComment(url, revNumber, rtbComment.Text);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            frmPopup = new Form();
            frmPopup.Icon = this.Icon;
            frmPopup.Text = "Add new Project";
            frmPopup.ClientSize = new System.Drawing.Size(280, 165);
            ucProject prj = new ucProject(null,dbLocation);
            frmPopup.Controls.Add(prj);
            frmPopup.ShowDialog();

            //Reload the projects
            LoadProjects();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lstProjects.SelectedIndex >= 0)
            {
                frmPopup = new Form();
                frmPopup.Icon = this.Icon;
                frmPopup.Text = "Edit Project";
                frmPopup.ClientSize = new System.Drawing.Size(280, 165);
                Project proj = projects[lstProjects.SelectedIndex];
                ucProject prj = new ucProject(proj, dbLocation);
                frmPopup.Controls.Add(prj);
                frmPopup.ShowDialog();

                //Reload the projects
                LoadProjects();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            Project proj = projects[lstProjects.SelectedIndex];
            string projectName = proj.Name;

            if(MessageBox.Show(string.Format("Are you sure you want to delete this project: {0}",projectName),"Delete Project",MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                pDA.DeleteProjectFromDB(projectName);
            }

            lastIndex = 0;

            //Reload the projects
            LoadProjects();
        }

        private void dgvFiles_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string svnURL = "";

            if (e.ColumnIndex == 2)
            {
                svnURL = dgvFiles[2, e.RowIndex].Value.ToString();
                System.Diagnostics.Process.Start(svnURL);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void administrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadAdministration();
        }

        #endregion

        #region Timers

        private void waitTimer_Tick(object sender, EventArgs e)
        {
            ticker++;

            //After a couple of seconds of console inactivity analyze the output
            if (ticker == Properties.Settings.Default.SVNConsoleTimeout)
            {
                AnalyzeOutput(consoleOutput);
                waitTimer.Enabled = false;
                ticker = 0;
            }
        }

        #endregion

        #region Methods

        private bool DoLogin()
        {
            frmPopup = new Form();
            frmPopup.StartPosition = FormStartPosition.CenterScreen;
            frmPopup.Icon = this.Icon;
            frmPopup.Text = "Login";
            frmPopup.ClientSize = new System.Drawing.Size(221, 77);

            ucLogin lgi = new ucLogin(dbLocation);

            frmPopup.Controls.Add(lgi);
            frmPopup.ShowDialog();

            return lgi.LoginState;
        }

        private void LoadAdministration()
        {
            frmPopup = new Form();
            frmPopup.Icon = this.Icon;
            frmPopup.Text = "Administration";

            frmPopup.ClientSize = new System.Drawing.Size(275, 88);

            Login lgnAdmin = pDA.LoadLogin();


            if (lgnAdmin != null)
            {
                ucAdministration adm = new ucAdministration(lgnAdmin, dbLocation);

                frmPopup.Controls.Add(adm);
                frmPopup.ShowDialog();
            }
        }

        private void SetupHookScript()
        {
            hookScript = "----------------------------\n" +
                        "rem Only allow log messages to be changed.\n" +
                        "if \"%4\" == \"svn:log\" exit 0\n" +
                        "echo Property '%4' cannot be changed >&2\n" +
                        "exit 1\n" +
                        "----------------------------\n";
        }

        private void ClearOutput()
        {
            dgvLogEntries.DataSource = null;
            dgvLogEntries.Refresh();

            dgvFiles.DataSource = null;
            dgvFiles.Refresh();

            rtbComment.Text = "";

            Application.DoEvents();
        }

        private void CreateNewDB()
        {
            if (pDA.CreateNewDB())
            {
                MessageBox.Show("New Test database created. Login password is 'password'.");
            }
        }

        private void LoadProjects()
        {
            projects = new List<Project>();
            lstProjects.Items.Clear();

            try
            {
                projects = pDA.LoadProjects();

                if (projects.Count > 0)
                {
                    foreach (Project project in projects)
                    {
                        lstProjects.Items.Add(project.Name);
                    }

                    if (lstProjects.Items.Count <= lastIndex)
                    {
                        lstProjects.SelectedIndex = lastIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void SVNGetRepoLog()
        {
            if (projects.Count > 0)
            {
                url = projects[lstProjects.SelectedIndex].URL;
                string uName = projects[lstProjects.SelectedIndex].UserName;
                string pWord = projects[lstProjects.SelectedIndex].Password;
                string salt = projects[lstProjects.SelectedIndex].Salt;
                //Decrypt the Password
                if (pWord != string.Empty && salt != string.Empty)
                {
                    pWord = EncryptionUtils.Decrypt(pWord, "SVNCommenter2013", salt);
                }

                SVNGetRepoLog(url,uName,pWord);
            }
        }

        #endregion

        #region SVN Commands

        private void SVNGetRepoLog(string repoURL,string uName,string pWord)
        {
            mode = 0;
            consoleOutput = "";
            string cmd = "log";
            string credentials = "";

            if (uName != string.Empty && pWord != string.Empty)
            {
                credentials = string.Format(" --username {0} --password {1}", uName, pWord);
            }

            waitTimer.Enabled = true;
            rtbComment.Text = "";
            revNumber = -1;

            progressBar1.Visible = true;
            Application.DoEvents();

            //Build the SVN command
            cmd = cmd + " " + repoURL + credentials + " --verbose --xml";

            ConsoleReader.RunCommand("svn.exe",cmd ); 
        }

        private void SVNChangeComment(string repoURL, int revision, string newComment)
        {
            mode = 1;
            consoleOutput = "";

            //Write the Log Message to file
            string commentPath = Application.StartupPath + @"\comment.txt";
            StreamWriter sw = new StreamWriter(commentPath, false, Encoding.ASCII);
            //Write each line to the file as SVN complains about inconsistent line endings
            foreach (string line in newComment.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                sw.WriteLine(line);
            }
            sw.Close();

            //Build The Command to update the log from a file
            string cmd = string.Format("propset --revprop -r {0} svn:log -F \"{1}\" {2}", revision, commentPath, repoURL);

            waitTimer.Enabled = true;
            progressBar1.Visible = true;
            Application.DoEvents();
            //Run the command
            ConsoleReader.RunCommand("svn.exe", cmd);
        }

        #endregion

        #region SVNResponce

        void ConsoleReader_ErrorOccured(string errorMsg)
        {
            if (errorMsg != string.Empty && errorMsg != null)
            {
                ticker = 0; //Reset the wait timer
                consoleOutput += errorMsg + "\n";
            }
        }

        void ConsoleReader_OutputChanged(string output)
        {
            ticker = 0; //Reset the wait timer
            consoleOutput += output + "\n";
        }

        private void AnalyzeOutput(string output)
        {
            switch (mode)
            {
                case 0: //Log reader
                   // DisplayLogEntries(output);
                    ReadXMLReply(output);
                    break;
                case 1: //Comment changing
                    if (output.Contains("pre-revprop-change hook"))
                    {
                        MessageBox.Show(string.Format("SVN msg:\n{0}", output + hookScript));
                    }
                    else
                    {
                        rtbStatus.Text = string.Format("SVN msg: {0}", output);
                    }
                    break;
                default:
                    break;
            }

            progressBar1.Visible = false;
            Application.DoEvents();
        }

        private void ReadXMLReply(string output)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);

            if (output.ToLower().Contains("error validating server certificate"))
            {
                MessageBox.Show("Error logging into the SVN server.\nPlease provide a username and password or log into the svn server.\nIf https is used, accept the security certificate permanently.");
            }
            else if (output.ToLower().Contains("error resolving case of") || output.ToLower().Contains("unable to connect to a repository at"))
            {
                rtbStatus.Text = string.Format("SVN msg: {0}", output);
            }
            else //Received data
            {
                writer.Write(output);
                writer.Flush();
                stream.Position = 0;

                xmlDS = new DataSet();
                xmlDS.ReadXml(stream);

                if (xmlDS.Tables.Count > 0)
                {
                    DisplayLogEntries();
                }
            }
        }

        private void DisplayLogEntries()
        {
            dgvLogEntries.DataSource = null;
            dgvLogEntries.Refresh();

            if (xmlDS.Tables.Count > 0)
            {
                dgvLogEntries.AutoGenerateColumns = true;
                dgvLogEntries.DataSource = xmlDS;
                dgvLogEntries.DataMember = "logentry";
            }

        }

        private void DisplayRevisionFiles()
        {
            DataRow[] drows;
            string expression = "";

            int logID = 0;
            int pathNo = 0;
            dgvFiles.Rows.Clear();

            if (xmlDS.Tables.Count > 0)
            {
                expression = "revision LIKE '" + revNumber + "'";
                drows = xmlDS.Tables["logentry"].Select(expression);
                if (drows != null)
                {
                    logID = Convert.ToInt32(drows[0]["logentry_id"]);

                    expression = "logentry_id = '" + logID + "'";
                    drows = xmlDS.Tables["paths"].Select(expression);
                    if (drows != null)
                    {
                        pathNo = Convert.ToInt32(drows[0]["paths_id"]);

                        expression = "paths_id = '" + pathNo + "'";
                        drows = xmlDS.Tables["path"].Select(expression);

                        if (drows != null)
                        {
                            dgvFiles.AutoGenerateColumns = false;
                            foreach (DataRow drow in drows)
                            {
                                object[] values = { drow[1], drow[0], url + drow[5].ToString() };
                                dgvFiles.Rows.Add(values);
                            }
                        }
                    }
                }
            }
        }

        #endregion

    }
}
