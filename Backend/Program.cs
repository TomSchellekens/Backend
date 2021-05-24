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

                    var endpointDescription = CoreClientUtils.SelectEndpoint("opc.tcp://192.168.8.145:4840", false);
                    //var endpointDescription = CoreClientUtils.SelectEndpoint("opc.tcp://192.168.1.145:4840", false);

                    ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                    //UAClient uaClient = new UAClient(application.ApplicationConfiguration);
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

                //Invoeren Order 
                var enterOrder = new MonitoredItem(subscription.DefaultItem) { DisplayName = "InvoerenOrder", StartNodeId = @"ns=3;s=""db_OPCdata"".""invoerenOrder""" };
                enterOrder.Notification += (sender, e) => OnEnteringOrder(sender, e, session);
                subscription.AddItem(enterOrder);

                //Vrijgeven Order
                var realeaseOrder = new MonitoredItem(subscription.DefaultItem) { DisplayName = "VrijgevenOrder", StartNodeId = @"ns=3;s=""db_OPCdata"".""vrijgevenOrder""" };
                realeaseOrder.Notification += (sender, e) => OnReleasingOrder(sender, e, session);
                subscription.AddItem(realeaseOrder);

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


        private static void OnEnteringOrder(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
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

        private static void OnReleasingOrder(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
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

        private static void OnStartingOrder(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
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
				Console.WriteLine(value.Value);
            }
        }

        private static void OnStateBakLijn1(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);
            }
        }

        private static void OnStateVerpakkenLijn1(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);

            }
        }

        private static void OnStateDeegLijn2(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);
            }
        }

        private static void OnStateBakLijn2(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);
            }
        }

        private static void OnStateVerpakkenLijn2(MonitoredItem item, MonitoredItemNotificationEventArgs e, Session session)
        {
            foreach (var value in item.DequeueValues())
            {
                Console.WriteLine("{0} = {1}", item.DisplayName, value.Value);
            }
        }

    }
}




