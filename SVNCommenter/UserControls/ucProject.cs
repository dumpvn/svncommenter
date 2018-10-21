using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using PMCore.Security;
using SVNCommenter.Classes;

namespace SVNCommenter
{
    public partial class ucProject : UserControl
    {
        #region Properties

        private string projName;
        public string ProjName
        {
            get { return projName; }
            set
            {
                projName = value;
                txtName.Text = projName;
            }
        }

        private string projURL;
        public string ProjURL
        {
            get { return projURL; }
            set
            {
                projURL = value;
                txtURL.Text = projURL;
            }
        }

        private string projUserName;
        public string ProjUserName
        {
            get { return projUserName; }
            set
            {
                projUserName = value;
                txtUsername.Text = projUserName;
            }
        }

        private string projPassword;
        public string ProjPassword
        {
            get { return projPassword; }
            set
            {
                projPassword = value;
                txtPassword.Text = projPassword;
            }
        } 

        #endregion

        ProjectDA pDA;
        private bool EditMode = false;

        public ucProject(Project projectEntry,string dbLocation)
        {
            pDA = new ProjectDA(dbLocation);
            InitializeComponent();

            if (projectEntry != null)
            {
                EditMode = true;
                ProjName = projectEntry.Name;
                ProjURL = projectEntry.URL;
                ProjUserName = projectEntry.UserName;
                try
                {
                    string PWord = projectEntry.Password;
                    string salt = projectEntry.Salt;

                    if (PWord != string.Empty || salt != string.Empty)
                    {
                        ProjPassword = EncryptionUtils.Decrypt(PWord, "SVNCommenter2013", salt);
                        if (projPassword == string.Empty)
                        {
                            MessageBox.Show("Please check the Password as it could not be decrypted.");
                        }
                    }
                    else
                    {
                        ProjPassword = "";
                    }
                }
                catch (Exception ex) //Probably an incorrect password / salt combination
                {
                    Console.WriteLine(ex.Message);
                    MessageBox.Show("Please check the Password as it could not be decrypted.");
                }
                
            }
        }

        #region Form Events
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (EditMode)
            {
                pDA.EditProject(ProjName, txtName.Text, txtURL.Text, txtUsername.Text, txtPassword.Text);
            }
            else
            {
                pDA.AddProject(txtName.Text, txtURL.Text, txtUsername.Text, txtPassword.Text);
            }
            this.Parent.Dispose();
        }

        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = (chkShowPassword.Checked) ? '*' : '\0';
        } 
        #endregion
    }
}
