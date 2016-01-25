using NUnit.Framework;
using OSMToSCT2.TextConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMToSCT2.Tests.ConverterFixtures
{
    [TestFixture]
    public class OSMToSCTTextConverterFixture
    {
        private OSMToSCTTextConverter mConverter;

        [SetUp]
        public void Setup()
        {
            mConverter = new OSMToSCTTextConverter("RUNWAYCOLOR", "TAXIWAYCOLOR", "APRONCOLOR", "TERMINALCOLOR", "HANGARCOLOR", "OTHERCOLOR");
        }

        [TestCase(OSMToSCTTextConverterFixture.inputOSM1, OSMToSCTTextConverterFixture.outputSCT1)]
        public void General(string inputOSM, string expectedOutputSCT)
        {
            String output = mConverter.Convert(inputOSM);

            Assert.AreEqual(output.Trim(), expectedOutputSCT.Trim());           
        }

        public const string genericOSMHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
        public const string inputOSM1 =
            genericOSMHeader +
            "<osm version=\"0.6\" generator=\"OSMToSCT2\">" +
            "  <node id=\"1\" action=\"modify\" visible=\"true\" lat=\"1.0\" lon=\"-1.0\" />" +
            "  <node id=\"2\" action=\"modify\" visible=\"true\" lat=\"-2.0\" lon=\"2.0\" />" +
            "  <node id=\"3\" action=\"modify\" visible=\"true\" lat=\"30.0\" lon=\"3.0\" />" +
            "  <node id=\"4\" action=\"modify\" visible=\"true\" lat=\"-40.0\" lon=\"-4.0\" />" +
            "  <way id=\"10\" action=\"modify\" visible=\"true\">" +
            "    <nd ref=\"1\" />" +
            "    <nd ref=\"2\" />" +
            "    <nd ref=\"3\" />" +
            "    <nd ref=\"4\" />" +
            "    <nd ref=\"1\" />" +
            "    <tag k=\"aeroway\" v=\"runway\" />" +
            "    <tag k=\"ref\" v=\"Test Runway 1\" />" +
            "  </way>" +
            "</osm>";
        public const string outputSCT1 =
            ";- Test Runway 1\r\n" +
            "RUNWAYCOLOR  N001.00.00.000 W001.00.00.000\r\n" +
            "  S002.00.00.000 E002.00.00.000\r\n" +
            "  N030.00.00.000 E003.00.00.000\r\n" +
            "  S040.00.00.000 W004.00.00.000\r\n";
    }
}
