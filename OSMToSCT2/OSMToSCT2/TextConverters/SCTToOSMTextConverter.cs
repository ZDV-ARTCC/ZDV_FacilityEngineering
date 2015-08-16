using OSMToSCT2.Geo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace OSMToSCT2.TextConverters
{
    /// <summary>
    /// Utility class for converting SCT data to OSM data
    /// </summary>
    public class SCTToOSMTextConverter
    {
        private String mColorTagRunway;
        private String mColorTagTaxiway;
        private String mColorTagApron;
        private String mColorTagTerminal;
        private String mColorTagHangar;
        private String mColorTagOther1;

        public SCTToOSMTextConverter(String colorTagRunway,
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

        /// <summary>
        /// Converts the SCT data in inputSCT to OSM data
        /// </summary>
        /// <param name="inputSCT">SCT data</param>
        /// <returns>OSM data</returns>
        public String Convert(String inputSCT)
        {
            StringBuilder outputBuilder;
            String[] inputLines;
            Regex coordRegex;
            Match coordMatch;
            String trimmedLine;
            String colorTag;
            String label;
            Dictionary<int, Shape> shapes;
            Dictionary<int, Node> nodes;
            Shape currentShape;
            Node currentNode;
            int lastId;

            // ex. N039.13.42.473 W106.52.13.792
            coordRegex = new Regex(@"([NS]\d{3}.\d{2}.\d{2}.\d{3,})\s*([WE]\d{3}.\d{2}.\d{2}.\d{3,})");

            lastId = -1;
            label = "";

            currentShape = null;

            outputBuilder = new StringBuilder();
            shapes = new Dictionary<int, Shape>();
            nodes = new Dictionary<int, Node>();

            // Split input into individual lines
            inputLines = inputSCT.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in inputLines)
            {
                trimmedLine = line.Trim();

                // Line is blank, skip
                if (String.IsNullOrWhiteSpace(line))
                {
                    continue;
                }


                if (trimmedLine[0] == ';')
                {
                    if (trimmedLine.Length > 3 && trimmedLine.Substring(0, 3) == ";- ")
                    {
                        // Line is a label
                        label = trimmedLine.Substring(3);
                    }
                    else
                    {
                        // Line is comment, skip
                        continue;
                    }
                }

                coordMatch = coordRegex.Match(trimmedLine);

                // Check that a coordinate line was found
                if (!coordMatch.Success)
                    continue;

                if (coordMatch.Index > 0) // Indicates that there is may be a color tag
                {
                    colorTag = trimmedLine.Substring(0, coordMatch.Index).TrimEnd();

                    // Create a new shape
                    currentShape = new Shape();
                    currentShape.ID = lastId--;
                    currentShape.Name = label;

                    if (colorTag == mColorTagRunway)
                        currentShape.Type = Shape.ShapeType.Runway;
                    else if (colorTag == mColorTagTaxiway)
                        currentShape.Type = Shape.ShapeType.Taxiway;
                    else if (colorTag == mColorTagApron)
                        currentShape.Type = Shape.ShapeType.Apron;
                    else if (colorTag == mColorTagTerminal)
                        currentShape.Type = Shape.ShapeType.Terminal;
                    else if (colorTag == mColorTagHangar)
                        currentShape.Type = Shape.ShapeType.Hangar;
                    else if (colorTag == mColorTagOther1)
                        currentShape.Type = Shape.ShapeType.Other1;
                    else
                        currentShape.Type = Shape.ShapeType.Other1;

                    shapes.Add(currentShape.ID, currentShape);
                }

                // Create a dummy shape if none exists
                if (currentShape == null)
                {
                    currentShape = new Shape();
                    currentShape.ID = lastId--;
                    currentShape.Name = label;
                    currentShape.Type = Shape.ShapeType.Other1;
                }

                // Create a new node
                currentNode = new Node();
                currentNode.ID = lastId--;
                currentNode.Latitude = DecimalLatitudeFromString(coordMatch.Groups[1].Value);
                currentNode.Longitude = DecimalLongitudeFromString(coordMatch.Groups[2].Value);

                nodes.Add(currentNode.ID, currentNode);
                currentShape.Nodes.Add(currentNode);
            }

            
            // Return results
            return GenerateOSM(shapes.Values.ToList(), nodes.Values.ToList());
        }

        /// <summary>
        /// Parses an SCT-formatted Latitude string to a Decimal
        /// </summary>
        /// <param name="strLatitude">SCT-formatted Latitude string</param>
        /// <returns>Decimal Latitude</returns>
        private decimal DecimalLatitudeFromString(String strLatitude)
        {
            decimal decLatitude;
            decimal decSeconds;
            decimal decMinutes;
            decimal sign;
            string[] parts;

            if (strLatitude[0] == 'N')
            {
                sign = 1;
            }
            else
            {
                sign = -1;
            }

            parts = strLatitude.Substring(1).Split('.');

            decSeconds = Decimal.Parse(parts[2] + "." + parts[3]) / 3600;
            decMinutes = Decimal.Parse(parts[1]) / 60;
            decLatitude = sign * (Decimal.Parse(parts[0]) + decMinutes + decSeconds);

            return decLatitude;
        }
        
        /// <summary>
        /// Parses an SCT-formatted Longitude string to a Decimal
        /// </summary>
        /// <param name="strLongitude">SCT-formatted Longitude string</param>
        /// <returns>Decimal Longitude</returns>
        private decimal DecimalLongitudeFromString(String strLongitude)
        {
            decimal decLongitude;
            decimal decSeconds;
            decimal decMinutes;
            decimal sign;
            string[] parts;

            if (strLongitude[0] == 'E')
            {
                sign = 1;
            }
            else
            {
                sign = -1;
            }

            parts = strLongitude.Substring(1).Split('.');

            decSeconds = Decimal.Parse(parts[2] + "." + parts[3]) / 3600;
            decMinutes = Decimal.Parse(parts[1]) / 60;
            decLongitude = sign * (Decimal.Parse(parts[0]) + decMinutes + decSeconds);

            return decLongitude;
        }

        /// <summary>
        /// Generates OSM text from collection of Shapes and Nodes
        /// </summary>
        /// <param name="shapes">Shapes (ways)</param>
        /// <param name="nodes">Nodes</param>
        /// <returns>OSM data (XML)</returns>
        private string GenerateOSM(IEnumerable<Shape> shapes, IEnumerable<Node> nodes)
        {
            //StringBuilder outputBuilder;
            XmlWriter writer;
            XmlWriterSettings settings;
            string outputText;
            MemoryStream memoryStream;

            memoryStream = new MemoryStream();

            settings = new XmlWriterSettings()
            {
                ConformanceLevel = ConformanceLevel.Document,
                Encoding = new UTF8Encoding(false),
                Indent = true,
                IndentChars = "  ",
                OmitXmlDeclaration = false,
                WriteEndDocumentOnClose = true
            };

            writer = XmlWriter.Create(memoryStream, settings);

            // Start OSM
            writer.WriteStartElement("osm");
            writer.WriteAttributeString("version", "0.6");
            writer.WriteAttributeString("generator", "OSMToSCT2");

            // Nodes
            foreach (Node node in nodes)
            {
                writer.WriteStartElement("node");
                writer.WriteAttributeString("id", node.ID.ToString());
                writer.WriteAttributeString("action", "modify");
                writer.WriteAttributeString("visible", "true");
                writer.WriteAttributeString("lat", node.Latitude.ToString("##0.00000000000"));
                writer.WriteAttributeString("lon", node.Longitude.ToString("##0.00000000000"));
                writer.WriteEndElement();
            }

            // Ways
            foreach (Shape shape in shapes)
            {
                writer.WriteStartElement("way");
                writer.WriteAttributeString("id", shape.ID.ToString());
                writer.WriteAttributeString("action", "modify");
                writer.WriteAttributeString("visible", "true");

                // Add nodes
                foreach (Node node in shape.Nodes)
                {
                    writer.WriteStartElement("nd");
                    writer.WriteAttributeString("ref", node.ID.ToString());
                    writer.WriteEndElement();
                }

                // Add the first node again to close the shape
                if (shape.Nodes.Count() > 1)
                {      
                    writer.WriteStartElement("nd");
                    writer.WriteAttributeString("ref", shape.Nodes.First().ID.ToString());
                    writer.WriteEndElement();
                }

                switch (shape.Type)
                {
                    case Shape.ShapeType.Apron:
                        writer.WriteStartElement("tag");
                        writer.WriteAttributeString("k", "aeroway");
                        writer.WriteAttributeString("v", "apron");
                        writer.WriteEndElement();

                        if (shape.Name != "")
                        {
                            writer.WriteStartElement("tag");
                            writer.WriteAttributeString("k", "ref");
                            writer.WriteAttributeString("v", shape.Name);
                            writer.WriteEndElement();
                        }
                        break;
                    case Shape.ShapeType.Hangar:
                        writer.WriteStartElement("tag");
                        writer.WriteAttributeString("k", "aeroway");
                        writer.WriteAttributeString("v", "hangar");
                        writer.WriteStartElement("tag");
                        writer.WriteAttributeString("k", "building");
                        writer.WriteAttributeString("v", "hangar");
                        writer.WriteEndElement();

                        if (shape.Name != "")
                        {
                            writer.WriteStartElement("tag");
                            writer.WriteAttributeString("k", "ref");
                            writer.WriteAttributeString("v", shape.Name);
                            writer.WriteEndElement();
                        }
                        break;
                    case Shape.ShapeType.Runway:
                        writer.WriteStartElement("tag");
                        writer.WriteAttributeString("k", "aeroway");
                        writer.WriteAttributeString("v", "runway");
                        writer.WriteEndElement();

                        if (shape.Name != "")
                        {
                            writer.WriteStartElement("tag");
                            writer.WriteAttributeString("k", "ref");
                            writer.WriteAttributeString("v", shape.Name);
                            writer.WriteEndElement();
                        }
                        break;
                    case Shape.ShapeType.Taxiway:
                        writer.WriteStartElement("tag");
                        writer.WriteAttributeString("k", "aeroway");
                        writer.WriteAttributeString("v", "taxiway");
                        writer.WriteEndElement();

                        if (shape.Name != "")
                        {
                            writer.WriteStartElement("tag");
                            writer.WriteAttributeString("k", "ref");
                            writer.WriteAttributeString("v", shape.Name);
                            writer.WriteEndElement();
                        }
                        break;
                    case Shape.ShapeType.Terminal:
                        writer.WriteStartElement("tag");
                        writer.WriteAttributeString("k", "aeroway");
                        writer.WriteAttributeString("v", "terminal");
                        writer.WriteEndElement();

                        if (shape.Name != "")
                        {
                            writer.WriteStartElement("tag");
                            writer.WriteAttributeString("k", "name");
                            writer.WriteAttributeString("v", shape.Name);
                            writer.WriteEndElement();
                        }
                        break;
                    default:
                        if (shape.Name != "")
                        {
                            writer.WriteStartElement("tag");
                            writer.WriteAttributeString("k", "ref");
                            writer.WriteAttributeString("v", shape.Name);
                            writer.WriteEndElement();
                        }
                        break;
                }

                writer.WriteEndElement();
            }

            // End OSM
            writer.WriteEndElement();

            writer.Flush();

            outputText = Encoding.UTF8.GetString(memoryStream.ToArray());
            
            return outputText;
        }
    }
}
