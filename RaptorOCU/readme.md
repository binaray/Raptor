# Raptor OCU
Graphical User Interface for Raptor system.

## Todo in order of priority:
- Output and show coordinates
- Update map size from beacon coordinates
- Bearings: by unit vector?

## Input and synchronization
- Synchronize by vector or by position?
- Input next position
- Input vector => check latency of program

## Raptor Action Messages
All topics to be broadcasted to a single channel for now. A mode parameter in the goal will determine which message to listen to.
The goal format as follows:
```
{
	int mode;	//for switch case
	string param;	//json parameter subject to mode
}
```
##### TODO: Determine RosMessageName

Goal 0: Get all unit type, status, positions
Feedback:
Result:
```
{
	int objective;
}
```


Goal 1: Manual movement of unit id
Feedback:
Result:

Goal 2: Get all unit type, status, positions
Feedback:
Result: