using HydroMonitor.Interfaces;
using HydroMonitor.Models;
using HydroMonitor.Repository;
using HydroMonitor.Services;
using HydroMonitorTests.MockRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydroMonitorTests
{
    [TestClass]
    public class MQTTServiceTests
    {
        private MQTTService service;
        private MockSensorTypeDAO sensorTypeDAO;
        private MockSensorDAO sensorDAO;
        private MockSensorReadingDAO readingDAO;
        Random random = new Random();


        [TestInitialize]
        public void init()
        {
            //TODO: replace these with mocked versions later
            sensorTypeDAO = new MockSensorTypeDAO();
            sensorDAO =  new MockSensorDAO(sensorTypeDAO); //new MockSensorDAO();
            readingDAO = new MockSensorReadingDAO();

            //service = new MQTTService(sensorDAO, readingDAO);
        }

        [TestMethod]
        public void BrokerTest()
        {
            //how to test the service when all the important bits are private?
            //service.SetupBroker();
            //var sensorId = random.Next();
            //for (int i = 0; i < 10; i++)
            //{
            //    service.SaveToDatabase(Convert.ToString(random.NextDouble() * 10), sensorId, DateTime.Now);
            //}

            //Assert.IsTrue(readingDAO.Get(sensorId).Count == 10);

        }

        [TestCleanup]
        public void Cleanup()
        {
            sensorTypeDAO.Destroy();
            sensorDAO.Destroy();
            readingDAO.Destroy();

            //service.Close();
        }



    }
}
