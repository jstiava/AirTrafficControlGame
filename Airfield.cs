using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ATCGame.Core;

public class Airfield
{
    public Dictionary<string, Gate> Gates;

    public List<Junction> Junctions;
    public List<AirfieldRoadSegment> Roads;
    public Dictionary<string, Runway> Runways;
    public List<Obstruction> Obstructions;

    private void init(List<Junction> junctions, List<AirfieldRoadSegment> roads, List<Obstruction> obstructions)
    {
        this.Gates = new Dictionary<string, Gate>();
        this.Junctions = junctions;
        this.Roads = new List<AirfieldRoadSegment>();
        this.Runways = new Dictionary<string, Runway>();
        this.Obstructions = obstructions;

        foreach (var route in roads)
        {
            if (route.Is_Runway)
            {
                Runway theRunway = new Runway(route);
                this.Roads.Add(route);
                if (this.Runways.ContainsKey(route.Path))
                {
                    continue;
                }
                this.Runways.Add(theRunway.Path, theRunway);
                continue;
            }

            Taxiway theTaxiway = new Taxiway(route);
            this.Roads.Add(theTaxiway);
        }
    }

    public bool RegisterGate(string name, Gate newGate, string? taxiway, string? owner, int? order)
    {
        if (taxiway != null)
        {
            newGate.PushbackToTaxiway = taxiway;
        }
        if (owner != null)
        {
            newGate.Owner = owner;
        }
        if (order != null)
        {
            newGate.Priority = (int)order;
        }
        this.Gates[name] = newGate;
        return true;
    }

    public Airfield(List<Junction> junctions, List<AirfieldRoadSegment> roads, List<Obstruction> obstructions)
    {
        this.init(junctions, roads, obstructions);
    }

    public Airfield(GraphReader graphReader)
    {
        this.init(graphReader.GetNodes(), graphReader._roads, graphReader._obstructions);
    }
}

public class Gate
{
    public GateStatus Status = GateStatus.OPEN;
    public string Id { get; set; }
    public string Type { get; set; }
    public float At_Gate_Heading { get; set; }

    public Vector2 Position { get; set; }
    public Vector2 ScreenPosition { get; set; }

    public string Owner { get; set; }

    public string PushbackToTaxiway { get; set; }

    public int Priority {  get; set; }

    public Gate(GateRaw raw)
    {
        Id = raw.Id;
        At_Gate_Heading = raw.At_Gate_Heading;
        Position = new Vector2(raw.X, raw.Y);
    }

    public Gate(Gate old)
    {
        Id = old.Id;
        At_Gate_Heading = old.At_Gate_Heading;
        Position = old.Position;
        Owner = Owner;

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
    public Vector2 ScreenPosition { get; set; }

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
        ScreenPosition = Position;
    }
}

public class Taxiway : AirfieldRoadSegment
{

    public Taxiway(EdgeRaw raw) : base(raw)
    {

    }

    public Taxiway(AirfieldRoadSegment raw) : base(raw)
    {

    }
}

public class Runway : AirfieldRoadSegment
{

    public Runway(EdgeRaw raw) : base(raw)
    {

    }

    public Runway(AirfieldRoadSegment raw) : base(raw)
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

    public AirfieldRoadSegment(AirfieldRoadSegment other)
    {
        Id = other.Id;
        Path = other.Path;
        Is_Runway = other.Is_Runway;
        Heading = other.Heading;
        From = other.From;
        To = other.To;
        this.Points = new List<Vector2>(other.Points);
        this.ScreenPoints = new List<Vector2>(other.Points);
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