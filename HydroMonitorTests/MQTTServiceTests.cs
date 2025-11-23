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
        private SensorTypeDAO sensorTypeDAO;
        private SensorDAO sensorDAO;
        private SensorReadingDAO readingDAO;


        [TestInitialize]
        public void init()
        {
            //TODO: replace these with mocked versions later
            sensorTypeDAO = new SensorTypeDAO();
            sensorDAO =  new SensorDAO(sensorTypeDAO); //new MockSensorDAO();
            readingDAO = new SensorReadingDAO();

            service = new MQTTService(sensorDAO, readingDAO);
        }

        [TestMethod]
        public void BrokerTest()
        {
            service.SetupBroker();

            
        }

        [TestCleanup]
        public void Cleanup()
        {
            sensorTypeDAO.Destroy();
            sensorDAO.Destroy();
            readingDAO.Destroy();

            service.Close();
        }



    }
}
