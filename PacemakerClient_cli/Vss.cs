using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace PacemakerClient_cli
{
    internal class Vss
    {
        public static void VssStart()
        {
            
            string serviceName = "VSS";

            // Set up a connection to the WMI service
            ManagementScope scope = new ManagementScope("\\\\.\\root\\cimv2");
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Service WHERE Name = '" + serviceName + "'");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

            try
            {
                // Execute the query to get a collection of matching services
                ManagementObjectCollection services = searcher.Get();

                foreach (ManagementObject service in services.Cast<ManagementObject>())
                {
                    // Start the service
                    service.InvokeMethod("StartService", null);

                    // Print a message indicating the service was started
                    Console.WriteLine("[*] Signal sent to start the " + service["Name"] + " service.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            
        }

        public static void VssStop()
        {
            string serviceName = "VSS";

            // Stop the service
            ServiceController serviceController = new ServiceController(serviceName);

            try
            {
                if (serviceController.Status == ServiceControllerStatus.Running)
                {
                    serviceController.Stop();
                    serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                    Console.WriteLine("[*] Signal sent to stop the " + serviceName + " service.");
                }
                else
                {
                    Console.WriteLine("[*] The " + serviceName + " service is not running.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[*] Failed to stop the " + serviceName + " service. Error: " + ex.Message);
            }
        }

        public static string VssStatus()
        {
            string serviceName = "VSS";
            string serviceState = "Stopped";

            // Set up a connection to the WMI service
            ManagementScope scope = new ManagementScope("\\\\.\\root\\cimv2");
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Service WHERE Name = '" + serviceName + "'");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

            try
            {
                // Execute the query to get a collection of matching services
                ManagementObjectCollection services = searcher.Get();

                foreach (ManagementObject service in services.Cast<ManagementObject>())
                {
                    serviceState = service["State"].ToString();
                    Console.WriteLine("[*] " + serviceState);

                    
                }
                return serviceState;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return serviceState;
            }
        }

        public static void VssList()
        {
            
            // Set up a connection to the WMI service
            ManagementScope scope = new ManagementScope(@"\\.\root\cimv2");
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_ShadowCopy");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

            try
            {
                Console.WriteLine("SHADOW COPIES");
                Console.WriteLine("=============");
                Console.WriteLine();

                // Execute the query to get a collection of shadow copies
                ManagementObjectCollection shadowCopies = searcher.Get();
                Console.Write(shadowCopies);

                foreach (ManagementObject shadowCopy in shadowCopies.Cast<ManagementObject>())
                {
                    Console.WriteLine("[*] ID:                  " + shadowCopy["ID"]);
                    Console.WriteLine("[*] Client accessible:   " + shadowCopy["ClientAccessible"]);
                    Console.WriteLine("[*] Count:               " + shadowCopy["Count"]);
                    Console.WriteLine("[*] Device object:       " + shadowCopy["DeviceObject"]);
                    Console.WriteLine("[*] Differential:        " + shadowCopy["Differential"]);
                    Console.WriteLine("[*] Exposed locally:     " + shadowCopy["ExposedLocally"]);
                    Console.WriteLine("[*] Exposed name:        " + shadowCopy["ExposedName"]);
                    Console.WriteLine("[*] Exposed remotely:    " + shadowCopy["ExposedRemotely"]);
                    Console.WriteLine("[*] Hardware assisted:   " + shadowCopy["HardwareAssisted"]);
                    Console.WriteLine("[*] Imported:            " + shadowCopy["Imported"]);
                    Console.WriteLine("[*] No auto release:     " + shadowCopy["NoAutoRelease"]);
                    Console.WriteLine("[*] Not surfaced:        " + shadowCopy["NotSurfaced"]);
                    Console.WriteLine("[*] No writers:          " + shadowCopy["NoWriters"]);
                    Console.WriteLine("[*] Originating machine: " + shadowCopy["OriginatingMachine"]);
                    Console.WriteLine("[*] Persistent:          " + shadowCopy["Persistent"]);
                    Console.WriteLine("[*] Plex:                " + shadowCopy["Plex"]);
                    Console.WriteLine("[*] Provider ID:         " + shadowCopy["ProviderID"]);
                    Console.WriteLine("[*] Service machine:     " + shadowCopy["ServiceMachine"]);
                    Console.WriteLine("[*] Set ID:              " + shadowCopy["SetID"]);
                    Console.WriteLine("[*] State:               " + shadowCopy["State"]);
                    Console.WriteLine("[*] Transportable:       " + shadowCopy["Transportable"]);
                    Console.WriteLine("[*] Volume name:         " + shadowCopy["VolumeName"]);
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex);
            }
            
        }

        public static void VssCreate()
        {
            // Get the drive letter of the primary drive (usually C:\)
            string driveLetter = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)).Substring(0, 1);

            // Set up a connection to the WMI service
            ManagementScope scope = new ManagementScope(@"\\.\root\cimv2");
            ManagementClass shadowCopyClass = new ManagementClass(scope, new ManagementPath("Win32_ShadowCopy"), null);

            try
            {
                // Create the shadow copy
                ManagementBaseObject inParams = shadowCopyClass.GetMethodParameters("Create");
                inParams["Volume"] = driveLetter + ":\\";
                inParams["Context"] = "ClientAccessible";
                ManagementBaseObject outParams = shadowCopyClass.InvokeMethod("Create", inParams, null);

                // Check the result of the operation
                uint errResult = (uint)(outParams["ReturnValue"]);
                if (errResult == 0)
                {
                    Console.WriteLine("[*] Shadow copy created successfully.");
                }
                else
                {
                    Console.WriteLine("[*] Failed to create a shadow copy. Error code: " + errResult);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            
        }
    }
}
