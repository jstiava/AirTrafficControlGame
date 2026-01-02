using ATCGame.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATCGame;

public class Radar
{
    public Dictionary<string, Aircraft> Objects;

    public Radar()
    {
        this.Objects = new Dictionary<string, Aircraft>();
    }

    public bool Register(Aircraft aircraft)
    {
        if (this.Objects.ContainsKey(aircraft.Callsign))
        {
            return false;
        }
        this.Objects.Add(aircraft.Callsign, aircraft);
        return true;
    }


}
