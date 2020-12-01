# Raptor OCU
Graphical User Interface for Raptor system. Developed on 2019.4.6f1 Unity Editor.

## Script Hierarchy
Hierarchy of the c sharp files
```
Scripts
├── Extensions
│   ├── CustomWebRequest.cs		//Web handler for video feed
│   ├── SerializableTypes.cs	//Serializable helper for data structures for save/load
│   └── Singleton.cs			//Unity Monobehaviour singleton base
├── Controllable				//contains scene unit inheritables
│   ├── Unit.cs					//Base parent class for all units on scene
│   ├── Payload.cs				//Payload platform unit, child of Unit
│   ├── Beacon.cs				//Beacon platform unit, child of Unit
│   ├── PlannerUnit.cs			//Planner unit used in planner mode, child of Unit
│   └── PayloadDisplayItem.cs	//Payload UI display, referenced in Payload.cs
├── RosConnector				//contains additional RosSharp definition
│   ├── Actions					//contains generated topic handlers from RosSharp for MoveBase
│   │   ├── MoveBaseActionFeedback.cs
│   │   ├── MoveBaseActionGoal.cs
│   │   ├── MoveBaseActionResult.cs
│   │   ├── MoveBaseFeedback.cs
│   │   ├── MoveBaseGoal.cs
│   │   └── MoveBaseResult.cs
│   ├── Messages				//contains additional message data structures for GPS data
│   │   ├── NatSatFix.cs
│   │   └── NavSatStatus.cs
│   ├── RaptorConnector.cs		//Modified from RosConnector.cs, optimized for Unity and Raptor usage
│   └── MoveBaseActionClient.cs	//Handler for MoveBaseAction
├── OcuManager.cs				//Overall scene manager. Handles unit initialization, in scene display
├── UiManager.cs				//Unity Canvas manager. Handles GUI states.
├── OcuLogger.cs				//Logger for logger display
├── OcuLogItem.cs				//Dynamic list entry for logger display
├── CameraPan.cs				//Handles camera pan movement and grid drawing
├── Compass.cs					//To show geographic North
├── RaptorPlanData.cs			//Data class for saving/loading formation plan files
├── WorldScaler.cs				//Static class for default scale sizes
└── PlayerPrefsConstants.cs		//Constant definition for Unity's PlayerPref variables
```

#### Note to Developers
The runtime order starts from RaptorConnector->OcuManager/UiManager. Everything else starts up independently or is setup after.


## Build modes
Project can be runned on 2 different modes which can be changed at pre-compilation at EventSystem->RaptorConnector:
- Production- app will connect to designated ROS bridge ip address
- UiTest- app will run without connection
