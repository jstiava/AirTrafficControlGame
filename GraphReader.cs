using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ATCGame.Core;

public class GraphReader
{
    public List<Junction> _nodes;
    public List<AirfieldRoadSegment> _roads;

    public List<Junction> GetNodes()
    {
        return _nodes;
    }

    public List<AirfieldRoadSegment> GetEdges()
    {
        return _roads;
    }

    public GraphReader()
    {

    }


    public async Task ReadAsync(string filepath)
    {
        string jsonContent = await File.ReadAllTextAsync("../../../" + filepath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var graph = JsonSerializer.Deserialize<GraphData>(jsonContent, options);

        this._nodes = graph.Nodes.Select(raw => new Junction(raw)).ToList();
        List<AirfieldRoadSegment> roads = graph.Edges.Select<EdgeRaw, AirfieldRoadSegment>(raw =>
        {
            if (raw.Is_Runway)
            {
                return new Runway(raw);
            }
            return new Taxiway(raw);
        }).ToList();

        this._roads = roads;
    }
}


public class GraphData
{
    [JsonPropertyName("nodes")]
    public List<NodeRaw> Nodes { get; set; } = new List<NodeRaw>();

    [JsonPropertyName("edges")]
    public List<EdgeRaw> Edges { get; set; } = new List<EdgeRaw>();
}

public class NodeRaw
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("x")]
    public float X { get; set; }
    [JsonPropertyName("y")]
    public float Y { get; set; }
    [JsonPropertyName("valence")]
    public int Valence { get; set; }
    [JsonPropertyName("edges")]
    public IReadOnlyList<EdgeRaw> Edges { get; set; }
    [JsonPropertyName("crossing_runway")]
    public bool Crossing_Runway { get; set; }
    [JsonPropertyName("is_gate")]
    public bool Is_Gate { get; set; }

    public NodeRaw() { }
}

public class EdgeRaw
{
    public int Id { get; set; }
    public string Path { get; set; }
    public bool Is_Runway { get; set; }
    public double? Heading { get; set; }
    public int? From { get; set; }
    public int? To { get; set; }
    public string? Type { get; set; }

    public List<List<float>>? Points { get; set; }

    public EdgeRaw() { }
}