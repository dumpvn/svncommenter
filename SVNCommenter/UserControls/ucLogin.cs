using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SVNCommenter.Classes;

namespace SVNCommenter
{
    public partial class ucLogin : UserControl
    {
        #region Variables
        public bool LoginState { get; set; }
        ProjectDA pDA = null; 
        #endregion

        public ucLogin(string dbLocation)
        {
            InitializeComponent();

            pDA = new ProjectDA(dbLocation);
        }

        #region Form Events
        private void btnLogin_Click(object sender, EventArgs e)
        {
            LoginState = pDA.CheckLogin(txtPassword.Text);
            this.Parent.Dispose();
        }

        private void txtPassword_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin_Click(this, null);
            }
        } 
        #endregion
    }
}
