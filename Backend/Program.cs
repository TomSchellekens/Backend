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
using System.Threading;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Quickstarts.Backend
{
    
    public static class Program
    {
        static Guid orderid;
        static List<Guid> JobOrdersDeeg1 = new List<Guid>();
        static List<Guid> JobOrdersBakken1 = new List<Guid>();
        static List<Guid> JobOrdersVerpakken1 = new List<Guid>();
        static List<Guid> JobOrdersDeeg2 = new List<Guid>();
        static List<Guid> JobOrdersBakken2 = new List<Guid>();
        static List<Guid> JobOrdersVerpakken2 = new List<Guid>();
        static int counterJobOrderDeeg1, counterJobOrderBakken1, counterJobOrderVerpakken1, counterJobOrderDeeg2, counterJobOrderBakken2, counterJobOrderVerpakken2;


        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Trying to connect to OPC...");

                    // Define the UA Client application
                    ApplicationInstance application = new ApplicationInstance();
                    application.ApplicationName = "Quickstart Console Backend";
                    application.ApplicationType = ApplicationType.Client;

                    // load the application configuration.
                    application.LoadApplicationConfiguration("Quickstarts.Backend.Config.xml", silent: false);

                    // check the application certificate.
                    application.CheckApplicationInstanceCertificate(silent: false, minimumKeySize: 0);

                    EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(application.ApplicationConfiguration);

                    //var endpointDescription = CoreClientUtils.SelectEndpoint("opc.tcp://192.168.8.145:4840", false);
                    //var endpointDescription = CoreClientUtils.SelectEndpoint("opc.tcp://192.168.1.145:4840", false);
                    var endpointDescription = CoreClientUtils.SelectEndpoint("opc.tcp://192.168.0.1:4840", false);

                    ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                    application.ApplicationConfiguration.CertificateValidator.CertificateValidation += CertificateValidation;

                    makeSession(application, endpoint);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }


        private static void makeSession(ApplicationInstance application, ConfiguredEndpoint endpoint)
        {
            //start session to the OPC server
            using (var session = Session.Create(application.ApplicationConfiguration, endpoint, false, false, application.ApplicationName, 30 * 60 * 1000, new UserIdentity(), null).GetAwaiter().GetResult())
            {
                Console.WriteLine("Connected.");
                var subscription = new Subscription(session.DefaultSubscription) { PublishingInterval = 500 };

                //Starten Order
                var startOrder = new MonitoredItem(subscription.DefaultItem) { DisplayName = "StartenOrder", StartNodeId = @"ns=3;s=""db_OPCdata"".""startenOrder""" };
                startOrder.Notification += (sender, e) => OnStartingOrder(sender, e, session);
                subscription.AddItem(startOrder);

                //End order
                var endOrder = new MonitoredItem(subscription.DefaultItem) { DisplayName = "EndOrder", StartNodeId = @"ns=3;s=""db_OPCdata"".""endOrder""" };
                endOrder.Notification += (sender, e) => OnEndingOrder(sender, e, session);
                subscription.AddItem(endOrder);

                //subscriptions on packml status               
                //PackML status deegverwerking lijn 1
                var deegvwkLijn1 = new MonitoredItem(subscription.DefaultItem) { DisplayName = "StateDeegL1", StartNodeId = @"ns=3;s=""db_OPCdata"".""lijn1"".""PackMl_Deegverwerking"".""O_i_State""" };
                deegvwkLijn1.Notification += (sender, e) => OnStateDeegLijn1(sender, e, session);
                subscription.AddItem(deegvwkLijn1);

                //PackML status bakken lijn 1 
                var bakkenLijn1 = new MonitoredItem(subscription.DefaultItem) { DisplayName = "StateBakkenL1", StartNodeId = @"ns=3;s=""db_OPCdata"".""lijn1"".""PackML_Bakken"".""O_i_State""" };
                bakkenLijn1.Notification += (sender, e) => OnStateBakLijn1(sender, e, session);
                subscription.AddItem(bakkenLijn1);

                //PackML status verpakken lijn 1 
                var verpakkenLijn1 = new MonitoredItem(subscription.DefaultItem) { DisplayName = "StateVerpakkenL1", StartNodeId = @"ns=3;s=""db_OPCdata"".""lijn1"".""PackML_Verpakken"".""O_i_State""" };
                verpakkenLijn1.Notification += (sender, e) => OnStateVerpakkenLijn1(sender, e, session);
                subscription.AddItem(verpakkenLijn1);

                //PackML status deegverwerking lijn 2
                var deegvwkLijn2 = new MonitoredItem(subscription.DefaultItem) { DisplayName = "StateDeegL2", StartNodeId = @"ns=3;s=""db_OPCdata"".""lijn2"".""PackMl_Deegverwerking"".""O_i_State""" };
                deegvwkLijn2.Notification += (sender, e) => OnStateDeegLijn2(sender, e, session);
                subscription.AddItem(deegvwkLijn2);

                //PackML status bakken lijn 2
                var bakkenLijn2 = new MonitoredItem(subscription.DefaultItem) { DisplayName = "StateBakkenL2", StartNodeId = @"ns=3;s=""db_OPCdata"".""lijn2"".""PackML_Bakken"".""O_i_State""" };
                bakkenLijn2.Notification += (sender, e) => OnStateBakLijn2(sender, e, session);
                subscription.AddItem(bakkenLijn2);

                //PackML status verpakken lijn 2
                var verpakkenLijn2 = new MonitoredItem(subscription.DefaultItem) { DisplayName = "StateVerpakkenL2", StartNodeId = @"ns=3;s=""db_OPCdata"".""lijn2"".""PackML_Verpakken"".""O_i_State""" };
                verpakkenLijn2.Notification += (sender, e) => OnStateVerpakkenLijn2(sender, e, session);
                subscription.AddItem(verpakkenLijn2);

                //Start jobs lijn 1
                var startjobslijn1 = new MonitoredItem(subscription.DefaultItem) { DisplayName = "StartJobsLijn1", StartNodeId = @"ns=3;s=""db_OPCdata"".""startJobsLijn1""" };
                startjobslijn1.Notification += (sender, e) => OnStartJobsLijn1(sender, e, session);
                subscription.AddItem(startjobslijn1);

                //Start jobs lijn 2
                var startjobslijn2 = new MonitoredItem(subscription.DefaultItem) { DisplayName = "StartJobsLijn2", StartNodeId = @"ns=3;s=""db_OPCdata"".""startJobsLijn2""" };
                startjobslijn2.Notification += (sender, e) => OnStartJobsLijn2(sender, e, session);
                subscription.AddItem(startjobslijn2);

                //Add subscription to de session
                session.AddSubscription(subscription);
                subscription.Create();
                session.PublishError += (sender, e) => { Console.WriteLine("Error detected."); };
				

                while (!session.KeepAliveStopped)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private static void CertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
        {
            bool certificateAccepted = true;

            // ****
            // Implement a custom logic to decide if the certificate should be
            // accepted or not and set certificateAccepted flag accordingly.
            // The certificate can be retrieved from the e.Certificate field
            // ***

            ServiceResult error = e.Error;
            while (error != null)
            {
                Console.WriteLine(error);
                error = error.InnerResult;
            }

            if (certificateAccepted)
            {
                Console.WriteLine("Untrusted Certificate accepted. SubjectName = {0}", e.Certificate.SubjectName);
            }

            e.AcceptAll = certificateAccepted;
        }

        private static void OnStartingOrder(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);
                if ((bool)value.Value == true)
                {

                    //Read opc variable from frontend
                    orderid = Guid.Parse(session.ReadValue(@"ns=3;s=""db_OPCdata"".""orderDbId""").ToString());
                        
                    //DBID zijn nog van lijn 2 is om te testen
                    List<Guid> dbids = new List<Guid>();
                    dbids.Add(Guid.Parse("39D84F43-01C8-4753-A210-FB58392D2059")); //Deegverwerking 1 
                    dbids.Add(Guid.Parse("7ECE938B-5D65-4643-9F3D-60D3DD42AD3F")); //Bakken 1 
                    dbids.Add(Guid.Parse("EB06DF1A-C6B2-4727-A11D-1D0EDD10FBFD")); //Verpakken 1 
                    dbids.Add(Guid.Parse("9D0541D4-F18F-482F-99BF-C9B66B32559A")); //Deegverwerking 2
                    dbids.Add(Guid.Parse("56B71358-4F47-4A27-A4A9-2CABFEBCF366")); //Bakken 2 
                    dbids.Add(Guid.Parse("17D4B634-3EFA-4F7D-94C8-7842F1F1AC8F")); //Verpakken 2

                    //Hier JobOrders ophalen voor specifiek order nummer en opslaan
                    SqlData sqlData1 = new SqlData();
                    sqlData1.checkConnection();
                    sqlData1.fillPerformanceTables(orderid);
                    for (int i = 0; i < 6; i++)
                    {
                        Console.WriteLine(dbids[i]);
                        DataTable data1 = sqlData1.getJobOrders(dbids[i], orderid);
                        switch (i)
                        {
                            case 0:
                                for (int j = 0; j < data1.Rows.Count; j++)
                                {
                                    JobOrdersDeeg1.Add(Guid.Parse(data1.Rows[j][0].ToString()));
                                }
                                break;
                            case 1:
                                for (int j = 0; j < data1.Rows.Count; j++)
                                {
                                    JobOrdersBakken1.Add(Guid.Parse(data1.Rows[j][0].ToString()));
                                }
                                break;
                            case 2:
                                for (int j = 0; j < data1.Rows.Count; j++)
                                {
                                    JobOrdersVerpakken1.Add(Guid.Parse(data1.Rows[j][0].ToString()));
                                }
                                break;
                            case 3:
                                for (int j = 0; j < data1.Rows.Count; j++)
                                {
                                    JobOrdersDeeg2.Add(Guid.Parse(data1.Rows[j][0].ToString()));
                                }
                                break;
                            case 4:
                                for (int j = 0; j < data1.Rows.Count; j++)
                                {
                                    JobOrdersBakken2.Add(Guid.Parse(data1.Rows[j][0].ToString()));
                                }
                                break;
                            case 5:
                                for (int j = 0; j < data1.Rows.Count; j++)
                                {
                                    JobOrdersVerpakken2.Add(Guid.Parse(data1.Rows[j][0].ToString()));
                                }
                                break;
                            default:
                                break;
                        }
                    }

                    counterJobOrderDeeg1 = JobOrdersDeeg1.Count;
                    counterJobOrderBakken1 = JobOrdersBakken1.Count;
                    counterJobOrderVerpakken1 = JobOrdersVerpakken1.Count;
                    counterJobOrderDeeg2 = JobOrdersDeeg2.Count;
                    counterJobOrderBakken2 = JobOrdersBakken2.Count;
                    counterJobOrderVerpakken2 = JobOrdersVerpakken2.Count;

                    object[] values = {false, false };
                    IList<NodeId> nodeIds = new List<NodeId>();
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""startJobsLijn1"""));
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""startJobsLijn2"""));

                    if (counterJobOrderDeeg1 > 0 && counterJobOrderBakken1 > 0 && counterJobOrderVerpakken1 > 0) 
                    {
                        values[0] = true;
                    }
					else
					{
                        values[0] = false;
 					}

					if (counterJobOrderDeeg2 > 0 && counterJobOrderBakken2 > 0 && counterJobOrderVerpakken2 > 0)
					{
                        values[1] = true;
                    }
					else
					{
                        values[1] = false;
                    }

                    WriteValueCollection nodesToWrite = new WriteValueCollection();

                    for (int i = 0; i < nodeIds.Count; i++)
                    {
                        WriteValue bWriteValue = new WriteValue();
                        bWriteValue.NodeId = nodeIds[i];
                        bWriteValue.AttributeId = Attributes.Value;
                        bWriteValue.Value = new DataValue();
                        bWriteValue.Value.Value = values[i];
                        nodesToWrite.Add(bWriteValue);
                    }

                    // Write the node attributes
                    StatusCodeCollection results = null;
                    DiagnosticInfoCollection diagnosticInfos;

                    // Call Write Service
                    session.Write(null,
                                    nodesToWrite,
                                    out results,
                                    out diagnosticInfos);

                    foreach (StatusCode writeResult in results)
                    {
                        Console.WriteLine("     {0}", writeResult);
                    }
                }
            }
        }
        
        private static void OnStartJobsLijn1(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
		{
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);
                if ((bool)value.Value == true)
                {
                    Console.WriteLine(value.Value.ToString());
                    //Ingedrienten
                    float bloem = 0, boter = 0, gist = 0, meel = 0, suiker = 0, water = 0, zout = 0;

                    //SQL data 
                    SqlData sqlData = new SqlData();
                    sqlData.checkConnection();
                    DataTable data = sqlData.getIngredients(JobOrdersDeeg1[0]);

                    //row then colum
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        string Ingredient = data.Rows[i][0].ToString();

                        switch (Ingredient)
                        {
                            case "Bloem":
                                bloem = float.Parse(data.Rows[i][1].ToString());
                                Console.WriteLine("Bloem = {0}", bloem);
                                break;
                            case "Gist":
                                gist = float.Parse(data.Rows[i][1].ToString());
                                Console.WriteLine("Gist = {0}", gist);
                                break;
                            case "Meel":
                                meel = float.Parse(data.Rows[i][1].ToString());
                                Console.WriteLine("Meel = {0}", meel);
                                break;
                            case "Water":
                                water = float.Parse(data.Rows[i][1].ToString());
                                Console.WriteLine("Water = {0}", water);
                                break;
                            case "Suiker":
                                suiker = float.Parse(data.Rows[i][1].ToString());
                                Console.WriteLine("Suiker = {0}", suiker);
                                break;
                            case "Zout":
                                zout = float.Parse(data.Rows[i][1].ToString());
                                Console.WriteLine("Zout = {0}", zout);
                                break;
                            case "Boter":
                                boter = float.Parse(data.Rows[i][1].ToString());
                                Console.WriteLine("Boter = {0}", boter);
                                break;
                            default:
                                break;
                        }
                    }

                    IList<NodeId> nodeIds = new List<NodeId>();
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""PackMl_Deegverwerking"".""I_b_Cmd_Start"""));
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""Recept"".""S_r_bloem"""));
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""Recept"".""S_r_boter"""));
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""Recept"".""S_r_gist"""));
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""Recept"".""S_r_meel"""));
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""Recept"".""S_r_suiker"""));
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""Recept"".""S_r_water"""));
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""Recept"".""S_r_zout"""));


                    object[] values = { true, bloem, boter, gist, meel, suiker, water, zout };

                    WriteValueCollection nodesToWrite = new WriteValueCollection();

                    for (int i = 0; i < nodeIds.Count; i++)
                    {
                        WriteValue bWriteValue = new WriteValue();
                        bWriteValue.NodeId = nodeIds[i];
                        bWriteValue.AttributeId = Attributes.Value;
                        bWriteValue.Value = new DataValue();
                        bWriteValue.Value.Value = values[i];
                        nodesToWrite.Add(bWriteValue);
                    }

                    // Write the node attributes
                    StatusCodeCollection results = null;
                    DiagnosticInfoCollection diagnosticInfos;

                    // Call Write Service
                    session.Write(null,
                                    nodesToWrite,
                                    out results,
                                    out diagnosticInfos);

                    foreach (StatusCode writeResult in results)
                    {
                        Console.WriteLine("     {0}", writeResult);
                    }

                }
            }
        }

        private static void OnStartJobsLijn2(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);
                if ((bool)value.Value == true)
                {
                    Console.WriteLine(value.Value.ToString());
                    //Ingedrienten
                    float bloem = 0, boter = 0, gist = 0, meel = 0, suiker = 0, water = 0, zout = 0;

                    //SQL data 
                    SqlData sqlData = new SqlData();
                    sqlData.checkConnection();
                    DataTable data = sqlData.getIngredients(JobOrdersDeeg2[0]);

                    //row then colum
                    for (int i = 0; i < data.Rows.Count; i++)
                    {
                        string Ingredient = data.Rows[i][0].ToString();

                        switch (Ingredient)
                        {
                            case "Bloem":
                                bloem = float.Parse(data.Rows[i][1].ToString());
                                Console.WriteLine("Bloem = {0}", bloem);
                                break;
                            case "Gist":
                                gist = float.Parse(data.Rows[i][1].ToString());
                                Console.WriteLine("Gist = {0}", gist);
                                break;
                            case "Meel":
                                meel = float.Parse(data.Rows[i][1].ToString());
                                Console.WriteLine("Meel = {0}", meel);
                                break;
                            case "Water":
                                water = float.Parse(data.Rows[i][1].ToString());
                                Console.WriteLine("Water = {0}", water);
                                break;
                            case "Suiker":
                                suiker = float.Parse(data.Rows[i][1].ToString());
                                Console.WriteLine("Suiker = {0}", suiker);
                                break;
                            case "Zout":
                                zout = float.Parse(data.Rows[i][1].ToString());
                                Console.WriteLine("Zout = {0}", zout);
                                break;
                            case "Boter":
                                boter = float.Parse(data.Rows[i][1].ToString());
                                Console.WriteLine("Boter = {0}", boter);
                                break;
                            default:
                                break;
                        }
                    }

                    IList<NodeId> nodeIds = new List<NodeId>();//
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""PackMl_Deegverwerking"".""I_b_Cmd_Start"""));
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""Recept"".""S_r_bloem"""));
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""Recept"".""S_r_boter"""));
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""Recept"".""S_r_gist"""));
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""Recept"".""S_r_meel"""));
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""Recept"".""S_r_suiker"""));
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""Recept"".""S_r_water"""));
                    nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""Recept"".""S_r_zout"""));


                    object[] values = { true, bloem, boter, gist, meel, suiker, water, zout };

                    WriteValueCollection nodesToWrite = new WriteValueCollection();

                    for (int i = 0; i < nodeIds.Count; i++)
                    {
                        WriteValue bWriteValue = new WriteValue();
                        bWriteValue.NodeId = nodeIds[i];
                        bWriteValue.AttributeId = Attributes.Value;
                        bWriteValue.Value = new DataValue();
                        bWriteValue.Value.Value = values[i];
                        nodesToWrite.Add(bWriteValue);
                    }

                    // Write the node attributes
                    StatusCodeCollection results = null;
                    DiagnosticInfoCollection diagnosticInfos;

                    // Call Write Service
                    session.Write(null,
                                    nodesToWrite,
                                    out results,
                                    out diagnosticInfos);

                    foreach (StatusCode writeResult in results)
                    {
                        Console.WriteLine("     {0}", writeResult);
                    }

                }
            }
        }

        private static void OnEndingOrder(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);
                if ((bool)value.Value == true)
                {
                    Console.WriteLine(value.Value.ToString());

                }
            }
        }

        private static void OnStateDeegLijn1(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);
                int state = Convert.ToInt32(value.Value);

                switch (state)
                {
                    case 10:
                        Console.WriteLine("Idle");
                        break;
                    case 30:
                        Console.WriteLine("Execute");
                        break;
                    case 50:
                        Console.WriteLine("Complete");
                        string[] names = { "Mengtijd", "Part", "Bloem", "Boter", "Gist", "Meel", "Suiker", "Water","Zout"};
                        //Read nodes
                        IList<Type> types = new List<Type>();
                        IList<NodeId> nodeIdsRead = new List<NodeId>();
                        List<object> readValues;
                        List<ServiceResult> readResult;
                        List<string> name = new List<string>();

						foreach (var n in names)
						{
                            name.Add(n);
						}

                        types.Add(typeof(Int32));
                        types.Add(typeof(Int16));
                        types.Add(typeof(float));
                        types.Add(typeof(float));
                        types.Add(typeof(float));
                        types.Add(typeof(float));
                        types.Add(typeof(float));
                        types.Add(typeof(float));
                        types.Add(typeof(float));

                        //var mengtijd = session.ReadValue(@"ns=3;s=""db_OPCdata"".""lijn1"".""S_di_WerkelijkMengtijd""");

                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""S_di_WerkelijkMengtijd"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""S_i_AmountParts"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""ActueleWaarde"".""S_r_bloem"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""ActueleWaarde"".""S_r_boter"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""ActueleWaarde"".""S_r_gist"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""ActueleWaarde"".""S_r_meel"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""ActueleWaarde"".""S_r_suiker"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""ActueleWaarde"".""S_r_water"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""ActueleWaarde"".""S_r_zout"""));

                        session.ReadValues(nodeIdsRead, types, out readValues, out readResult);
                        int intVars;
                        float fVars;

                        SqlData sqlData1 = new SqlData();
                        sqlData1.checkConnection();


                        for (int i = 0; i < readValues.Count; i++)
						{
							switch (names[i])
							{
                                case "Mengtijd":                                    
                                    intVars = Int32.Parse(readValues[i].ToString());
                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    break;
                                case "Part":                         
                                    intVars = Int16.Parse(readValues[i].ToString());                                   
                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    sqlData1.fillMaterialActualTabel((float)intVars, JobOrdersDeeg1[0], names[i]);
                                    sqlData1.fillMaterialActualTabel((float)intVars, JobOrdersBakken1[0], names[i]);
                                    break;
                                case "Bloem":          
                                    fVars = float.Parse(readValues[i].ToString());
                                    
                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    sqlData1.fillMaterialActualTabel(fVars, JobOrdersDeeg1[0], names[i]);
                                    break;
                                case "Boter":
                                    fVars = float.Parse(readValues[i].ToString());
                                    
                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    sqlData1.fillMaterialActualTabel(fVars, JobOrdersDeeg1[0], names[i]);
                                    break;
                                case "Gist":
                                    fVars = float.Parse(readValues[i].ToString());
                                    
                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    sqlData1.fillMaterialActualTabel(fVars, JobOrdersDeeg1[0], names[i]);
                                    break;
                                case "Meel":
                                    fVars = float.Parse(readValues[i].ToString());
                                    
                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    sqlData1.fillMaterialActualTabel(fVars, JobOrdersDeeg1[0], names[i]);
                                    break;
                                case "Suiker":
                                    fVars = float.Parse(readValues[i].ToString());
                                    
                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    sqlData1.fillMaterialActualTabel(fVars, JobOrdersDeeg1[0], names[i]);
                                    break;
                                case "Water":
                                    fVars = float.Parse(readValues[i].ToString());
                                    
                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    sqlData1.fillMaterialActualTabel(fVars, JobOrdersDeeg1[0], names[i]);
                                    break;
                                case "Zout":
                                    fVars = float.Parse(readValues[i].ToString());
                                    
                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    sqlData1.fillMaterialActualTabel(fVars, JobOrdersDeeg1[0], names[i]);
                                    break;
                                default:
									break;
							}
						}


                        //Write nodes
                        IList<NodeId> nodeIds_State_50 = new List<NodeId>();
                        nodeIds_State_50.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""PackML_Bakken"".""I_b_Cmd_Start"""));
                        nodeIds_State_50.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""PackMl_Deegverwerking"".""I_b_Cmd_Start"""));
                        nodeIds_State_50.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""startenOrder"""));
                        nodeIds_State_50.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""startJobsLijn1"""));

                        object[] values_State_50 = { true, false, false, false };

                        WriteValueCollection nodesToWrite_State_50 = new WriteValueCollection();

						for (int i = 0; i < nodeIds_State_50.Count; i++)
						{
                            WriteValue bWriteValue = new WriteValue();
                            bWriteValue.NodeId = nodeIds_State_50[i];
                            bWriteValue.AttributeId = Attributes.Value;
                            bWriteValue.Value = new DataValue();
                            bWriteValue.Value.Value = values_State_50[i];
                            nodesToWrite_State_50.Add(bWriteValue);
                        }
                      
                        // Write the node attributes
                        StatusCodeCollection results_State_50 = null;
                        DiagnosticInfoCollection diagnosticInfos_State_50;

                        // Call Write Service
                        session.Write(null,
                                        nodesToWrite_State_50,
                                        out results_State_50,
                                        out diagnosticInfos_State_50);

                        foreach (StatusCode writeResult in results_State_50)
                        {
                            Console.WriteLine("     {0}", writeResult);
                        }

                        //Gooit JobOrder uit de wachtrij
                        JobOrdersDeeg1.Remove(JobOrdersDeeg1[0]);
                        counterJobOrderDeeg1 = JobOrdersDeeg1.Count;
						Console.WriteLine("Count Job Order Deeg 1: {0}",counterJobOrderDeeg1);
                        break;
                    case 70:
                        Console.WriteLine("Hold");
                        break;
                    case 100:
                        Console.WriteLine("Stopped");
                        break;
                    default:
                        break;
                }

            }
        }

        private static void OnStateBakLijn1(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);
                int state = Convert.ToInt32(value.Value);

                switch (state)
                {
                    case 10:
                        Console.WriteLine("Idle");
                        break;
                    case 30:
                        Console.WriteLine("Execute");
                        break;
                    case 50:
                        Console.WriteLine("Complete");
                        string[] names = { "GebakkenAfkeur", "Gebakken", "BakkenMax", "BakkenMin", "BakkenAVG" };
                        //Read nodes
                        IList<Type> types = new List<Type>();
                        IList<NodeId> nodeIdsRead = new List<NodeId>();
                        List<object> readValues;
                        List<ServiceResult> readResult;

                        types.Add(typeof(Int16));
                        types.Add(typeof(Int16)); 
                        types.Add(typeof(Int16)); 
                        types.Add(typeof(Int16));
                        types.Add(typeof(float));

                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""Q_i_BrodenAfkeurBakkenL1"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""Q_i_BrodenGebakkenL1"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""S_i_Bakken_max"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""S_i_Bakken_min"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""S_r_Bakken_AVG"""));

                        session.ReadValues(nodeIdsRead, types, out readValues, out readResult);
                        int intVars;
                        float fVars;

                        SqlData sqlData1 = new SqlData();
                        sqlData1.checkConnection();


                        for (int i = 0; i < readValues.Count; i++)
                        {
                            switch (names[i])
                            {
                                case "GebakkenAfkeur":
                                    intVars = Int16.Parse(readValues[i].ToString());
                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    break;
                                case "Gebakken":
                                    intVars = Int16.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    sqlData1.fillMaterialActualTabel((float)intVars, JobOrdersBakken1[0], names[i]);
                                    sqlData1.fillMaterialActualTabel((float)intVars, JobOrdersVerpakken1[0], names[i]);
                                    break;
                                case "BakkenMax":
                                    intVars = Int16.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    break;
                                case "BakkenMin":
                                    intVars = Int16.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    break;
                                case "BakkenAVG":
                                    fVars = float.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    break;
                                default:
                                    break;
                            }
                        }

                        //write to nodes
                        IList<NodeId> nodeIds = new List<NodeId>();
                        nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""PackML_Verpakken"".""I_b_Cmd_Start"""));
                        nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""PackML_Bakken"".""I_b_Cmd_Start"""));

                        object[] values = { true, false };

                        WriteValueCollection nodesToWrite = new WriteValueCollection();

                        for (int i = 0; i < nodeIds.Count; i++)
                        {
                            WriteValue bWriteValue = new WriteValue();
                            bWriteValue.NodeId = nodeIds[i];
                            bWriteValue.AttributeId = Attributes.Value;
                            bWriteValue.Value = new DataValue();
                            bWriteValue.Value.Value = values[i];
                            nodesToWrite.Add(bWriteValue);
                        }


                        // Write the node attributes
                        StatusCodeCollection results = null;
                        DiagnosticInfoCollection diagnosticInfos;

                        // Call Write Service
                        session.Write(null,
                                        nodesToWrite,
                                        out results,
                                        out diagnosticInfos);

                        foreach (StatusCode writeResult in results)
                        {
                            Console.WriteLine("     {0}", writeResult);
                        }

                        //Gooit JobOrder uit de wachtrij
                        JobOrdersBakken1.Remove(JobOrdersBakken1[0]);
                        counterJobOrderBakken1 = JobOrdersBakken1.Count;
                        Console.WriteLine("Count Job Order Bakken 1: {0}",counterJobOrderBakken1);

                        break;
                    case 70:
                        Console.WriteLine("Hold");
                        break;
                    case 100:
                        Console.WriteLine("Stopped");
                        break;
                    default:
                        break;
                }
            }
        }

        private static void OnStateVerpakkenLijn1(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);
                int state = Convert.ToInt32(value.Value);

                switch (state)
                {
                    case 10:
                        Console.WriteLine("Idle");
                        break;
                    case 30:
                        Console.WriteLine("Execute");
                        break;
                    case 50:
                        Console.WriteLine("Complete");
                        string[] names = { "Verpakt", "Stickers" };
                        //Read nodes
                        IList<Type> types = new List<Type>();
                        IList<NodeId> nodeIdsRead = new List<NodeId>();
                        List<object> readValues;
                        List<ServiceResult> readResult;

                        types.Add(typeof(Int16));
                        types.Add(typeof(Int16));

                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""Q_i_BrodenVerpaktL1"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""Produceren"".""Q_i_BrodenAfkStickersL1"""));


                        session.ReadValues(nodeIdsRead, types, out readValues, out readResult);
                        int intVars;
                        int tempVerpakt = 0;
                        SqlData sqlData1 = new SqlData();
                        sqlData1.checkConnection();


                        for (int i = 0; i < readValues.Count; i++)
                        {
                            switch (names[i])
                            {
                                case "Verpakt":
                                    intVars = Int16.Parse(readValues[i].ToString());
                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    tempVerpakt = intVars;
                                    sqlData1.fillMaterialActualTabel((float)intVars, JobOrdersVerpakken1[0], names[i]);
                                    break;
                                case "Stickers":
                                    intVars = Int16.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    sqlData1.fillMaterialActualTabel((float)(intVars + tempVerpakt), JobOrdersVerpakken1[0], names[i]);
                                    break;
                                default:
                                    break;
                            }
                        }


                        IList<NodeId> nodeIds = new List<NodeId>();
                        nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""PackML_Verpakken"".""I_b_Cmd_Start"""));
                        nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""PackMl_Deegverwerking"".""I_b_Cmd_Reset"""));
                        nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""PackML_Bakken"".""I_b_Cmd_Reset"""));
                        nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""PackML_Verpakken"".""I_b_Cmd_Reset"""));
                        

                        object[] values = { false, true, true, true };

                        WriteValueCollection nodesToWrite = new WriteValueCollection();

                        for (int i = 0; i < nodeIds.Count; i++)
                        {
                            WriteValue bWriteValue = new WriteValue();
                            bWriteValue.NodeId = nodeIds[i];
                            bWriteValue.AttributeId = Attributes.Value;
                            bWriteValue.Value = new DataValue();
                            bWriteValue.Value.Value = values[i];
                            nodesToWrite.Add(bWriteValue);
                        }
                                             
                        // Write the node attributes
                        StatusCodeCollection results = null;
                        DiagnosticInfoCollection diagnosticInfos;

                        // Call Write Service
                        session.Write(null,
                                        nodesToWrite,
                                        out results,
                                        out diagnosticInfos);

                        foreach (StatusCode writeResult in results)
                        {
                            Console.WriteLine("     {0}", writeResult);
                        }

                        Thread.Sleep(2000);

                        IList<NodeId> nodeIds1 = new List<NodeId>();
                        nodeIds1.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""PackMl_Deegverwerking"".""I_b_Cmd_Reset"""));
                        nodeIds1.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""PackML_Bakken"".""I_b_Cmd_Reset"""));
                        nodeIds1.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn1"".""PackML_Verpakken"".""I_b_Cmd_Reset"""));
                        nodeIds1.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""startJobsLijn1"""));

                        bool test = false;

						if (counterJobOrderDeeg1 == 0)
						{
                            test = false;
						}
						else
						{
                            test = true;
						}

                        object[] values1 = { false, false, false, test };

                        WriteValueCollection nodesToWrite1 = new WriteValueCollection();

                        for (int i = 0; i < nodeIds1.Count; i++)
                        {
                            WriteValue bWriteValue = new WriteValue();
                            bWriteValue.NodeId = nodeIds1[i];
                            bWriteValue.AttributeId = Attributes.Value;
                            bWriteValue.Value = new DataValue();
                            bWriteValue.Value.Value = values1[i];
                            nodesToWrite1.Add(bWriteValue);
                        }

                        // Write the node attributes
                        StatusCodeCollection results1 = null;
                        DiagnosticInfoCollection diagnosticInfos1;

                        // Call Write Service
                        session.Write(null,
                                        nodesToWrite1,
                                        out results1,
                                        out diagnosticInfos1);

                        foreach (StatusCode writeResult in results1)
                        {
                            Console.WriteLine("     {0}", writeResult);
                        }

                        //Gooit JobOrder uit de wachtrij
                        JobOrdersVerpakken1.Remove(JobOrdersVerpakken1[0]);
                        counterJobOrderVerpakken1 = JobOrdersVerpakken1.Count;
                        Console.WriteLine("Count Job Order Verpakken 1: {0}",counterJobOrderVerpakken1);
                        break;
                    case 70:
                        Console.WriteLine("Hold");
                        break;
                    case 100:
                        Console.WriteLine("Stopped");
                        break;
                    default:
                        break;
                }

            }
        }

        private static void OnStateDeegLijn2(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);
                int state = Convert.ToInt32(value.Value);

                switch (state)
                {
                    case 10:
                        Console.WriteLine("Idle");
                        break;
                    case 30:
                        Console.WriteLine("Execute");
                        break;
                    case 50:
                        Console.WriteLine("Complete");
                        string[] names = { "Mengtijd", "Part", "Bloem", "Boter", "Gist", "Meel", "Suiker", "Water", "Zout" };
                        //Read nodes
                        IList<Type> types = new List<Type>();
                        IList<NodeId> nodeIdsRead = new List<NodeId>();
                        List<object> readValues;
                        List<ServiceResult> readResult;

                        types.Add(typeof(Int32));
                        types.Add(typeof(Int16));
                        types.Add(typeof(float));
                        types.Add(typeof(float));
                        types.Add(typeof(float));
                        types.Add(typeof(float));
                        types.Add(typeof(float));
                        types.Add(typeof(float));
                        types.Add(typeof(float));

                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""S_di_WerkelijkMengtijd"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""S_i_AmountParts"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""ActueleWaarde"".""S_r_bloem"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""ActueleWaarde"".""S_r_boter"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""ActueleWaarde"".""S_r_gist"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""ActueleWaarde"".""S_r_meel"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""ActueleWaarde"".""S_r_suiker"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""ActueleWaarde"".""S_r_water"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""ActueleWaarde"".""S_r_zout"""));

                        session.ReadValues(nodeIdsRead, types, out readValues, out readResult);
                        int intVars;
                        float fVars;

                        SqlData sqlData1 = new SqlData();
                        sqlData1.checkConnection();


                        for (int i = 0; i < readValues.Count; i++)
                        {
                            switch (names[i])
                            {
                                case "Mengtijd":
                                    intVars = Int32.Parse(readValues[i].ToString());
                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    break;
                                case "Part":
                                    intVars = Int16.Parse(readValues[i].ToString());
                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    sqlData1.fillMaterialActualTabel((float)intVars, JobOrdersDeeg2[0], names[i]);
                                    sqlData1.fillMaterialActualTabel((float)intVars, JobOrdersBakken2[0], names[i]);
                                    break;
                                case "Bloem":
                                    fVars = float.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    sqlData1.fillMaterialActualTabel(fVars, JobOrdersDeeg2[0], names[i]);
                                    break;
                                case "Boter":
                                    fVars = float.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    sqlData1.fillMaterialActualTabel(fVars, JobOrdersDeeg2[0], names[i]);
                                    break;
                                case "Gist":
                                    fVars = float.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    sqlData1.fillMaterialActualTabel(fVars, JobOrdersDeeg2[0], names[i]);
                                    break;
                                case "Meel":
                                    fVars = float.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    sqlData1.fillMaterialActualTabel(fVars, JobOrdersDeeg2[0], names[i]);
                                    break;
                                case "Suiker":
                                    fVars = float.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    sqlData1.fillMaterialActualTabel(fVars, JobOrdersDeeg2[0], names[i]);
                                    break;
                                case "Water":
                                    fVars = float.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    sqlData1.fillMaterialActualTabel(fVars, JobOrdersDeeg2[0], names[i]);
                                    break;
                                case "Zout":
                                    fVars = float.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    sqlData1.fillMaterialActualTabel(fVars, JobOrdersDeeg2[0], names[i]);
                                    break;
                                default:
                                    break;
                            }
                        }

                        //Write nodes
                        IList<NodeId> nodeIds_State_50 = new List<NodeId>();
                        nodeIds_State_50.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""PackML_Bakken"".""I_b_Cmd_Start"""));
                        nodeIds_State_50.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""PackMl_Deegverwerking"".""I_b_Cmd_Start"""));
                        nodeIds_State_50.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""startenOrder"""));
                        nodeIds_State_50.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""startJobsLijn2"""));

                        object[] values_State_50 = { true, false, false, false };

                        WriteValueCollection nodesToWrite_State_50 = new WriteValueCollection();

                        for (int i = 0; i < nodeIds_State_50.Count; i++)
                        {
                            WriteValue bWriteValue = new WriteValue();
                            bWriteValue.NodeId = nodeIds_State_50[i];
                            bWriteValue.AttributeId = Attributes.Value;
                            bWriteValue.Value = new DataValue();
                            bWriteValue.Value.Value = values_State_50[i];
                            nodesToWrite_State_50.Add(bWriteValue);
                        }

                        // Write the node attributes
                        StatusCodeCollection results_State_50 = null;
                        DiagnosticInfoCollection diagnosticInfos_State_50;

                        // Call Write Service
                        session.Write(null,
                                        nodesToWrite_State_50,
                                        out results_State_50,
                                        out diagnosticInfos_State_50);

                        foreach (StatusCode writeResult in results_State_50)
                        {
                            Console.WriteLine("     {0}", writeResult);
                        }

                        //Gooit JobOrder uit de wachtrij
                        JobOrdersDeeg2.Remove(JobOrdersDeeg2[0]);
                        counterJobOrderDeeg2 = JobOrdersDeeg2.Count;
                        Console.WriteLine("Count Job Order Deeg 2: {0}", counterJobOrderDeeg2);
                        break;
                    case 70:
                        Console.WriteLine("Hold");
                        break;
                    case 100:
                        Console.WriteLine("Stopped");
                        break;
                    default:
                        break;
                }
            }
        }

        private static void OnStateBakLijn2(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);
                int state = Convert.ToInt32(value.Value);

                switch (state)
                {
                    case 10:
                        Console.WriteLine("Idle");
                        break;
                    case 30:
                        Console.WriteLine("Execute");
                        break;
                    case 50:
                        Console.WriteLine("Complete");
                        string[] names = { "GebakkenAfkeur", "Gebakken", "BakkenMax", "BakkenMin", "BakkenAVG" };
                        //Read nodes
                        IList<Type> types = new List<Type>();
                        IList<NodeId> nodeIdsRead = new List<NodeId>();
                        List<object> readValues;
                        List<ServiceResult> readResult;

                        types.Add(typeof(Int16));
                        types.Add(typeof(Int16));
                        types.Add(typeof(Int16));
                        types.Add(typeof(Int16));
                        types.Add(typeof(float));

                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""Q_i_BrodenAfkeurBakkenL2"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""Q_i_BrodenGebakkenL2"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""S_i_Bakken_max"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""S_i_Bakken_min"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""S_r_Bakken_avg"""));

                        session.ReadValues(nodeIdsRead, types, out readValues, out readResult);

                        int intVars;
                        float fVars;

                        SqlData sqlData1 = new SqlData();
                        sqlData1.checkConnection();


                        for (int i = 0; i < readValues.Count; i++)
                        {
                            switch (names[i])
                            {
                                case "GebakkenAfkeur":
                                    intVars = Int16.Parse(readValues[i].ToString());
                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    break;
                                case "Gebakken":
                                    intVars = Int16.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    sqlData1.fillMaterialActualTabel((float)intVars, JobOrdersBakken2[0], names[i]);
                                    sqlData1.fillMaterialActualTabel((float)intVars, JobOrdersVerpakken2[0], names[i]);
                                    break;
                                case "BakkenMax":
                                    intVars = Int16.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    break;
                                case "BakkenMin":
                                    intVars = Int16.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    break;
                                case "BakkenAVG":
                                    fVars = float.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], fVars);
                                    break;
                                default:
                                    break;
                            }
                        }


                        //write to nodes
                        IList<NodeId> nodeIds = new List<NodeId>();
                        nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""PackML_Verpakken"".""I_b_Cmd_Start"""));
                        nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""PackML_Bakken"".""I_b_Cmd_Start"""));

                        object[] values = { true, false };

                        WriteValueCollection nodesToWrite = new WriteValueCollection();

                        for (int i = 0; i < nodeIds.Count; i++)
                        {
                            WriteValue bWriteValue = new WriteValue();
                            bWriteValue.NodeId = nodeIds[i];
                            bWriteValue.AttributeId = Attributes.Value;
                            bWriteValue.Value = new DataValue();
                            bWriteValue.Value.Value = values[i];
                            nodesToWrite.Add(bWriteValue);
                        }


                        // Write the node attributes
                        StatusCodeCollection results = null;
                        DiagnosticInfoCollection diagnosticInfos;

                        // Call Write Service
                        session.Write(null,
                                        nodesToWrite,
                                        out results,
                                        out diagnosticInfos);

                        foreach (StatusCode writeResult in results)
                        {
                            Console.WriteLine("     {0}", writeResult);
                        }

                        //Gooit JobOrder uit de wachtrij
                        JobOrdersBakken2.Remove(JobOrdersBakken2[0]);
                        counterJobOrderBakken2 = JobOrdersBakken2.Count;
                        Console.WriteLine("Count Job Order Bakken 2: {0}", counterJobOrderBakken2);

                        break;
                    case 70:
                        Console.WriteLine("Hold");
                        break;
                    case 100:
                        Console.WriteLine("Stopped");
                        break;
                    default:
                        break;
                }
            }
        }

        private static void OnStateVerpakkenLijn2(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);
                int state = Convert.ToInt32(value.Value);

                switch (state)
                {
                    case 10:
                        Console.WriteLine("Idle");
                        break;
                    case 30:
                        Console.WriteLine("Execute");
                        break;
                    case 50:
                        Console.WriteLine("Complete");
                        string[] names = { "Verpakt", "Stickers" };
                        //Read nodes
                        IList<Type> types = new List<Type>();
                        IList<NodeId> nodeIdsRead = new List<NodeId>();
                        List<object> readValues;
                        List<ServiceResult> readResult;

                        types.Add(typeof(Int16));
                        types.Add(typeof(Int16));

                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""Q_i_BrodenVerpaktL2"""));
                        nodeIdsRead.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""Produceren"".""Q_i_BrodenAfkStickersL2"""));


                        session.ReadValues(nodeIdsRead, types, out readValues, out readResult);

                        int intVars;
                        int tempVerpakt = 0;
                        SqlData sqlData1 = new SqlData();
                        sqlData1.checkConnection();


                        for (int i = 0; i < readValues.Count; i++)
                        {
                            switch (names[i])
                            {
                                case "Verpakt":
                                    intVars = Int16.Parse(readValues[i].ToString());
                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    tempVerpakt = intVars;
                                    sqlData1.fillMaterialActualTabel((float)intVars, JobOrdersVerpakken2[0], names[i]);
                                    break;
                                case "Stickers":
                                    intVars = Int16.Parse(readValues[i].ToString());

                                    Console.WriteLine("{0} = {1}", names[i], intVars);
                                    sqlData1.fillMaterialActualTabel((float)(intVars + tempVerpakt), JobOrdersVerpakken2[0], names[i]);
                                    break;
                                default:
                                    break;
                            }
                        }


                        IList<NodeId> nodeIds = new List<NodeId>();
                        nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""PackML_Verpakken"".""I_b_Cmd_Start"""));
                        nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""PackMl_Deegverwerking"".""I_b_Cmd_Reset"""));
                        nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""PackML_Bakken"".""I_b_Cmd_Reset"""));
                        nodeIds.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""PackML_Verpakken"".""I_b_Cmd_Reset"""));


                        object[] values = { false, true, true, true };

                        WriteValueCollection nodesToWrite = new WriteValueCollection();

                        for (int i = 0; i < nodeIds.Count; i++)
                        {
                            WriteValue bWriteValue = new WriteValue();
                            bWriteValue.NodeId = nodeIds[i];
                            bWriteValue.AttributeId = Attributes.Value;
                            bWriteValue.Value = new DataValue();
                            bWriteValue.Value.Value = values[i];
                            nodesToWrite.Add(bWriteValue);
                        }

                        // Write the node attributes
                        StatusCodeCollection results = null;
                        DiagnosticInfoCollection diagnosticInfos;

                        // Call Write Service
                        session.Write(null,
                                        nodesToWrite,
                                        out results,
                                        out diagnosticInfos);

                        foreach (StatusCode writeResult in results)
                        {
                            Console.WriteLine("     {0}", writeResult);
                        }

                        Thread.Sleep(2000);

                        IList<NodeId> nodeIds1 = new List<NodeId>();
                        nodeIds1.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""PackMl_Deegverwerking"".""I_b_Cmd_Reset"""));
                        nodeIds1.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""PackML_Bakken"".""I_b_Cmd_Reset"""));
                        nodeIds1.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""lijn2"".""PackML_Verpakken"".""I_b_Cmd_Reset"""));
                        nodeIds1.Add(new NodeId(@"ns=3;s=""db_OPCdata"".""startJobsLijn2"""));

                        bool test = false;

                        if (counterJobOrderDeeg2 == 0)
                        {
                            test = false;
                        }
                        else
                        {
                            test = true;
                        }

                        object[] values1 = { false, false, false, test };

                        WriteValueCollection nodesToWrite1 = new WriteValueCollection();

                        for (int i = 0; i < nodeIds1.Count; i++)
                        {
                            WriteValue bWriteValue = new WriteValue();
                            bWriteValue.NodeId = nodeIds1[i];
                            bWriteValue.AttributeId = Attributes.Value;
                            bWriteValue.Value = new DataValue();
                            bWriteValue.Value.Value = values1[i];
                            nodesToWrite1.Add(bWriteValue);
                        }

                        // Write the node attributes
                        StatusCodeCollection results1 = null;
                        DiagnosticInfoCollection diagnosticInfos1;

                        // Call Write Service
                        session.Write(null,
                                        nodesToWrite1,
                                        out results1,
                                        out diagnosticInfos1);

                        foreach (StatusCode writeResult in results1)
                        {
                            Console.WriteLine("     {0}", writeResult);
                        }

                        //Gooit JobOrder uit de wachtrij
                        JobOrdersVerpakken2.Remove(JobOrdersVerpakken2[0]);
                        counterJobOrderVerpakken2 = JobOrdersVerpakken2.Count;
                        Console.WriteLine("Count Job Order Verpakken 2: {0}", counterJobOrderVerpakken2);
                        break;
                    case 70:
                        Console.WriteLine("Hold");
                        break;
                    case 100:
                        Console.WriteLine("Stopped");
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
