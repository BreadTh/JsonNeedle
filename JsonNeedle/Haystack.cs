using Newtonsoft.Json.Linq;
using Spectre.Console;
using System;
using System.Collections.Generic;

namespace JsonNeedle
{
    public class Haystack
    {
        public List<Step> FindConnection(List<NodeSet> nodeSets, string from, string to) 
        {
            var unconnectedNodes = new Queue<Node>();
            var edges = new Queue<Step>();

            foreach(var set in nodeSets)
                foreach(var item in set.items) 
                {
                    bool isStartingPoint = false;
                    var node = new Node(item, set);

                    FindFromValue(item);

                    if(!isStartingPoint)
                        unconnectedNodes.Enqueue(node);

                    void FindFromValue(JToken space) 
                    {
                        if(space.Type == JTokenType.Object) 
                            foreach(var property in ((JObject)space).Properties())
                                FindFromValue(property.Value);

                        else if(space.Type == JTokenType.Array)
                            foreach(var child in ((JArray)space).Children())
                                FindFromValue(child);

                        else if(space.ToString() == from) 
                        {
                            edges.Enqueue(new Step(node, from));
                            isStartingPoint = true;
                        }
                    }
                }

            var results = new List<Step>();

            while(edges.Count != 0)
            {
                var edgeLinks = new Dictionary<string, Step>();
                
                foreach(var edge in edges) 
                {
                    foreach(var edgeProperty in edge.node.item.Properties())
                    {

                        void FindEdgeLinksAndFinalResult(JToken edgeSpace)
                        {
                            if(edgeSpace.Type == JTokenType.Object)
                                foreach(var property in ((JObject)edgeSpace).Properties())
                                    FindEdgeLinksAndFinalResult(property.Value);
                            
                            else if(edgeSpace.Type == JTokenType.Array) 
                                foreach(var child in ((JArray)edgeSpace).Children())
                                    FindEdgeLinksAndFinalResult(child);
                            else  
                            {
                                if(edgeSpace.ToString() == to)
                                    results.Add(edge);
                                else
                                    edgeLinks[edgeSpace.ToString()] = edge;
                            }
                        }
                        FindEdgeLinksAndFinalResult(edgeProperty.Value);
                    }
                }
                var newEdges = new Queue<Step>();
                var newUnconnectedNodes = new Queue<Node>();
                        
                foreach(var unconnectedNode in unconnectedNodes)
                {
                    var connectionFound = FindConnectionToUnconnected(unconnectedNode.item);

                    if(!connectionFound)
                        newUnconnectedNodes.Enqueue(unconnectedNode);

                    bool FindConnectionToUnconnected(JToken unconnectedSpace)
                    {
                        var connectionFound = false;
                        if(unconnectedSpace.Type == JTokenType.Object)
                            foreach(var unconnectedProperty in ((JObject)unconnectedSpace).Properties()) 
                            {
                                var found = FindConnectionToUnconnected(unconnectedProperty.Value);
                                connectionFound = connectionFound || found;
                            }

                        else if(unconnectedSpace.Type == JTokenType.Array)
                            foreach(var child in ((JArray)unconnectedSpace).Children()) 
                            {
                                var found = FindConnectionToUnconnected(child);
                                connectionFound = connectionFound || found;
                            }
                        else
                        {
                            if(edgeLinks.ContainsKey(unconnectedSpace.ToString())) 
                            {
                                newEdges.Enqueue(new Step(unconnectedNode, unconnectedSpace.ToString(), edgeLinks[unconnectedSpace.ToString()]));
                                return connectionFound = true;
                            }
                        }
                        return connectionFound;
                    }
                }
                unconnectedNodes = newUnconnectedNodes;

                edges = newEdges;
            }
            return results;
        }

        public void PrintFirst(List<Step> connectionEnds, string to)
        {
            if(connectionEnds.Count == 0)
                Console.WriteLine("No connection found");
            else 
            {
                Console.WriteLine("Shortest connection found: ");
                PrintConnection(connectionEnds[0], to, 1);
            }

            Console.ReadLine();

            void PrintConnection(Step step, string previousValue, int colorNumber)
            {
                if(step.parent is not null) 
                    PrintConnection(step.parent, step.matchingValue, colorNumber+1);
                
                var header = step.node.set.name;
                
                Console.WriteLine(header);
                Console.WriteLine(new string('=', header.Length));
                Console.WriteLine();
                AnsiConsole.Markup(Highlight(step.matchingValue, colorNumber, Highlight(previousValue, colorNumber-1, step.node.item.ToString().Replace("[", "[[").Replace("]", "]]") )));
                Console.WriteLine();
                Console.WriteLine();

                string Highlight(string target, int colorNumber, string space) 
                {
                    var colors = new List<string>() {"red", "green", "blue", "yellow", "fuchsia", "aqua"};
                    var color = colors[colorNumber%colors.Count];
                    var result = space.Replace(target, $"[underline {color.ToString().ToLower()}]{target}[/]");
                    return result;
                }
            }
        }
    }
}
