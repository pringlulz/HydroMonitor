using HydroMonitor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HydroMonitorTests
{
    



    [TestClass]
    public class ARPServiceTest
    {

        [TestMethod]
        public void TestGetMacAddresses()
        { //Make sure the list is fetched and contains items in the right kinda format.

            try {
                List<string> list = ARPService.GetMacAddresses();
                
                Assert.IsTrue(list.Count > 0);
                Assert.IsTrue(Regex.Match(list.First(), "^([0-9a-fA-F]{2}(?:[:]?[0-9a-fA-F]{2}){5})$").Success);
            } catch (Exception ex) {
                Assert.Fail(ex.Message);
            }
        }
        [TestMethod]
        public void TestGetArpResult()
        {
            List<ARPService.ArpEntity> result = ARPService.GetArpResult();
            
            Assert.IsTrue(result.Count > 0);

        }


        
    }
}
