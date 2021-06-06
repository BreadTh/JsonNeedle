using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace JsonNeedle
{
    public record NodeSet(string name, List<JObject> items);
}
