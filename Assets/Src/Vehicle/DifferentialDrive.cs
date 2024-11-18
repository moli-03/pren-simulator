using System;
using Assets.Src.Util;
using UnityEngine;

public class DifferentialDrive : MonoBehaviour
{
	public readonly float MaxRpm = 400;
	public float LeftWheelRpm { get; private set; } = 0f;
	public float RightWheelRpm { get; private set; } = 0f;
	public GameObject LeftWheel;
	public GameObject RightWheel;
	public float WheelDistance;
	public readonly float WheelRadius = 0.03f;

	public Vector3 InitialWorldPosition;
	public float InitialWorldRotation;
	public Vector3 CalculatedWorldForward => Pathing.Vec2ToVec3(new Vector2(Mathf.Sin(this.CalculatedWorldOrientation), Mathf.Cos(this.CalculatedWorldOrientation))).normalized;
	public Vector3 CalculatedWorldPosition => Pathing.Vec2ToVec3(this.Position) + this.InitialWorldPosition;
	public Vector2 Forward => new Vector2(Mathf.Sin(this.Orientation), Mathf.Cos(this.Orientation)).normalized;
	public Vector2 Position { get; private set; } = Vector2.zero;
	public float Orientation { get; private set; } = 0;
	public float CalculatedWorldOrientation => NormalizeAngle(this.InitialWorldRotation + this.Orientation);

	private delegate void OnUpdateCallback();
	private OnUpdateCallback OnUpdate = null;

	private bool DebugMode = true;
	private GameObject PositionIndicator;
	private LineRenderer PositionIndicatorLine;
	private float PositionIndicatorHeight = 0.8f;
	private float PositionIndicatorLength = 0.3f;

	void Start() {
		this.WheelDistance = (this.LeftWheel.transform.position - this.RightWheel.transform.position).magnitude;

		this.InitialWorldPosition = this.transform.position;
		this.InitialWorldRotation = this.transform.eulerAngles.y * Mathf.Deg2Rad;

		if (this.DebugMode) {
			this.PositionIndicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
			this.PositionIndicator.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
			this.PositionIndicatorLine = this.PositionIndicator.AddComponent<LineRenderer>();
			this.PositionIndicatorLine.receiveShadows = false;
			this.PositionIndicatorLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			this.PositionIndicatorLine.useWorldSpace = true;
        	this.PositionIndicatorLine.startWidth = 0.008f;
        	this.PositionIndicatorLine.endWidth = 0.008f;
        	this.PositionIndicatorLine.material = new Material(Shader.Find("Sprites/Default"));
        	this.PositionIndicatorLine.startColor = Color.cyan;
        	this.PositionIndicatorLine.endColor = Color.cyan;
		}
	}

	public void SetLeftWheelSpeedPercent(float percent) {
		this.LeftWheelRpm = this.MaxRpm * Math.Clamp(percent, -1, 1);
	}

	public void SetRightWheelSpeedPercent(float percent) {
		this.RightWheelRpm = this.MaxRpm * Math.Clamp(percent, -1, 1);
	}

	public void SetLeftWheelRpm(float rpm) {
		this.LeftWheelRpm = Math.Clamp(rpm, -this.MaxRpm, this.MaxRpm);
	}

	public void SetRightWheelRpm(float rpm) {
		this.RightWheelRpm = Math.Clamp(rpm, -this.MaxRpm, this.MaxRpm);
	}

	public void DriveForwardPercent(float percent) {
		this.SetLeftWheelSpeedPercent(percent);
		this.SetRightWheelSpeedPercent(percent);
	}

	public void TurnRightOnSpot(float percent) {
		this.SetRightWheelSpeedPercent(-percent);
		this.SetLeftWheelSpeedPercent(percent);
	}

	public void TurnLeftOnSpot(float percent) {
		this.SetLeftWheelSpeedPercent(-percent);
		this.SetRightWheelSpeedPercent(percent);
	}

	public void Stop() {
		this.SetLeftWheelSpeedPercent(0);
		this.SetRightWheelSpeedPercent(0);
	}

