using System;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Configuration;

namespace Quickstarts.ConsoleReferenceClient
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            IOutput console = new ConsoleOutput();
            console.WriteLine("OPC UA Console Reference Client");
            try
            {
                // Define the UA Client application
                ApplicationInstance application = new ApplicationInstance();
                application.ApplicationName = "Quickstart Console Reference Client";
                application.ApplicationType = ApplicationType.Client;

                // load the application configuration.
                await application.LoadApplicationConfiguration("ConsoleReferenceClient.Config.xml", silent: false);
                // check the application certificate.
                await application.CheckApplicationInstanceCertificate(silent: false, minimumKeySize: 0);

                // create the UA Client object and connect to configured server.
                UAClient uaClient = new UAClient(application.ApplicationConfiguration, console, ClientBase.ValidateResponse);

                bool connected = await uaClient.ConnectAsync();
                if (connected)
                {
                    // Run tests for available methods.
                    uaClient.ReadNodes();
                    uaClient.WriteNodes();
                    uaClient.Browse();
                    uaClient.CallMethod();

                    uaClient.SubscribeToDataChanges();
                    // Wait for some DataChange notifications from MonitoredItems
                    await Task.Delay(20_000);

                    uaClient.Disconnect();
                }
                else
                {
                    console.WriteLine("Hallo");
                }

                console.WriteLine("\nProgram ended.");
                console.WriteLine("Press any key to finish...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }
    }
}
