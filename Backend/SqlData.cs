/*
 * MOMS2 Backend - SqlData
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
using System.Data;

namespace Quickstarts.Backend
{
    /// <summary>
    /// Child class of SqlClass holding the SQL queries to interact with the database.
    /// </summary>
    public sealed class SqlData : SqlClass
    {
        /// <summary>
        /// Method that executes a dummy SQL statement to check the database connection.
        /// Contains various try .. catch statements to caputure connection related exceptions.
        /// </summary>
        /// <returns>True if connection could be established and if not returns False.</returns>
        public bool checkConnection()
        {
            bool blnReturnValue = false;
            initializeConnectionString();

            using (var connection = GetConnection())
            {
                using (var command = new SqlCommand("SELECT 1", connection))
                {
                    try
                    {
						//Console.WriteLine("checkConnection()\t {0}", connection.ConnectionString);
						Console.WriteLine("SQL-Connected");
                        connection.Open();                        
                        command.ExecuteScalar();
                        blnReturnValue = true; // Declare connection successful
                    } 
                    catch (SqlException ex) // This will catch all SQL exceptions
                    {
                        Console.WriteLine("Exception E101 {0}", ex.Message);
                        
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine("Exception E102");
                        
                    }
                    catch (Exception ex) // This will catch every Exception
                    {
                        Console.WriteLine("Exception E103");
                        
                    }

                    return blnReturnValue;
                }
            }
        }

        public DataTable getIngredients(Guid uniqueID)
		{
            try
            {
                using (var connection = GetConnection())
                {
                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("select * from dbo.defGetIngredienten('"+ uniqueID +"')", connection))
                    {
                        using (DataTable dataTable = new DataTable())
                        {
                            sqlDataAdapter.Fill(dataTable);
                             
                            return dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Generic exception: " + ex.Message);
                return null;
            }
        }

        public DataTable getJobOrders(Guid SegmentId, Guid OrderId)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("exec spGetJobOrder @segment = "+SegmentId+", @orderid = "+OrderId+"", connection))
                    {
                        using (DataTable dataTable = new DataTable())
                        {
                            sqlDataAdapter.Fill(dataTable);

                            return dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Generic exception: " + ex.Message);
                return null;
            }
        }
    }
}