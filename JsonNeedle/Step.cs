namespace JsonNeedle
{
    public record Step(Node node, string matchingValue, Step parent = null);
}
