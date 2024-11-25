using System;
using Assets.Src.Vehicle.States.Barrier;
using UnityEngine;

public class ClawController : MonoBehaviour
{
    
	private GameObject Arm;

	private enum Actions {
		LOWER,
		GRAB,
		LIFT,
		RELEASE
	}

	private float armMoveDistance = 0.1f; // m
	private float armMoveSpeed = 0.06f; // m/s

	private Actions? action = null;
	private Action callback = null;


	private GameObject pickedUpBarrier = null;
	private DateTime? startedGrabbingAt = null;
	private TimeSpan grabbingTime = TimeSpan.FromSeconds(2);
	private DateTime? startedReleasingAt = null;
	private TimeSpan releasingTime = TimeSpan.FromSeconds(2);


	void Start() {
		this.Arm = this.transform.Find("Arm").gameObject;
	}

	public void LowerArm(Action done) {
		this.action = Actions.LOWER;
		this.callback = done;
	}

	public void GrabBarrier(GameObject barrier, Action done) {
		this.pickedUpBarrier = barrier;
		this.startedGrabbingAt = DateTime.Now;
		this.action = Actions.GRAB;
		this.callback = done;
	}

	public void LiftArm(Action done) {
		this.action = Actions.LIFT;
		this.callback = done;
	}

	public void ReleaseBarrier(Action done) {
		this.startedReleasingAt = DateTime.Now;
		this.action = Actions.RELEASE;
		this.callback = done;
	}


	private void HandleLoweringArm() {

		// Stop when reaching the arm move distance
		if (Mathf.Abs(this.Arm.transform.localPosition.y) >= this.armMoveDistance) {
			this.action = null;
			this.callback?.Invoke();
			return;
		}

		// Move the arm further down
		Vector3 position = this.Arm.transform.localPosition;
		position = new Vector3(position.x, position.y - this.armMoveSpeed * Time.deltaTime, position.z);
		this.Arm.transform.localPosition = position;
	}


	private void HandleGrabbing() {

		// Wait the grabbing time
		if (DateTime.Now - this.startedGrabbingAt.Value >= this.grabbingTime) {

			// Set it as a child of the arm so it moves along the car
			this.pickedUpBarrier.transform.SetParent(this.Arm.transform);

			this.action = null;
			this.callback?.Invoke();
			return;
		}

	}


	private void HandleLiftingArm() {
		
		// Stop when reaching the original position
		if (this.Arm.transform.localPosition.y >= 0f) {
			this.Arm.transform.localPosition = new Vector3(this.Arm.transform.localPosition.x, 0f, this.Arm.transform.localPosition.z);
			this.action = null;
			this.callback?.Invoke();
			return;
		}

		// Move arm up
		Vector3 position = this.Arm.transform.localPosition;
		position = new Vector3(position.x, position.y + this.armMoveSpeed * Time.deltaTime, position.z);
		this.Arm.transform.localPosition = position;
	}


	private void HandleReleasing() {

		// Wait the releasing time
		if (DateTime.Now - this.startedReleasingAt.Value >= this.releasingTime) {

			// Release the barrier again
			this.pickedUpBarrier.transform.SetParent(null);

			this.action = null;
			this.callback?.Invoke();
			return;
		}
	}

	void Update() {

		if (this.action == null) {
			return;
		}

		if (this.action == Actions.LOWER) {
			this.HandleLoweringArm();
		}
		else if (this.action == Actions.GRAB) {
			this.HandleGrabbing();
		}
		else if (this.action == Actions.LIFT) {
			this.HandleLiftingArm();
		}
		else if (this.action == Actions.RELEASE) {
			this.HandleReleasing();
		}
	}

}
