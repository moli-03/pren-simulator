using System.Collections.Generic;
using UnityEngine;

public class LineSensorBoard : MonoBehaviour {

	public IRSensor FrontSensor;

	public List<IRSensor> HorizontalSensors = new List<IRSensor>();
	public List<IRSensor> VerticalSensors = new List<IRSensor>();

	[HideInInspector]
	public readonly int HorizontalSensorCount = 3;

	[HideInInspector]
	public readonly int VerticalSensorCount = 3;

	[HideInInspector]
	public readonly float VerticalSensorDistance = 0.035f;

	[HideInInspector]
	public readonly float HorizontalSensorDistance = 0.035f;
	
	[HideInInspector]
	public readonly float FrontSensorDistance = 0.14f;

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

		// Create the front sensor
		this.FrontSensor = CreateIRSensorGameObject(new Vector3(0, 0, this.FrontSensorDistance));

		// Create the middle sensor
		IRSensor middleSensor = CreateIRSensorGameObject(Vector3.zero);

		// Create vertical sensors
		int verticalMiddleIndex = Mathf.CeilToInt((float)this.VerticalSensorCount / 2);
		for (int i = -(verticalMiddleIndex - 1); i <= verticalMiddleIndex - 1; i++) {

			// Skip the middle one
			if (i == verticalMiddleIndex) {
				this.VerticalSensors.Add(middleSensor);
				continue;
			}

			float offset = i * this.VerticalSensorDistance;
			this.VerticalSensors.Add(CreateIRSensorGameObject(new Vector3(0, 0, offset)));
		}

		// Create horizontal sensors
		int horizontalMiddleIndex = Mathf.CeilToInt((float)this.HorizontalSensorCount / 2);
		for (int i = -(horizontalMiddleIndex - 1); i <= horizontalMiddleIndex - 1; i++) {

			// Skip the middle one
			if (i == horizontalMiddleIndex) {
				this.HorizontalSensors.Add(middleSensor);
				continue;
			}

			float offset = i * this.HorizontalSensorDistance;
			this.HorizontalSensors.Add(CreateIRSensorGameObject(new Vector3(offset, 0, 0)));
		}

		this.ShowDebugLines();
	}


	void ShowDebugLines() {

		this.FrontSensor.DrawDebugLine();

		this.HorizontalSensors.ForEach(sensor => sensor.DrawDebugLine());
		this.VerticalSensors.ForEach(sensor => sensor.DrawDebugLine());
	}


}