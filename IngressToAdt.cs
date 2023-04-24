using System;
using System.Threading.Tasks;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace _07JP27.HomeDigitalTwin
{
    public class IngressToAdt
    {
        private static readonly string AdtInstanceUrl = Environment.GetEnvironmentVariable("ADT_SERVICE_URL");

        [FunctionName("IngressToAdt")]
        public async Task RunAsync([TimerTrigger("0 */10 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Ingress to ADT Timer trigger function started at: {DateTime.Now}");

            if (AdtInstanceUrl == null) throw new ArgumentNullException("AdtInstanceUrl");

             try
            {
                // Authenticate with Managed ID
                var cred = new DefaultAzureCredential();
                var client = new DigitalTwinsClient(new Uri(AdtInstanceUrl), cred);
                log.LogInformation($"ADT service client connection created.");
               
                // random class for mock data
                Random random = new System.Random();

                // Update twin with device status
                var meterData = new Azure.JsonPatchDocument();
                meterData.AppendReplace("/Humidity", random.Next(20, 70));
                meterData.AppendReplace("/Temperature", random.Next(0, 35));
                await client.UpdateDigitalTwinAsync("Meter1", meterData);

                var circulatorData = new Azure.JsonPatchDocument();
                circulatorData.AppendReplace("/PowerOn", random.Next(0, 2) == 1);
                await client.UpdateDigitalTwinAsync("Circulator1", circulatorData);

                var humidifierData = new Azure.JsonPatchDocument();
                humidifierData.AppendReplace("/LackWater", random.Next(0, 2) == 1);
                humidifierData.AppendReplace("/PowerOn", random.Next(0, 2) == 1);
                humidifierData.AppendReplace("/NebulizationEfficiency", random.Next(0, 100));
                await client.UpdateDigitalTwinAsync("Humidifier1", humidifierData);
            }
            catch (Exception ex)
            {
                log.LogError($"Error in ingest function: {ex.Message}");
                throw;
            }

            log.LogInformation($"Ingress to ADT Timer trigger function finished at: {DateTime.Now}");
        }
    }
}
