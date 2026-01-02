using ATCGame.Core;
using ATCGame.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;


namespace ATCGame;

public class Airport
{
    public string Name;
    public JsonContent Content;
    public Airfield Airfield { get; set; }

    public Dictionary<string, Airline> Airlines;

    public Dictionary<string, AirTrafficController> Controllers;
    public Radar Radar;
    public AirportMeteringDayScheduler Scheduler;

    private Texture2D ArrivalImage;
    private Texture2D DepartureImage;


    public bool AddSchedule(List<FlightPlanCsvRow> flights)
    {
        DateTime targetDate = new DateTime(2025, 12, 30);
        foreach (FlightPlanCsvRow flight in flights)
        {
            if (flight.Scheduled.Date == targetDate.Date)
            {
                Flight theFlight = new Flight(flight);
                if (theFlight == null)
                {
                    continue;
                }
                this.Scheduler.Add(theFlight);
            }
        }
        return true;
    }

    public bool RegisterAirline(Airline airline)
    {
        if (this.Airlines.ContainsKey(airline.name))
        {
            return false;
        }

        PriorityQueue<string, int> airlineGateList = new PriorityQueue<string, int>();
        foreach (var gate in Airfield.Gates)
        {
            if (gate.Value.Owner == airline.name)
            {
                airlineGateList.Enqueue(gate.Value.Id, gate.Value.Priority);
            }
        }

        airline.OpenGates = airlineGateList;
        this.Airlines.Add(airline.name, airline);
        return true;
    }

    public void PingFlightsOnSchedule(DateTime now)
    {
        List<Flight> flightsReadyForAircraftAssignment = this.Scheduler.PingFlightsOnSchedule(now);
        foreach(var flight in flightsReadyForAircraftAssignment)
        {

            flight.AssignAircraft(new Aircraft(
                flight.Callsign,
                flight.Type == TrafficDirection.ARRIVAL ? ArrivalImage : DepartureImage,
                new Vector2(0, 0)
            ));
        }
    }


    public Airport(string name, Dictionary<string, double> controller_freqs, List<Junction> junctions, List<AirfieldRoadSegment> roads, List<Obstruction> obstructions, Texture2D _arrivalImage, Texture2D _departureImage)
    {
        this.Name = name;
        this.Airfield = new Airfield(junctions, roads, obstructions);
        this.Radar = new Radar();
        this.Scheduler = new AirportMeteringDayScheduler();
        this.Airlines = new Dictionary<string, Airline>();

        ArrivalImage = _arrivalImage;
        DepartureImage = _departureImage;

        this.Controllers = new Dictionary<string, AirTrafficController>();
        foreach (var kvp in controller_freqs)
        {
            LevelCycle level = (LevelCycle)Enum.Parse(typeof(LevelCycle), kvp.Key, true);
            this.Controllers.Add(kvp.Value.ToString(), new AirTrafficController($"{this.Name} {kvp.Key}", kvp.Value, level));
        }
    }

    public Airport(string name, Dictionary<string, double> controller_freqs, GraphReader graphReader, Texture2D _arrivalImage, Texture2D _departureImage)
    {
        this.Name = name;
        this.Airfield = new Airfield(graphReader);
        this.Radar = new Radar();
        this.Scheduler = new AirportMeteringDayScheduler();
        this.Airlines = new Dictionary<string, Airline>();

        ArrivalImage = _arrivalImage;
        DepartureImage = _departureImage;

        this.Controllers = new Dictionary<string, AirTrafficController>();
        foreach (var kvp in controller_freqs)
        {
            LevelCycle level = (LevelCycle)Enum.Parse(typeof(LevelCycle), kvp.Key, true);
            this.Controllers.Add(kvp.Value.ToString(), new AirTrafficController($"{this.Name} {kvp.Key}", kvp.Value, level));
        }
    }
}