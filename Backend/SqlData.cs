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

        public DataTable getIngredients(Guid joborderID)
		{
            try
            {
                using (var connection = GetConnection())
                {
                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("exec spGetIngredients @orderID = '"+joborderID+"'", connection))
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

        public DataTable getJobOrders(Guid SegmentId, Guid RequestId)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("exec spGetJobOrder @segment = '" + SegmentId + "', @requestid = '" + RequestId + "'", connection))
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

        public void fillPerformanceTables(Guid RequestId)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("exec spOnStartingOrder @RequestId = '" + RequestId + "'", connection))
                    {
                        using (DataTable dataTable = new DataTable())
                        {
                            sqlDataAdapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Generic exception: " + ex.Message);
            }
        }

        public void fillMaterialActualTabel(float Quantity, Guid JobOrderId, string MaterialDefinitionId)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("exec spInsertMaterialActual @Quantity = '" + Quantity + "', @JobOrderId = '" + JobOrderId + "', @MaterialDefinitionId = '" + MaterialDefinitionId + "'", connection))
                    {
                        using (DataTable dataTable = new DataTable())
                        {
                            sqlDataAdapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Generic exception: " + ex.Message);
            }
        }

        public void SetEndTime(Guid JobOrderId)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("exec spSetEndTime @JobOrderId = '" + JobOrderId +"'", connection))
                    {
                        using (DataTable dataTable = new DataTable())
                        {
                            sqlDataAdapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Generic exception: " + ex.Message);
            }
        }

        public void SetStartTime(Guid JobOrderId, bool isFirstSegment)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("exec spSetStartTimeJob @JobOrderId = '" + JobOrderId + "', @isFirstSegment = '" + isFirstSegment + "'", connection))
                    {
                        using (DataTable dataTable = new DataTable())
                        {
                            sqlDataAdapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Generic exception: " + ex.Message);
            }
        }
        

        public void insertCustomMaterial(Guid JobOrderId, string description, Guid processsegmentid, string quan, string eenheid)
        {
            
            try
            {
                using (var connection = GetConnection())
                {
                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("exec spInsertCustomMaterial @joborderid = '"+JobOrderId+"', @description = '"+description+"', @processsegmentdbid = '"+processsegmentid+ "', @quanity = '"+quan+"', @eenheid = '"+eenheid+"'", connection))
                    {
                        using (DataTable dataTable = new DataTable())
                        {
                            sqlDataAdapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Generic exception: " + ex.Message);
            }
        }
        

        public void insertCustomEquip(Guid JobOrderId, string description, string quan, string eenheid)
        {

            try
            {
                using (var connection = GetConnection())
                {
                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("exec spInsertCustomEquipment @joborderid = '" + JobOrderId + "', @description = '" + description + "', @quanity = '" + quan + "', @eenheid = '" + eenheid + "'", connection))
                    {
                        using (DataTable dataTable = new DataTable())
                        {
                            sqlDataAdapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Generic exception: " + ex.Message);
            }
        }

        public void endRequestOrder(Guid requestid)
        {

            try
            {
                using (var connection = GetConnection())
                {
                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("exec spCompleteRequest @requestid = '"+requestid+"'", connection))
                    {
                        using (DataTable dataTable = new DataTable())
                        {
                            sqlDataAdapter.Fill(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Generic exception: " + ex.Message);
            }
        }
    }
}