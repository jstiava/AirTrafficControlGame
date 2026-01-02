using ATCGame.Core;
using ATCGame.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ATCGame;


public class AirportMeteringDayScheduler
{
    private PriorityQueue<Flight, DateTime> ActiveFlights;
    private PriorityQueue<Flight, DateTime> ScheduledFlights;

    public bool Add(Flight flight)
    {
        this.ScheduledFlights.Enqueue(flight, flight.Scheduled);
        return true;
    }

    public List<Flight> PingFlightsOnSchedule(DateTime now)
    {

        List<Flight> newFlights = new List<Flight>();
        while (true)
        {
            var target = this.ScheduledFlights.Peek();
            if (target.PingFromColdAndDark(now))
            {
                newFlights.Add(this.ScheduledFlights.Dequeue());
                continue;
            }
            break;
        }

        return newFlights;
    }

    public bool RegisterActiveFlights(List<Flight> newFlights)
    {
        foreach (var flight in newFlights)
        {
            this.ActiveFlights.Enqueue(flight, flight.Scheduled);
        }

        return true;
    }

    public AirportMeteringDayScheduler()
    {
        this.ScheduledFlights = new PriorityQueue<Flight, DateTime>();
        this.ActiveFlights = new PriorityQueue<Flight, DateTime>();
    }
}


public class Flight
{
    public TrafficDirection Type { get; set; }
    public string From { get; set; } = "";
    public string To { get; set; } = "";

    public string Callsign { get; set; } = "";

    public string Number { get; set; } = "";
    public string Company { get; set; } = "";
    public DateTime Scheduled { get; set; }
    public string AircraftName { get; set; } = "";
    private Aircraft Aircraft;

    public Flight(FlightPlanCsvRow other)
    {
        this.Type = other.Type switch
        {
            "Arrival" => TrafficDirection.ARRIVAL,
            "Departure" => TrafficDirection.DEPARTURE,
            _ => throw new ArgumentException($"Unknown flight type: {other.Type}")
        };
        this.Company = other.Company;
        this.Callsign = other.Callsign;
        this.AircraftName = other.Aircraft;
        this.Number = other.Number;
        this.Scheduled = other.Scheduled;
        this.From = other.From;
        this.To = other.To;
    }

    public bool PingFromColdAndDark(DateTime dateTime)
    {
        if (this.Scheduled >= dateTime.AddMinutes(60))
        {
            return true;
        }
        return false; 
    }

    public void AssignAircraft(Aircraft aircraft)
    {
        if (this.Aircraft != null)
        {
            throw new Exception("Flight already assigned a flight.");
        }
        this.Aircraft = aircraft;
    }
}
