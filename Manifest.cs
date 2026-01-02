using System;
using ATCGame.Core;
using CsvHelper.Configuration.Attributes;

namespace ATCGame.Core;



public class Manifest
{
    public int _numberOfAircraft = 1;
    public bool _isHeavy = false;
    public string _computerId;

    public Airport _departingAirport;
    public Airport _arrivalAirport;

    public Manifest()
    {
        _numberOfAircraft = 1;
        _isHeavy = false;
        _computerId = null;
        _departingAirport = null;
        _arrivalAirport = null;
    }
}