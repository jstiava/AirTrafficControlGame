using System;
using ATCGame.Core;

namespace ATCGame.Core;


public class FlightProgress
{
    public string _callsign = "UNKNOWN";
    public TrafficDirection _direction;
    public PlaneState _state;
    public int _revision = 0;
    public string _requestOriginator = string.Empty;
    public int _numberOfAircraft = 1;
    public bool _isHeavy = false;
    public string _computerId;
    public int beaconCode = 0;


    public FlightProgress(string callsign, TrafficDirection direction, PlaneState state)
    {
        _callsign = callsign;
        _direction = TrafficDirection.DEPARTURE;
        _state = PlaneState.PARKED;

        // TODO
    }
}


public class FlightArrival : FlightProgress
{
    public string _coordinationFix = string.Empty;
    public DateTime _estimatedTimeOfArrivalToCoordinationFixAt = DateTime.UtcNow;

    public string _runway = string.Empty;
    public bool _isRadarContact {  get; set; } = false;
    public DateTime _landingClearanceIssuedAt;
    public DateTime _touchdownAt;
    public string _runwayExit = string.Empty;
    public bool _isClearedRunway = false;



    public FlightArrival(string callsign, TrafficDirection direction, PlaneState state) : base(callsign, direction, state)
    {
        // TODO
    }

}

public class FlightDeparture : FlightProgress
{
    public DateTime _proposedDepartureAt = DateTime.MinValue;
    public string _departingAirport {  get; set; } = string.Empty;

    public string _runway = string.Empty;
    public bool _holdingShort {  get; set; } = false;
    public bool _isReleased = false;
    public DateTime _lineUpAt;
    public DateTime _takeoffTime;
    public string _departureFrequency;



    public FlightDeparture(string callsign, TrafficDirection direction, PlaneState state) : base(callsign, direction, state)
    {
        // TODO
    }

}