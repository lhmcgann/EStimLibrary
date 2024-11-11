using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using EStimLibrary.Core.HardwareInterfaces;
using EStimLibrary.Core.Stimulation;

namespace EStimLibrary.Tests
{
    [TestFixture]
    public class ThreadConfigDataPerStimulatorTests
    {
        [Test]
        public void Test_ShallowCopy_Behavior()
        {
            // Arrange
            int globalStimId = 1;

            // Create IndependentLeads
            var lead1 = new Lead("Lead1", new Channel(1));
            var lead2 = new Lead("Lead2", new Channel(2));
            var independentLeads = new List<Lead> { lead1, lead2 };

            // Create StimParamData
            var stimParamData = new Dictionary<string, Tuple<IDataLimits, object>>();

            var dataLimits = new ContinuousDataLimits(0, 100);
            stimParamData["Param1"] = new Tuple<IDataLimits, object>(dataLimits, 50.0);

            // Create ModulatableStimParams
            var modulatableStimParams = new SortedSet<string> { "Param1" };

            // Act
            var threadConfig = new ThreadConfigDataPerStimulator(
                globalStimId,
                independentLeads,
                stimParamData,
                modulatableStimParams);

            // Modify the original data structures after creating threadConfig
            independentLeads.Add(new Lead("Lead3", new Channel(3)));
            stimParamData["Param2"] = new Tuple<IDataLimits, object>(dataLimits, 75.0);
            modulatableStimParams.Add("Param2");

            // Modify nested data in stimParamData
            dataLimits.MinBound = -10; // Since dataLimits is mutable

            // Assert

            // Check if threadConfig.IndependentLeads has the new lead
            Assert.AreEqual(3, threadConfig.IndependentLeads.Count(), "IndependentLeads should have 3 leads if shallow copy");

            // Check if threadConfig.StimParamData has the new parameter
            Assert.IsTrue(threadConfig.StimParamData.ContainsKey("Param2"), "StimParamData should contain 'Param2' if shallow copy");

            // Check if threadConfig.ModulatableStimParams has the new parameter
            Assert.IsTrue(threadConfig.ModulatableStimParams.Contains("Param2"), "ModulatableStimParams should contain 'Param2' if shallow copy");

            // Check if dataLimits.MinBound in threadConfig reflects the change
            var limitsInThreadConfig = threadConfig.StimParamData["Param1"].Item1 as ContinuousDataLimits;
            Assert.AreEqual(-10, limitsInThreadConfig.MinBound, "DataLimits MinBound should reflect the change if shallow copy");

            // Modify data in threadConfig and see if original data changes
            var firstLead = threadConfig.IndependentLeads.First();
            firstLead.Name = "ModifiedLead1";

            Assert.AreEqual("ModifiedLead1", independentLeads[0].Name, "Original independentLeads should reflect changes made through threadConfig if shallow copy");
        }
    }
}
