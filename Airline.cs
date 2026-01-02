using ATCGame.Core;

namespace ATCGame;

public class Airline
{
    public string name;
    public string fullname;
    public string ICAO;
    public string callsign;

    public PriorityQueue<string, int> OpenGates;
    public HashSet<string> OccupiedGates;


    public string FindOpen()
    {
        var theGate = this.OpenGates.Dequeue();
        this.OccupiedGates.Add(theGate);
        if (theGate == null)
        {
            throw new Exception("No open gate found.");
        }
        return theGate;
    }

    public bool Open(string gateId, int priority)
    {
        if (this.OccupiedGates.Contains(gateId))
        {
            this.OpenGates.Enqueue(gateId, priority);
            return true;
        }
        return false;
    }


    public Airline(string name, string fullname, string IACO, string callsign)
    {
        this.name = name;
        this.fullname = fullname;
        this.ICAO = IACO;
        this.callsign = callsign;
        this.OccupiedGates = new HashSet<string>();
        this.OpenGates = new PriorityQueue<string, int>();
    }

}
