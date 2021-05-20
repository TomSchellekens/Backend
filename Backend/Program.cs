using System;
using System.Collections.Generic;
using System.Threading;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace Quickstarts.Backend
{
    public static class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Trying to connect...");

                    // Define the UA Client application
                    ApplicationInstance application = new ApplicationInstance();
                    application.ApplicationName = "Quickstart Console Backend";
                    application.ApplicationType = ApplicationType.Client;

                    // load the application configuration.
                    application.LoadApplicationConfiguration("Quickstarts.Backend.Config.xml", silent: false);

                    // check the application certificate.
                    application.CheckApplicationInstanceCertificate(silent: false, minimumKeySize: 0);

                    EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(application.ApplicationConfiguration);

                    var endpointDescription = CoreClientUtils.SelectEndpoint("opc.tcp://192.168.1.145:4840", false);

                    ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                    //UAClient uaClient = new UAClient(application.ApplicationConfiguration);
                    application.ApplicationConfiguration.CertificateValidator.CertificateValidation += CertificateValidation;

                    //makeSession(application, endpoint);



                    //start session to the OPC server
                    using (var session = Session.Create(application.ApplicationConfiguration, endpoint, false, false, application.ApplicationName, 30 * 60 * 1000, new UserIdentity(), null).GetAwaiter().GetResult())
                    {
                        var subscription = new Subscription(session.DefaultSubscription) { PublishingInterval = 20 };

                        //Start read
                        var startRead = new MonitoredItem(subscription.DefaultItem) { DisplayName = "Test", StartNodeId = @"ns=3;s=""Test""" };
                        startRead.Notification += (sender, e) => OnStartReading(sender, e, session);
                        subscription.AddItem(startRead);

                        //Add subscription to de session
                        session.AddSubscription(subscription);
                        subscription.Create();
                        session.PublishError += (sender, e) => { Console.WriteLine("Error detected."); };

                        Console.WriteLine("Connected.");




                        while (!session.KeepAliveStopped)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }


        private static void makeSession(ApplicationInstance application, ConfiguredEndpoint endpoint)
        {
            bool currentState;
            using (var session = Session.Create(application.ApplicationConfiguration, endpoint, false, false, application.ApplicationName, 30 * 60 * 1000, new UserIdentity(), null).GetAwaiter().GetResult())
            {



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

        private static void OnStartReading(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);
                if ((bool)value.Value == true)
                {
                    // Write the configured nodes
                    WriteValueCollection nodesToWrite = new WriteValueCollection();

                    //make random array
                    int[] arr = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

                    for (int i = 0; i < arr.Length; i++)
                    {
                        WriteValue intWriteVal = new WriteValue();
                        intWriteVal.NodeId = new NodeId(@"ns=3;s=""OPC_UA_Data"".""pos_x""[" + i + "]");
                        intWriteVal.AttributeId = Attributes.Value;
                        intWriteVal.Value = new DataValue();
                        intWriteVal.Value.Value = arr[i];
                        nodesToWrite.Add(intWriteVal);
                    }
                    // Write the node attributes
                    StatusCodeCollection results = null;
                    DiagnosticInfoCollection diagnosticInfos;

                    // Call Write Service
                    session.Write(null,
                                    nodesToWrite,
                                    out results,
                                    out diagnosticInfos);
                }
                if ((bool)value.Value == false)
                {
                    // Write the configured nodes
                    WriteValueCollection nodesToWrite = new WriteValueCollection();

                    //make random array
                    int[] arr = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                    for (int i = 0; i < arr.Length; i++)
                    {
                        WriteValue intWriteVal = new WriteValue();
                        intWriteVal.NodeId = new NodeId(@"ns=3;s=""OPC_UA_Data"".""pos_x""[" + i + "]");
                        intWriteVal.AttributeId = Attributes.Value;
                        intWriteVal.Value = new DataValue();
                        intWriteVal.Value.Value = arr[i];
                        nodesToWrite.Add(intWriteVal);
                    }
                    // Write the node attributes
                    StatusCodeCollection results = null;
                    DiagnosticInfoCollection diagnosticInfos;

                    // Call Write Service
                    session.Write(null,
                                    nodesToWrite,
                                    out results,
                                    out diagnosticInfos);
                }

            }
        }
    }
}
