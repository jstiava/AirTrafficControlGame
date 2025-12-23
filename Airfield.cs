using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCGame.Core;

public class Airfield
{

}

public class Junction {
    public int Id { get; }
    public float X { get; }
    public float Y { get; }
    public int Valence { get; }
    public List<AirfieldRoadSegment> Edges { get; }
    public bool Crossing_Runway { get; }
    public bool Is_Gate { get; }

    public Vector2 Position { get; }
    public Vector2? ScreenPosition { get; set; }

    public Junction(int id, float x, float y, int valence, IReadOnlyList<EdgeRaw> edges, bool crossing_Runway, bool is_Gate)
    {
        Id = id;
        X = x;
        Y = y;
        Valence = valence;
        Edges = edges.Select(e => new AirfieldRoadSegment(e)).ToList();
        Crossing_Runway = crossing_Runway;
        Is_Gate = is_Gate;
        Position = new Vector2(x, y);
    }

    public Junction(NodeRaw raw)
    {
        Id = raw.Id;
        X = raw.X;
        Y = raw.Y;
        Valence = raw.Valence;
        Edges = (raw.Edges ?? new List<EdgeRaw>())
                .Select(e => new AirfieldRoadSegment(e))
                .ToList();
        Crossing_Runway = raw.Crossing_Runway;
        Is_Gate = raw.Is_Gate;
        Position = new Vector2(raw.X, raw.Y);
    }
}

public class Taxiway : AirfieldRoadSegment
{ 
    public Taxiway(int id, string path, bool is_runway, double? heading, int? from, int? to, List<List<float>>? points, string? type) : base(id, path, is_runway, heading, from, to, points, type)
    {

    }

    public Taxiway(EdgeRaw raw) : base(raw.Id, raw.Path, raw.Is_Runway, raw.Heading, raw.From, raw.To, raw.Points, raw.Type)
    {

    }
}

public class Runway : AirfieldRoadSegment
{
    public Runway(int id, string path, bool is_runway, double? heading, int? from, int? to, List<List<float>>? points, string? type) : base(id, path, is_runway, heading, from, to, points, type)
    {

    }

    public Runway(EdgeRaw raw) : base(raw.Id, raw.Path, raw.Is_Runway, raw.Heading, raw.From, raw.To, raw.Points, raw.Type)
    {

    }
}




public class AirfieldRoadSegment
{
    public int Id { get; }
    public string Path { get; }
    public bool Is_Runway { get; }
    public double? Heading { get; }
    public int? From { get; }
    public int? To { get; }

    public List<Vector2>? Points { get; set; } = new List<Vector2>();

    public List<Vector2>? ScreenPoints { get; set; } = new List<Vector2>();



    public AirfieldRoadSegment(int id, string path, bool is_runway, double? heading, int? from, int? to, List<List<float>>? points, string? type )
    {
        Id = id;
        Path = path;
        Is_Runway = is_runway || (type != null && type == "Runways");
        Heading = heading;
        From = from;
        To = to;

        if (points != null)
        {
            foreach (var p in points)
            {
                if (p.Count >= 2)
                    this.Points.Add(new Vector2(p[0], p[1]));
            }
        }
    }

    public AirfieldRoadSegment(EdgeRaw raw)
    {
        Id = raw.Id;
        Path = raw.Path;
        Is_Runway = raw.Is_Runway || (raw.Type != null && raw.Type == "Runways");
        Heading = raw.Heading;
        From = raw.From;
        To = raw.To;

        if (raw.Points != null)
        {
            foreach (var p in raw.Points)
            {
                if (p.Count >= 2)
                    this.Points.Add(new Vector2(p[0], p[1]));
            }
        }
    }
}