using Newtonsoft.Json.Linq;

namespace JsonNeedle
{
    public record Node(JObject item, NodeSet set);
}
