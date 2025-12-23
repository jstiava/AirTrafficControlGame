using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ATCGame.Core;

public class Airfield
{

}

public class Gate
{
    public string Id { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public string Type { get; set; }
    public float At_Gate_Heading { get; set; }

    public Vector2 Position { get; set; }
    public Vector2 ScreenPosition { get; set; }

    public Gate(GateRaw raw)
    {
        Id = raw.Id;
        X = raw.X;
        Y = raw.Y;
        At_Gate_Heading = raw.At_Gate_Heading;
        Position = new Vector2(raw.X, raw.Y);
    }
}

public class Junction
{
    public int Id { get; }
    public float X { get; }
    public float Y { get; }
    public int Valence { get; }
    public List<AirfieldRoadSegment> Edges { get; }
    public bool Crossing_Runway { get; }
    public bool Is_Gate { get; }

    public Vector2 Position { get; }
    public Vector2? ScreenPosition { get; set; }

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

    public Taxiway(EdgeRaw raw) : base(raw)
    {

    }
}

public class Runway : AirfieldRoadSegment
{

    public Runway(EdgeRaw raw) : base(raw)
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

public class Obstruction
{
    public string Id { get; set; }
    public List<Vector2> Points { get; set; } = new List<Vector2>();
    public List<Vector2> ScreenPoints { get; set; } = new List<Vector2>();
    public string Type { get; set; }

    public Obstruction(ObstructionRaw raw)
    {
        Id= raw.Id;
        Type = raw.Type;

        if (raw.Points != null)
        {
            foreach (var p in raw.Points)
            {
                if (p.Count >= 2)
                {
                    this.Points.Add(new Vector2(p[0], p[1]));
                }
                    
            }
        }
    }
}