using Assets.Src.Util;
using UnityEngine;

public class LineSensorBoard : MonoBehaviour {

	// Sensor for detecting the outgoing paths of a node
	[HideInInspector]
	public IRSensor PathDetectionSensor { get; private set; }

	[HideInInspector]
	public float PathDetectionSensorDistanceFromCenter { get; private set; } = 0.14f;



	// The sensors used to follow the line
	[HideInInspector]
	public IRSensor[] LineFollowSensors { get; private set; } = new IRSensor[3];

	[HideInInspector]
	public float LineFollowSensorDistanceFromCenter { get; private set; } = 0.06f;

	[HideInInspector]
	public float LineFollowSensorGap { get; private set; } = Constants.PATH_WIDTH * 3 / 4;


	// Horizontal sensors in the middle of the vehicle [top, bottom]
	[HideInInspector]
	public IRSensor[] HorizontalSensors { get; private set; } = new IRSensor[2];

	[HideInInspector]
	public float HorizontalSensorDistanceFromCenter { get; private set; } = Constants.NODE_RADIUS - 0.01f; // The outer one should be 0.5cm


	// Vertical sensors in the middle of the vehicle [left, right]
	[HideInInspector]
	public IRSensor[] VerticalSensors { get; private set; } = new IRSensor[2];

	[HideInInspector]
	public float VerticalSensorDistanceFromCenter { get; private set; } = Constants.NODE_RADIUS - 0.01f; // The outer one should be 0.5cm


	// The sensor in the middle
	[HideInInspector]
	public IRSensor MiddleSensor { get; private set; }


	// Sensors in between the vertical and horizontal sensors (counted clockwise starting front right)
	[HideInInspector]
	public IRSensor[] DiagonalSensors { get; private set; } = new IRSensor[4];
	
	[HideInInspector]
	public float DiagonalSensorFromCenter { get; private set; } = Constants.NODE_RADIUS / 2f;


	private IRSensor CreateIRSensorGameObject(Vector3 position)
	{
    	// Create the new GameObject and add the IRSensor component
    	// GameObject sensor = new GameObject("IRSensor");
		GameObject sensor = GameObject.CreatePrimitive(PrimitiveType.Cube);
		sensor.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		sensor.GetComponent<Renderer>().material.color = Color.black;
    	sensor.AddComponent<IRSensor>();
    	sensor.transform.SetParent(this.transform);
		sensor.transform.localPosition = position;
		sensor.transform.rotation = Quaternion.Euler(90f, 0, 0);
    	return sensor.GetComponent<IRSensor>();
	}



	void Start() {

		this.PathDetectionSensor = CreateIRSensorGameObject(new Vector3(0, 0, this.PathDetectionSensorDistanceFromCenter));

		// Create the front sensors
		this.LineFollowSensors[0] = CreateIRSensorGameObject(new Vector3(-this.LineFollowSensorGap, 0, this.LineFollowSensorDistanceFromCenter));
		this.LineFollowSensors[1] = CreateIRSensorGameObject(new Vector3(0, 0, this.LineFollowSensorDistanceFromCenter));
		this.LineFollowSensors[2] = CreateIRSensorGameObject(new Vector3(this.LineFollowSensorGap, 0, this.LineFollowSensorDistanceFromCenter));

		// Create the middle sensor
		this.MiddleSensor = CreateIRSensorGameObject(Vector3.zero);

		// Create the two vertical sensors
		this.VerticalSensors[0] = CreateIRSensorGameObject(new Vector3(0, 0, this.VerticalSensorDistanceFromCenter));
		this.VerticalSensors[1] = CreateIRSensorGameObject(new Vector3(0, 0, -this.VerticalSensorDistanceFromCenter));

		// Create the two horizontal sensors
		this.HorizontalSensors[0] = CreateIRSensorGameObject(new Vector3(-this.HorizontalSensorDistanceFromCenter, 0, 0));
		this.HorizontalSensors[1] = CreateIRSensorGameObject(new Vector3(this.HorizontalSensorDistanceFromCenter, 0, 0));

		// Create the middle sensors (45 deg)
		float distanceX = Mathf.Sin(Mathf.PI / 4) * this.DiagonalSensorFromCenter;
		float distanceY = Mathf.Cos(Mathf.PI / 4) * this.DiagonalSensorFromCenter;
		this.DiagonalSensors[0] = CreateIRSensorGameObject(new Vector3(distanceX, 0, distanceY));
		this.DiagonalSensors[1] = CreateIRSensorGameObject(new Vector3(distanceX, 0, -distanceY));
		this.DiagonalSensors[2] = CreateIRSensorGameObject(new Vector3(-distanceX, 0, -distanceY));
		this.DiagonalSensors[3] = CreateIRSensorGameObject(new Vector3(-distanceX, 0, distanceY));

		this.ShowDebugLines();
	}


	void ShowDebugLines() {

		foreach (IRSensor sensor in this.LineFollowSensors) {
			sensor.DrawDebugLine();
		}

		foreach (IRSensor sensor in this.HorizontalSensors) {
			sensor.DrawDebugLine();
		}

		foreach (IRSensor sensor in this.VerticalSensors) {
			sensor.DrawDebugLine();
		}
	}


}