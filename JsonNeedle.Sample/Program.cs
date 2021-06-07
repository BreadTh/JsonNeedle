using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonNeedle.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var from = "John";
            var to = "Alice";
            
            Console.WriteLine("Generating data...");
            var data = GenerateData();

            Console.WriteLine("Connecting data...");
            var haystack = new Haystack();
            var connectionEnds = haystack.FindConnection(data, from, to, (candidate) => 
                Guid.TryParse(candidate, out Guid _) && //We know that all relations are guids. This avoids relating Alice and John by their age.
                candidate != "7b3fa199-57fe-4617-a806-b1b7a1a061b1" //At a previous run we saw the algorithm transverse via the HR person's ID, but not actually through the HR person. We don't want that.
            );

            Console.WriteLine("Printing data...");
            haystack.PrintFirst(connectionEnds, to);
        }

        static List<NodeSet> GenerateData()
        {
            var employees = new List<object>()
            {
                new { reference = "115f08e4-65cf-4490-ab79-16bf0b8b8784", person = new { names = new List<object>(){"John", "Doe"}, age = 32 } },
                new { id = "a67c5fa9-0300-47e8-b61f-092de322e1e3", name = "Jane Doe" },
                new { id = "d6f3a810-53c4-4be6-af31-e56ad4be18d3", person = new { names = new List<object>(){"Alice", "Alison"}, extra = "Is great.", age = 32 } },
                new { id = "3b143085-f880-4310-9e74-c59e3a5dd563", name = "Bob" },
            };

            var employments = new List<object>()
            {
                new { employeeId = "115f08e4-65cf-4490-ab79-16bf0b8b8784", job = "4cd0cc2e-fa73-4ec9-82d9-2f853ec2211b"},
                new { employeeId = "a67c5fa9-0300-47e8-b61f-092de322e1e3", jobId = "7b3fa199-57fe-4617-a806-b1b7a1a061b1"},
                new { employerId = "d6f3a810-53c4-4be6-af31-e56ad4be18d3", roleId = "8d85a9f7-e2e5-446e-b918-a3b68aa79f4e"},
                new { employee = "3b143085-f880-4310-9e74-c59e3a5dd563", jobId = "136f8ebe-e402-40e4-b9c9-cf6b987586da"},
            };

            var jobs = new List<object>() 
            {
                new { id = "4cd0cc2e-fa73-4ec9-82d9-2f853ec2211b", role = "Coder", reportsTo = new { projects = "136f8ebe-e402-40e4-b9c9-cf6b987586da", hr = "7b3fa199-57fe-4617-a806-b1b7a1a061b1" }},
                new { id = "7b3fa199-57fe-4617-a806-b1b7a1a061b1", role = "HR" },
                new { id = "8d85a9f7-e2e5-446e-b918-a3b68aa79f4e", role = "Product Owner" },
                new { id = "136f8ebe-e402-40e4-b9c9-cf6b987586da", role = "Lead Developer", reportsTo = new List<object>() {"7b3fa199-57fe-4617-a806-b1b7a1a061b1", "8d85a9f7-e2e5-446e-b918-a3b68aa79f4e" } },
            };

            var result = new List<NodeSet>();

            var employeeNodSet = CreateData("employee", employees);
            var jobNodeSet = CreateData("job", jobs);
            var employmentNodeSet = CreateData("employment", employments);


            //just to have a lot of nodes.
            for(int i = 100_000; i > 0; i--) 
            {
                result.Add(employeeNodSet);
                result.Add(jobNodeSet);
                result.Add(employmentNodeSet);
            }

            return result;

            NodeSet CreateData(string description, List<object> items) =>
                new NodeSet(description, JArray.Parse(JsonConvert.SerializeObject(items)).Children().Select(x => (JObject)x).ToList());
        }
    }
}
