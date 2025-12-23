

namespace ATCGame.Core;


public enum PlaneState
{
    PARKED,
    PUSHBACK,
    TAXI_OUT,
    ONRUNWAY,
    TAKEOFF,
    DEPARTURE,
    OUTBOUND,
    ENROUTE,
    INBOUND,
    ARRIVAL,
    LANDING,
    TAXI_TO_GATE
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
    RAMP,
    GROUND,
    TOWER,
    DEPARTURE,
    OVERFLIGHT,
    ARRIVAL
}