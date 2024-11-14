using System;
using UnityEngine;

public class DifferentialDrive : MonoBehaviour
{
    public float MaxSpeed = 15f;        // Maximum speed
    public float TurnSpeed = 45f;       // Turning speed in degrees per second
	public float LeftWheelSpeedPercent { get; private set; } = 0f;
	public float RightWheelSpeedPercent { get; private set; } = 0f;

	public void SetLeftWheelSpeedPercent(float percent) {
		this.LeftWheelSpeedPercent = Math.Clamp(percent, -1, 1);
	}

	public void SetRightWheelSpeedPercent(float percent) {
		this.RightWheelSpeedPercent = Math.Clamp(percent, -1, 1);
	}

	public void DriveForward(float speed) {
		this.SetLeftWheelSpeedPercent(speed);
		this.SetRightWheelSpeedPercent(speed);
	}

	public void TurnRightOnSpot(float speed) {
		this.SetRightWheelSpeedPercent(-speed);
		this.SetLeftWheelSpeedPercent(speed);
	}

	public void TurnLeftOnSpot(float speed) {
		this.SetLeftWheelSpeedPercent(-speed);
		this.SetRightWheelSpeedPercent(speed);
	}

	public void Stop() {
		this.SetLeftWheelSpeedPercent(0);
		this.SetRightWheelSpeedPercent(0);
	}

    void Update()
    {
        // Calculate the forward speed based on average input from left and right wheels
        float forwardSpeed = (LeftWheelSpeedPercent + RightWheelSpeedPercent) / 2f * MaxSpeed * Time.deltaTime;

        // Calculate rotation based on the difference between left and right inputs
        float turnAmount = (RightWheelSpeedPercent - LeftWheelSpeedPercent) * TurnSpeed * Time.deltaTime;

        // Update rotation angle and position
        this.transform.Rotate(new Vector3(0, turnAmount, 0)); // Adjust rotation angle
        this.transform.Translate(this.transform.rotation * this.transform.forward * forwardSpeed); // Move forward in the new direction
    }
}
