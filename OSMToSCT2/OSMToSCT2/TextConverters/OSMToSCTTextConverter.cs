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
    public class OSMToSCTTextConverter
    {
        private String mColorTagRunway;
        private String mColorTagTaxiway;
        private String mColorTagApron;
        private String mColorTagTerminal;
        private String mColorTagHangar;
        private String mColorTagOther1;

        public OSMToSCTTextConverter(String colorTagRunway,
                                     String colorTagTaxiway,
                                     String colorTagApron,
                                     String colorTagTerminal,
                                     String colorTagHangar,
                                     String colorTagOther1)
        {
            mColorTagRunway = colorTagRunway;
            mColorTagTaxiway = colorTagTaxiway;
            mColorTagApron = colorTagApron;
            mColorTagTerminal = colorTagTerminal;
            mColorTagHangar = colorTagHangar;
            mColorTagOther1 = colorTagOther1;
        }

        public string Convert(string inputOSM)
        {
            string outputSCT;
            Dictionary<int, Shape> shapeDict;
            Dictionary<int, Node> nodeDict;

            ParseOSM(inputOSM, out shapeDict, out nodeDict);

            outputSCT = BuildSCT(shapeDict.Values.ToList());

            return outputSCT;
        }

        /// <summary>
        /// Parses the OSM data and creates a dictionary of Nodes and a dictionary of Shapes (ways)
        /// </summary>
        /// <param name="inputOSM">OSM data</param>
        /// <param name="shapeDict">out Dictionary of Shapes</param>
        /// <param name="nodeDict">out Dictionary of Nodes</param>
        private void ParseOSM(string inputOSM, out Dictionary<int, Shape> shapeDict, out Dictionary<int, Node> nodeDict)
        {
            XPathDocument xpDoc;
            XPathNavigator xpNav;
            XPathNodeIterator nodeIterator;
            XPathNodeIterator wayIterator;
            XPathNodeIterator aerowayIterator;
            XPathNodeIterator nameIterator;
            XPathNodeIterator wayPointIterator;
            string aerowayValue;
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

                    // Determine the aeroway type
                    aerowayIterator = wayIterator.Current.Select("tag[@k='aeroway']");

                    if (aerowayIterator.Count > 0)
                    {
                        aerowayIterator.MoveNext();
                        aerowayValue = aerowayIterator.Current.GetAttribute("v", "");

                        switch (aerowayValue)
                        {
                            case "hangar":
                                shape.Type = Shape.ShapeType.Hangar;
                                break;
                            case "terminal":
                                shape.Type = Shape.ShapeType.Terminal;
                                break;
                            case "apron":
                                shape.Type = Shape.ShapeType.Apron;
                                break;
                            case "taxiway":
                                shape.Type = Shape.ShapeType.Taxiway;
                                break;
                            case "runway":
                                shape.Type = Shape.ShapeType.Runway;
                                break;
                            default:
                                shape.Type = Shape.ShapeType.Other1;
                                break;
                        }
                    }
                    else
                    {
                        shape.Type = Shape.ShapeType.Other1;
                    }

                    // Determine the name/label
                    nameIterator = wayIterator.Current.Select("tag[@k='ref']");
                    if (nameIterator.Count == 0)
                    {
                        nameIterator = wayIterator.Current.Select("tag[@k='name']");
                    }

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

        /// <summary>
        /// Builds SCT text from a collection of Shapes
        /// </summary>
        /// <param name="shapes">Shape collection</param>
        /// <returns>SCT data</returns>
        private string BuildSCT(IEnumerable<Shape> shapes)
        {
            StringBuilder outputBuilder;

            outputBuilder = new StringBuilder();

            foreach (Shape shape in shapes)
            {
                // Write the name comment (if applicable)
                if (!String.IsNullOrEmpty(shape.Name))
                    outputBuilder.AppendLine(";- " + shape.Name);

                // Write the color tag
                switch (shape.Type)
                {
                    case Shape.ShapeType.Apron:
                        outputBuilder.Append(mColorTagApron);
                        break;
                    case Shape.ShapeType.Hangar:
                        outputBuilder.Append(mColorTagHangar);
                        break;
                    case Shape.ShapeType.Other1:
                        outputBuilder.Append(mColorTagOther1);
                        break;
                    case Shape.ShapeType.Runway:
                        outputBuilder.Append(mColorTagRunway);
                        break;
                    case Shape.ShapeType.Taxiway:
                        outputBuilder.Append(mColorTagTaxiway);
                        break;
                    case Shape.ShapeType.Terminal:
                        outputBuilder.Append(mColorTagTerminal);
                        break;
                }

                // Write each node      
                if (shape.Nodes[shape.Nodes.Count - 1] == shape.Nodes[0])
                {
                    // First and last node are the same, treat as area
                    Node node;
                    for (int n = 0; n < shape.Nodes.Count - 1; n++)
                    {
                        node = shape.Nodes[n];
                        outputBuilder.AppendFormat("  {0} {1}" + Environment.NewLine, LatitudeDecimalToDMS(node.Latitude), LongitudeDecimalToDMS(node.Longitude));
                    }
                }
                else
                {
                    // First and last nodes are different, treat as path
                    // TODO: render path as an area
                    Node node;
                    for (int n = 0; n < shape.Nodes.Count; n++)
                    {
                        node = shape.Nodes[n];
                        outputBuilder.AppendFormat(";  {0} {1}" + Environment.NewLine, LatitudeDecimalToDMS(node.Latitude), LongitudeDecimalToDMS(node.Longitude));
                    }
                }


                outputBuilder.AppendLine();
            }

            return outputBuilder.ToString();
        }

        /// <summary>
        /// Formats a decimal latitude value in the SCT DMS format
        /// </summary>
        /// <param name="latitudeDecimal">Decimal Latitude value</param>
        /// <returns>SCT-formatted latitude value (ex. N039.13.25.089)</returns>
        protected static String LatitudeDecimalToDMS(decimal latitudeDecimal)
        {
            String latitudeDMS;
            decimal latitudeM;
            decimal latitudeS;
            decimal latitudeSRemainder;

            latitudeDMS = "";

            if (latitudeDecimal >= 0)
            {
                latitudeDMS += "N";
            }
            else
            {
                latitudeDecimal = -latitudeDecimal;
                latitudeDMS += "S";
            }

            latitudeM = (latitudeDecimal - Math.Floor(latitudeDecimal)) * 60;
            latitudeS = Math.Round((latitudeM - Math.Floor(latitudeM)) * 60, 3);
            latitudeSRemainder = Math.Round((latitudeS - Math.Floor(latitudeS)) * 1000);

            latitudeDMS += String.Format("{0:000}.{1:00}.{2:00}.{3:000}",
                                         (int)latitudeDecimal,
                                         (int)latitudeM,
                                         (int)latitudeS,
                                         (int)latitudeSRemainder);

            return latitudeDMS;
        }

        /// <summary>
        /// Formats a decimal longitude value in the SCT DMS format
        /// </summary>
        /// <param name="longitudeDecimal">Decimal Longitude value</param>
        /// <returns>SCT-formatted longitude value (ex. W106.52.00.570)</returns>
        protected static String LongitudeDecimalToDMS(decimal longitudeDecimal)
        {
            String longitudeDMS;
            decimal longitudeM;
            decimal longitudeS;
            decimal longitudeSRemainder;

            longitudeDMS = "";

            if (longitudeDecimal >= 0)
            {
                longitudeDMS += "E";
            }
            else
            {
                longitudeDecimal = -longitudeDecimal;
                longitudeDMS += "W";
            }

            longitudeM = (longitudeDecimal - Math.Floor(longitudeDecimal)) * 60;
            longitudeS = Math.Round((longitudeM - Math.Floor(longitudeM)) * 60, 3);
            longitudeSRemainder = Math.Round((longitudeS - Math.Floor(longitudeS)) * 1000);


            longitudeDMS += String.Format("{0:000}.{1:00}.{2:00}.{3:000}",
                                         (int)longitudeDecimal,
                                         (int)longitudeM,
                                         (int)longitudeS,
                                         (int)longitudeSRemainder);

            return longitudeDMS;
        }
    }
}
