

namespace ATCGame.Core;


public enum PlaneState
{
    COLD_AND_DARK,
    PARKED,
    PUSHBACK,
    TAXI_OUT,
    ONRUNWAY,
    TAKEOFF,
    DEPARTURE,
    CLIMBING,
    MAINTAINING,
    FLYING_HOLD,
    DESCENDING,
    ARRIVAL,
    LANDING,
    TAXI_TO_GATE
}

public enum PilotAction
{
    STANDBY,
    ASK_FOR_CLEARANCE,
    ASK_FOR_PUSHBACK,
    ASK_TO_TAXI,
    READY_FOR_TAKEOFF,
    PROCEED_TO_NEXT,
    REPORT
}

public enum TrafficDirection
{
    ARRIVAL,
    DEPARTURE
}


public enum Heading
{
    N = 0,
    E = 90,
    S = 180,
    W = 270
}

public enum LevelCycle
{
    ATIS,
    CLEARANCE,
    GROUND,
    TOWER,
    DEPARTURE,
    OVERFLIGHT,
    ARRIVAL
}

public enum NavigationDirection
{
    CONTINUE,
    LEFT,
    RIGHT
}

public enum NavigationStatus
{
    NOT_STARTED,
    DOING,
    FAILED,
    COMPLETE
}

public enum GateStatus
{
    OPEN,
    OCCUPIED,
    BOARDING,
    DEBOARDING,
    CLOSED
}