using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVNCommenter.Classes
{
    public class Login
    {
        #region Properties
        public int ID { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; } 
        #endregion

        /// <summary>
        /// Empty Constructor. Use .Fill to initialize
        /// </summary>
        public Login()
        {
            
        }

        #region Public Methods
        /// <summary>
        /// Fill the Login object from a DataReader
        /// </summary>
        /// <param name="dr">DataReader containing the objects info</param>
        public void Fill(System.Data.SqlClient.SqlDataReader dr)
        {
            System.ComponentModel.PropertyDescriptorCollection props = System.ComponentModel.TypeDescriptor.GetProperties(this);
            for (int i = 0; (i < props.Count); i = (i + 1))
            {
                System.ComponentModel.PropertyDescriptor prop = props[i];
                if ((prop.IsReadOnly != true))
                {
                    try
                    {
                        if ((dr[prop.Name].Equals(System.DBNull.Value) != true))
                        {
                            if ((prop.PropertyType.Equals(dr[prop.Name].GetType()) != true))
                            {
                                prop.SetValue(this, prop.Converter.ConvertFrom(dr[prop.Name]));
                            }
                            else
                            {
                                prop.SetValue(this, dr[prop.Name]);
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Fill the Login object from a DataRow
        /// </summary>
        /// <param name="dr">DataRow containing the objects info</param>
        public void Fill(System.Data.DataRow dr)
        {
            System.ComponentModel.PropertyDescriptorCollection props = System.ComponentModel.TypeDescriptor.GetProperties(this);
            for (int i = 0; (i < props.Count); i = (i + 1))
            {
                System.ComponentModel.PropertyDescriptor prop = props[i];
                if ((prop.IsReadOnly != true))
                {
                    try
                    {
                        if ((dr[prop.Name].Equals(System.DBNull.Value) != true))
                        {
                            if ((prop.PropertyType.Equals(dr[prop.Name].GetType()) != true))
                            {
                                prop.SetValue(this, prop.Converter.ConvertFrom(dr[prop.Name]));
                            }
                            else
                            {
                                prop.SetValue(this, dr[prop.Name]);
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                    }
                }
            }
        }

        #endregion
    }
}
