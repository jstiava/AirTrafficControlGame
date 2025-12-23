using ATCGame.Entities;
using System.Collections.Generic;
using System.Numerics;

namespace ATCGame.Core;


public class ATCUnit
{
    public string _name;
    public string _code;
    public Vector2 _position;

    public ATCUnit(string name, string code)
    {
        _name = name; 
        _code = code;
    }

}

public class Airport : ATCUnit
{
    public Runway[] _runways = [];

    public Airport(string name, string code) : base(name, code)
    {
        
    }
}


public enum RunwayMode
{
    ARRIVALS_ONLY,
    DEPARTURES_ONLY,
    DUAL_USE
}

public enum RunwayActivity
{
    CROSSING,
    LINE_UP_AND_WAIT,
    LANDING,
    FREE
}

public class RunwayManager
{
    public string name;
    public RunwayMode _mode;
    public RunwayActivity _activity = RunwayActivity.FREE;
    public bool _hasILS = false;
    public PriorityQueue<FlightProgress, int> _outbound;
    public PriorityQueue<FlightProgress, int> _inbound;
    public FlightProgress _occupant;

    public bool IsOpen()
    {
        return _activity == RunwayActivity.FREE;
    }

    bool LineUpAndWait()
    {
        if (!IsOpen()) { return false; }
        this._activity = RunwayActivity.LINE_UP_AND_WAIT;
        FlightProgress nextOutbound = _outbound.Dequeue();
        this._occupant = nextOutbound;

        return true;

    }

    bool ClearedToLand()
    {
        if (!IsOpen()) { return false; }
        this._activity = RunwayActivity.LANDING;

        return true;
    }

    bool LineUpAndWait(FlightProgress aircraft)
    {
        if (!IsOpen()) { return false; }
        this._activity = RunwayActivity.LINE_UP_AND_WAIT;
        this._occupant = aircraft;

        return true;
    }

    bool Crossing(FlightProgress aircraft)
    {
        if (!IsOpen()) { return false; }
        this._activity = RunwayActivity.CROSSING;
        this._occupant = aircraft;

        return true;
    }

    bool Release()
    {
        this._activity = RunwayActivity.FREE;

        return true;
    }

}