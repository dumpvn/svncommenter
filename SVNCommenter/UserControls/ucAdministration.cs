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
    public partial class ucAdministration : UserControl
    {
        #region Properties

        private string adminPassword;
        public string AdminPassword
        {
            get { return adminPassword; }
            set
            {
                adminPassword = value;
                txtPassword.Text = adminPassword;
            }
        }

        #endregion

        ProjectDA pDA;

        public ucAdministration(Login adminEntry, string dbLocation)
        {
            pDA = new ProjectDA(dbLocation);

            InitializeComponent();

            if (adminEntry != null)
            {
                AdminPassword = adminEntry.Password;
                string salt = adminEntry.Salt;
                
                try
                {
                    if (AdminPassword != string.Empty || salt != string.Empty)
                    {
                        AdminPassword = EncryptionUtils.Decrypt(AdminPassword, "SVNCommenter2013", salt);
                    }
                    else
                    {
                        AdminPassword = "";
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
        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = (chkShowPassword.Checked) ? '*' : '\0';
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtPassword.Text == string.Empty)
            {
                MessageBox.Show("Please specify a passwords!");
            }
            else
            {
                pDA.SaveAdminDetails(txtPassword.Text);
            }
            this.Parent.Dispose();
        } 
        #endregion
    }
}
