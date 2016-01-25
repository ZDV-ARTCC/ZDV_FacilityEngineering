using OSMToSCT2.Geo;
using OSMToSCT2.Helpers;
using System;
using System.Collections;
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
    /// Utility class for converting SCT Video Map data to OSM data
    /// </summary>
    public class SCTToOSMVideoMapConverter
    {
        public SCTToOSMVideoMapConverter()
        { }

        public String Convert(String inputSCT)
        {
            String[] inputLines;
            Regex lineRegex;
            Match lineMatch;
            //Dictionary<int, Node> nodes;
            Dictionary<int, Shape> shapes;
            Dictionary<Tuple<Decimal, Decimal>, Node> nodes;
            Dictionary<int, Node> nodesById;
            String trimmedLine;
            String label;
            Node node1;
            Node node2;
            Node foundNode;
            Shape currentShape;

            int lastId;

            lastId = -1;
            label = "";
            nodes = new Dictionary<Tuple<decimal, decimal>, Node>();            
            shapes = new Dictionary<int, Shape>();

            currentShape = null;

            // ex. N039.13.42.473 W106.52.13.792 N048.98.18.189 W056.48.63.228
            lineRegex = new Regex(@"([NS]\d{3}.\d{2}.\d{2}.\d{3,})\s*([WE]\d{3}.\d{2}.\d{2}.\d{3,})\s*([NS]\d{3}.\d{2}.\d{2}.\d{3,})\s*([WE]\d{3}.\d{2}.\d{2}.\d{3,})");

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
                    // Line is comment, skip
                    continue;
                }

                lineMatch = lineRegex.Match(trimmedLine);

                // Check that a coordinate line was found
                if (!lineMatch.Success)
                    continue;

                if (lineMatch.Index > 0) // Indicates that there is a label
                {
                    label = trimmedLine.Substring(0, lineMatch.Index).TrimEnd();
                }

                // Parse Node 1
                node1 = new Node();
                node1.ID = lastId--;
                node1.Latitude = Utilities.DecimalLatitudeFromString(lineMatch.Groups[1].Value);
                node1.Longitude = Utilities.DecimalLongitudeFromString(lineMatch.Groups[2].Value);

                foundNode = null;
                nodes.TryGetValue(new Tuple<Decimal, Decimal>(node1.Latitude, node1.Longitude), out foundNode);
                //foundNode = (from nd in nodes.Values
                //             where nd.Latitude == node1.Latitude &&
                //                   nd.Longitude == node1.Longitude
                //             select nd).SingleOrDefault();

                if (foundNode != null)
                {
                    node1 = foundNode;
                    lastId++;
                }
                else
                {
                    nodes.Add(new Tuple<Decimal, Decimal>(node1.Latitude, node1.Longitude), node1);
                }

                // Parse Node 2
                node2 = new Node();
                node2.ID = lastId--;
                node2.Latitude = Utilities.DecimalLatitudeFromString(lineMatch.Groups[3].Value);
                node2.Longitude = Utilities.DecimalLongitudeFromString(lineMatch.Groups[4].Value);

                foundNode = null;
                nodes.TryGetValue(new Tuple<Decimal, Decimal>(node2.Latitude, node2.Longitude), out foundNode);
                //foundNode = (from nd in nodes.Values
                //             where nd.Latitude == node2.Latitude &&
                //                   nd.Longitude == node2.Longitude
                //             select nd).SingleOrDefault();

                if (foundNode != null)
                {
                    node2 = foundNode;
                    lastId++;
                }
                else
                {
                    nodes.Add(new Tuple<Decimal, Decimal>(node2.Latitude, node2.Longitude), node2);
                }

                // Create new shape if none has been created yet
                if (currentShape == null)
                {
                    currentShape = new Shape()
                    {
                        ID = lastId--,
                        Name = label,
                        Type = Shape.ShapeType.VideoMap
                    };

                    shapes.Add(currentShape.ID, currentShape);
                }

                // Check for continuity
                if (currentShape.Nodes.Count == 0) // No nodes in shape, add both
                {
                    currentShape.Nodes.Add(node1);
                    currentShape.Nodes.Add(node2);
                }
                else
                {
                    if (currentShape.Nodes.Last() == node1) // First node of this line matches the last node in shape
                    {
                        currentShape.Nodes.Add(node2);
                    }
                    else if (currentShape.Nodes.Last() == node2) // Second node of this line matches the last node in shape
                    {
                        currentShape.Nodes.Add(node1);
                    }
                    else // Discontinuity found, create new shape
                    {
                        currentShape = new Shape()
                        {
                            ID = lastId--,
                            Name = label,
                            Type = Shape.ShapeType.VideoMap
                        };
                        shapes.Add(currentShape.ID, currentShape);

                        currentShape.Nodes.Add(node1);
                        currentShape.Nodes.Add(node2);
                    }
                }
            }

            return GenerateOSM(shapes.Values.ToList(), nodes.Values.ToList());
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

                writer.WriteStartElement("tag");
                writer.WriteAttributeString("k", "ref");
                writer.WriteAttributeString("v", shape.Name);
                writer.WriteEndElement();

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
