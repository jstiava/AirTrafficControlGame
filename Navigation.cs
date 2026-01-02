using ATCGame.Core;
using ATCGame.Entities;
using Microsoft.Xna.Framework;

namespace ATCGame;


public class NavigationInstruction
{
    public string Message;
    public PilotAction OnCompleteReport;
    public NavigationStatus Status = NavigationStatus.NOT_STARTED;

    public NavigationInstruction(PilotAction onCompleteReport)
    {
        this.OnCompleteReport = onCompleteReport;
    }

    public virtual bool Step(Aircraft target)
    {
        return true;
    }

    public virtual string Readback()
    {
        return $"Roger";
    }

}


public class Clearance : NavigationInstruction
{
    public Clearance() : base(PilotAction.ASK_FOR_PUSHBACK)
    {
        // Ready to copy clearance
        // Cleared to Denver as filed, climb via SID, maintain 5,000 initially, departure freq 120.7, squawk 4521.
    }

    public override bool Step(Aircraft target)
    {
        this.Status = NavigationStatus.DOING;
        target.State = PlaneState.PUSHBACK;
        return true;
    }

    public override string Readback()
    {
        return $"Roger";
    }
}

public class Pushback : NavigationInstruction
{
    public string Face;
    public Pushback() : base(PilotAction.ASK_TO_TAXI)
    {
        // Pushback approved, face west, expect runway 28R for departure. Contact Ground on 121.9 when ready to taxi.
    }

    public override bool Step(Aircraft target)
    {
        this.Status = NavigationStatus.DOING;
        target.State = PlaneState.PUSHBACK;
        return true;
    }

    public override string Readback()
    {
        return $"Pushing back, facing {Face}, contact ground";
    }

}

public class Taxi : NavigationInstruction
{

    public Junction Start;
    public Junction End;
    public NavigationDirection EndNavigationDirection;
    public AirfieldRoadSegment Path;

    public Taxi(Junction start, Junction end, NavigationDirection endNavigationDirection, AirfieldRoadSegment path, PilotAction onCompleteReport) : base(onCompleteReport)
    {
        // Taxi to runway 28R via taxiways C and B, hold short of runway 28R. 
        this.End = end;
        this.Start = start;
        this.Path = new AirfieldRoadSegment(path);
        this.EndNavigationDirection = endNavigationDirection;
    }

    public override string Readback()
    {
        return $"Taxi, (what you said)";
    }
}

public class HoldShort : NavigationInstruction
{
    public HoldShort() : base(PilotAction.STANDBY)
    {

    }

    public override string Readback()
    {
        return $"Hold short";
    }
}


public class LineUpAndWait : NavigationInstruction
{
    public LineUpAndWait() : base(PilotAction.STANDBY)
    {

    }

    public override string Readback()
    {
        return $"Line up and wait, runway something";
    }

}

public class Takeoff : NavigationInstruction
{
    public Takeoff() : base(PilotAction.PROCEED_TO_NEXT)
    {

    }

    public override string Readback()
    {
        return $"Cleared for takeoff";
    }
}

public class FlyHeading : NavigationInstruction
{
    public FlyHeading() : base(PilotAction.PROCEED_TO_NEXT)
    {

    }

    public override string Readback()
    {
        return $"135, maintain 5000";
    }
}

public class ContactGoodday : NavigationInstruction
{
    public ContactGoodday(string atcLevel, float contactFrequency) : base(PilotAction.PROCEED_TO_NEXT)
    {

    }

    public override string Readback()
    {
        return $"Departure 135.6";
    }
}




