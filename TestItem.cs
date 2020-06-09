using Newtonsoft.Json;

public class TestItem
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("someProperty")]
    public int SomeProperty { get; set; }
    [JsonProperty("source")]
    public string Source { get; set; }
}