	public void TurnDeg(float deg, Action done) {
    	// Convert degrees to radians
    	float rad = deg * Mathf.Deg2Rad;
    	float previousRotation = this.Orientation;

    	// Set wheel speeds for turning
    	float turnSpeed = 0.35f;

    	if (deg > 0) {
        	this.TurnRightOnSpot(turnSpeed);  // Turn right if positive degree
    	} else {
        	this.TurnLeftOnSpot(turnSpeed);   // Turn left if negative degree
    	}

    	// Track the total rotation accumulated
    	float rotationAccumulated = 0f;

    	// Define the OnUpdate callback to check rotation progress
    	this.OnUpdate = () => {
        	// Calculate the difference between current and initial rotation
        	float deltaRotation = Mathf.DeltaAngle(Mathf.Rad2Deg * previousRotation, Mathf.Rad2Deg * this.Orientation);
        	rotationAccumulated += Mathf.Abs(deltaRotation);  // Accumulate the rotation
			previousRotation = this.Orientation;


        	// If we've rotated more than the required amount, stop the vehicle
        	if (rotationAccumulated >= Mathf.Abs(deg)) {
            	this.Stop();  // Stop the rotation
            	this.OnUpdate = null;  // Unsubscribe the update callback
            	done.Invoke();  // Call the callback once the turn is complete
        	}
    	};
	}

	// Normalize angle to be within the range [0, 2π] (or [0, 360°] in degrees)
	private float NormalizeAngle(float angle) {
		angle = Mathf.Repeat(angle, 2 * Mathf.PI);  // Ensure angle is within [0, 2π]
		if (angle < 0) {
			angle += 2 * Mathf.PI;  // Wrap negative angles around
		}
		return angle;
	}



	public void RotateFacing(Vector2 direction, Action done) {

		// Maybe minus here
		float deg = -Vector2.SignedAngle(this.Forward, direction);

		// Check if we dont have to turn at all
		if (deg == 0) {
			done.Invoke();
			return;
		}

		this.TurnDeg(deg, done);
	}



	/// <summary>
	/// Returns the current velocity in m/s
	/// </summary>
	private float GetVelocityMps(float rpm) {
		float wheelCircumference = 2 * Mathf.PI * this.WheelRadius;
		return wheelCircumference * rpm / 60f;
	}

	/// <summary>
	/// Returns the forward velocity by the current rpm of the wheels in meters per second
	/// </summary>
	/// <returns></returns>
	private float GetForwardLinearVelocityMps() {
		return (this.GetVelocityMps(this.LeftWheelRpm) + this.GetVelocityMps(this.RightWheelRpm)) / 2;
	}

	private float GetAngularVelocityMps() {
		return (this.GetVelocityMps(this.RightWheelRpm) - this.GetVelocityMps(this.LeftWheelRpm)) / this.WheelDistance;
	}


    void FixedUpdate()
    {
    	float v = this.GetForwardLinearVelocityMps();
    	float w = -this.GetAngularVelocityMps();

    	// Update orientation with the angular velocity
    	float deltaOrientation = w * Time.deltaTime;
		// Debug.Log("W: " + w + ", delta: " + deltaOrientation);
    	this.Orientation = NormalizeAngle(this.Orientation + deltaOrientation);

    	// Update position
    	this.Position += this.Forward * v * Time.deltaTime;

    	// Update position and rotation of vehicle
    	this.transform.position = this.CalculatedWorldPosition;
    	this.transform.rotation = Quaternion.Euler(0, this.CalculatedWorldOrientation * Mathf.Rad2Deg, 0);

    	// Handle debug mode
    	if (this.DebugMode) {
        	this.PositionIndicator.transform.position = new Vector3(this.CalculatedWorldPosition.x, this.PositionIndicatorHeight, this.CalculatedWorldPosition.z);
        	this.PositionIndicatorLine.SetPosition(0, this.PositionIndicator.transform.position);
        	this.PositionIndicatorLine.SetPosition(1, this.PositionIndicator.transform.position + this.CalculatedWorldForward * this.PositionIndicatorLength);
    	}

    	// Invoke current handler
    	this.OnUpdate?.Invoke();
    }
}
