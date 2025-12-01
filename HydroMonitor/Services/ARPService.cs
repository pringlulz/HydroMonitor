using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//https://www.maniuk.net/2016/08/get-ip-address-by-mac-address-in-csharp.html
namespace HydroMonitor.Services
{

    public class ARPService
    {
        public class ArpEntity
        {
            public string Ip { get; set; }
            public string MacAddress { get; set; }
            public string Type { get; set; }
        }
        public static List<String> GetMacAddresses()
        {
            return GetMacAddresses(GetArpResult());
        }

        public static List<String> GetMacAddresses(List<ArpEntity> list)
        { //fun fact! the test I wrote for this action actually caught the dashes and colons issue! isn't that neat
            return list.Select(i => i.MacAddress.Replace('-', ':')).ToList();
        }
        public static List<ArpEntity> GetArpResult() {

            var p = Process.Start(new ProcessStartInfo("arp", "-a")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });

            var output = p?.StandardOutput.ReadToEnd();
            p?.Close();
            return ParseArpResult(output);
          }

        public static String GetMacForIP(String IPAddress)
        {
            ArpEntity arp = GetArpResult().Find(a => IPAddress.Contains(a.Ip));
            return arp?.MacAddress.Replace('-', ':');
        }

        private static List<ArpEntity> ParseArpResult(string output)
        {
            var lines = output.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l));

            var result =
                (from line in lines
                 select Regex.Split(line, @"\s+")
                    .Where(i => !string.IsNullOrWhiteSpace(i)).ToList()
                    into items
                 where items.Count == 3
                 select new ArpEntity()
                 {
                     Ip = items[0],
                     MacAddress = items[1].ToLower(),
                     Type = items[2]
                 });

            return result.ToList();
        }
    }
}
