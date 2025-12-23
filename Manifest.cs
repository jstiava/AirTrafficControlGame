using System;
using ATCGame.Core;

namespace ATCGame.Core;

public class Manifest
{
    public string _callsign = "UNKNOWN";
    public PlaneState _state;
    public int _numberOfAircraft = 1;
    public bool _isHeavy = false;
    public string _computerId;

    public Airport _departingAirport;
    public Airport _arrivalAirport;

}