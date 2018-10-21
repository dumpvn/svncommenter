using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using PMCore.Security;

namespace SVNCommenter.Classes
{
    public class ProjectDA
    {
        #region Variables
        public string dbLocation { get; set; }
        private ProjectDS projectDS; 
        #endregion

        public ProjectDA(string db_Location)
        {
            dbLocation = db_Location;
            projectDS = new ProjectDS();
        }

        /// <summary>
        /// Creates a new demo database
        /// </summary>
        /// <returns>Boolean true or false if database is created</returns>
        public bool CreateNewDB()
        {
            try
            {
                projectDS = new ProjectDS();
                string salt = EncryptionUtils.CreateRandomSalt(24, true);

                //Add Test Project
                DataRow dr = projectDS.Tables["Project"].NewRow();
                dr[0] = 1;
                dr["Name"] = "Test Project";
                dr["URL"] = "http:\\testurl.com";
                dr["UserName"] = "";
                dr["Password"] = "";
                dr["Salt"] = "";
                projectDS.Tables["Project"].Rows.Add(dr);

                //Add Demo admin
                dr = projectDS.Tables["Login"].NewRow();
                dr[0] = 1;
                dr["Password"] = EncryptionUtils.Encrypt("password", "SVNCommenter2013", salt); //Default until user changes it
                dr["Salt"] = salt;
                projectDS.Tables["Login"].Rows.Add(dr);

                projectDS.WriteXml(dbLocation);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        #region Project Data Access

        /// <summary>
        /// Load the projects from the database
        /// </summary>
        /// <returns></returns>
        public List<Project> LoadProjects()
        {
            List<Project> projects = new List<Project>();
            Project prj = null;

            try
            {
                projectDS = new ProjectDS();
                projectDS.ReadXml(dbLocation);

                if (projectDS.Tables.Count > 0)
                {
                    foreach (DataRow dr in projectDS.Tables["Project"].Rows)
                    {
                        prj = new Project();
                        prj.Fill(dr);
                        projects.Add(prj);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return projects;
        }

        public bool AddProject(string ProjectName, string URL, string uName, string PWord)
        {
            try
            {
                projectDS = new ProjectDS();
                projectDS.ReadXml(dbLocation);

                if (projectDS.Tables.Count > 0)
                {
                    //Create a Security Salt for the project
                    string salt = EncryptionUtils.CreateRandomSalt(24, true);

                    DataRow drr = projectDS.Tables["Project"].NewRow();
                    drr["Name"] = ProjectName;
                    drr["URL"] = URL;
                    drr["UserName"] = uName;
                    drr["Password"] = EncryptionUtils.Encrypt(PWord, "SVNCommenter2013", salt);
                    drr["Salt"] = salt;

                    projectDS.Tables["Project"].Rows.Add(drr);
                    projectDS.Tables["Project"].AcceptChanges();

                    projectDS.WriteXml(dbLocation);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public bool EditProject(string oldProjectName, string newProjectName, string URL, string uName, string PWord)
        {
            try
            {
                projectDS = new ProjectDS();
                projectDS.ReadXml(dbLocation);

                if (projectDS.Tables.Count > 0)
                {
                    //Create a new Security Salt for the project
                    string salt = EncryptionUtils.CreateRandomSalt(24, true);

                    DataRow drr = projectDS.Tables["Project"].Select("Name='" + oldProjectName + "' ").First();
                    drr["Name"] = newProjectName;
                    drr["URL"] = URL;
                    drr["UserName"] = uName;
                    drr["Password"] = EncryptionUtils.Encrypt(PWord, "SVNCommenter2013", salt);
                    drr["Salt"] = salt;

                    projectDS.Tables["Project"].AcceptChanges();

                    projectDS.WriteXml(dbLocation);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public bool DeleteProjectFromDB(string ProjectName)
        {
            try
            {
                projectDS = new ProjectDS();
                projectDS.ReadXml(dbLocation);

                if (projectDS.Tables.Count > 0)
                {
                    DataRow[] drr = projectDS.Tables["Project"].Select("Name='" + ProjectName + "' ");
                    for (int i = 0; i < drr.Length; i++)
                    {
                        drr[i].Delete();
                    }

                    projectDS.Tables["Project"].AcceptChanges();

                    projectDS.WriteXml(dbLocation);
                    return true;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        #endregion

        #region Administration Data Access

        public Login LoadLogin()
        {
            Login lgn = null;
            try
            {
                projectDS = new ProjectDS();
                projectDS.ReadXml(dbLocation);

                if (projectDS.Tables.Count > 0)
                {
                    lgn = new Login();
                    lgn.Fill(projectDS.Tables["Login"].Rows[0]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return lgn;
        }

        public bool SaveAdminDetails(string pWord)
        {
            try
            {
                bool exists = false;
                projectDS = new ProjectDS();
                projectDS.ReadXml(dbLocation);

                if (projectDS.Tables.Count > 0)
                {
                    //Create a Security Salt for the project
                    string salt = EncryptionUtils.CreateRandomSalt(24, true);

                    DataRow drr = null;

                    try //Existing Record
                    {
                        drr = projectDS.Tables["Login"].Rows[0];
                        exists = true;
                    }
                    catch (Exception ex1)
                    {
                        //New Record
                        projectDS.Tables["Login"].NewRow();
                    }
                    
                    drr["Password"] = EncryptionUtils.Encrypt(pWord, "SVNCommenter2013", salt); //Uses default PW (SVNCommenter2013) to encrypt
                    drr["Salt"] = salt;

                    if (!exists)
                    {
                        projectDS.Tables["Login"].Rows.Add(drr);
                    }
                    projectDS.Tables["Login"].AcceptChanges();

                    projectDS.WriteXml(dbLocation);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return false;
        }

        public bool CheckLogin(string pwToCompare)
        {

            try
            {
                Login lgn = LoadLogin();

                if (lgn != null)
                {
                    string dbPWord = lgn.Password;
                    string salt = lgn.Salt;

                    dbPWord = EncryptionUtils.Decrypt(dbPWord, "SVNCommenter2013", salt);

                    if (dbPWord == pwToCompare)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           
            return false;
        }

        #endregion

    }
}
