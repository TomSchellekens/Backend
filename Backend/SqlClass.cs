/*
 * MOMS2 Frontend - SqlClass
 * 
 * Avans Hogeschool 's-Hertogenbosch - (c)2021
 * MOMS3 - leerjaar 3 - BLOK 12
 * 
 * Manufacturing execution system (MES) voor het vak MOMS2. Bedrijfproject met Actemium voor 'Broodbakkerij Zoete Broodjes Corp.'.
 * 
 * Door:				Studentnummer:
 * Ruben Gepkens		2137822
 * Tom Schellekens		2135695
 * Wes Verhagen			2135682
 * Maurits Duel			2142917
 * Leon van Elteren		2136163
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient; // Zelf toegevoegd

namespace FrontEnd
{
    /// <summary>
    /// Class for storing the SQL connection and connection related information.
    /// </summary>
    public class SqlClass
    {
        private string strConnectionString;
        public bool blnConnectionStatus { get; set; }

        public List<string> lstRecipes = new List<string> { }; // TODO
        public List<string> lstCustomers = new List<string> { }; // TODO
        public List<string> lstProductionlines = new List<string> { };
        public List<string> lstOrderstatusses = new List<string> { };

        /// <summary>
        /// Initialize SqlClass
        /// </summary>
        public SqlClass()
        {            
            blnConnectionStatus = false;
            initializeConnectionString();
        }

        /// <summary>
        /// Initialize the connectionstring.
        /// </summary>
        protected void initializeConnectionString()
        {
            Console.WriteLine("SqlClass : makeConnectionString(): " + strConnectionString);
            strConnectionString = Properties.Settings.Default.connectionString;            
        }

        /// <summary>
        /// Function for retrieving the SqlConnection containing the connection string information.
        /// </summary>
        /// <returns>SqlConnection</returns>
        protected SqlConnection GetConnection()
        {
            Console.WriteLine("sqlVerbinding: " + strConnectionString);
            return new SqlConnection(strConnectionString);
        }
    }
}