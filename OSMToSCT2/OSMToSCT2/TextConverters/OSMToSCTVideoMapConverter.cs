using OSMToSCT2.Geo;
using OSMToSCT2.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace OSMToSCT2.TextConverters
{
    public class OSMToSCTVideoMapConverter
    {
        public OSMToSCTVideoMapConverter()
        { }

        public string Convert(string inputOSM)
        {
            string outputSCT;
            Dictionary<int, Shape> shapeDict;
            Dictionary<int, Node> nodeDict;

            parseOSM(inputOSM, out shapeDict, out nodeDict);

            outputSCT = buildSCT(shapeDict.Values);

            return outputSCT;
        }
        
        private void parseOSM(string inputOSM, out Dictionary<int, Shape> shapeDict, out Dictionary<int, Node> nodeDict)
        {
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator nodeIterator;
            XPathNodeIterator wayIterator;
            XPathNodeIterator nameIterator;
            XPathNodeIterator wayPointIterator;
            string nameValue;
            int waypointIdValue;
            StreamWriter streamWriter;
            MemoryStream memoryStream;
            Node node;
            Shape shape;

            shapeDict = new Dictionary<int, Shape>();
            nodeDict = new Dictionary<int, Node>();

            // Slide the text into a memory stream for the XMLReader
            memoryStream = new MemoryStream();
            streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
            streamWriter.Write(inputOSM);
            streamWriter.Flush();
            memoryStream.Position = 0;

            try
            {
                xpDoc = new XPathDocument(memoryStream);
                xpNav = xpDoc.CreateNavigator();

                // Iterate through the node definitions
                nodeIterator = xpNav.Select("/osm/node");

                while (nodeIterator.MoveNext())
                {
                    try
                    {
                        node = new Node();
                        node.ID = Int32.Parse(nodeIterator.Current.GetAttribute("id", ""));
                        node.Latitude = Decimal.Parse(nodeIterator.Current.GetAttribute("lat", ""));
                        node.Longitude = Decimal.Parse(nodeIterator.Current.GetAttribute("lon", ""));

                        nodeDict.Add(node.ID, node);
                    }
                    catch (FormatException formatException)
                    {
                        throw new ConversionException("Error parsing lat/lon: " + nodeIterator.Current.ToString(), formatException);
                    }
                }

                // Iterate through the ways
                wayIterator = xpNav.Select("/osm/way");

                while (wayIterator.MoveNext())
                {
                    shape = new Shape();

                    shape.ID = Int32.Parse(wayIterator.Current.GetAttribute("id", ""));                    

                    // Determine the name/label
                    nameIterator = wayIterator.Current.Select("tag[@k='ref']");

                    if (nameIterator.Count > 0)
                    {
                        nameIterator.MoveNext();
                        nameValue = nameIterator.Current.GetAttribute("v", "");
                        shape.Name = nameValue;
                    }
                    else
                    {
                        shape.Name = "";
                    }

                    // Process the waypoints
                    wayPointIterator = wayIterator.Current.Select("nd");
                    while (wayPointIterator.MoveNext())
                    {
                        waypointIdValue = Int32.Parse(wayPointIterator.Current.GetAttribute("ref", ""));

                        node = nodeDict[waypointIdValue];
                        shape.Nodes.Add(node);
                    }

                    shapeDict.Add(shape.ID, shape);
                }
            }
            catch (XmlException xmlException)
            {
                throw new ConversionException("XML Exception occurred. See InnerException for details.", xmlException);
            }
            catch (ArgumentException argException)
            {
                throw new ConversionException("Argument Exception occurred. See InnerException for details.", argException);
            }
        }

        private string buildSCT(IEnumerable<Shape> shapes)
        {
            StringBuilder outputBuilder;
            Shape lastShape;

            lastShape = null;
            outputBuilder = new StringBuilder();

            foreach (Shape shape in shapes)
            {
                // Write the name comment (if applicable)
                if (!String.IsNullOrEmpty(shape.Name))
                    outputBuilder.AppendLine(";- " + shape.Name);
       
                // First and last node are the same, treat as area
                Node node;
                for (int n = 0; n < shape.Nodes.Count - 1; n++)
                {
                    node = shape.Nodes[n];
                    outputBuilder.AppendFormat("  {0} {1}" + Environment.NewLine, Utilities.LatitudeDecimalToDMS(node.Latitude), Utilities.LongitudeDecimalToDMS(node.Longitude));
                }

                outputBuilder.AppendLine();

                lastShape = shape;
            }

            return outputBuilder.ToString();
        }

    }
}
