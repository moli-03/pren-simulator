using Assets.Src.Util;
using UnityEngine;

namespace Assets.Src.Vehicle {

	public class LineFollower : MonoBehaviour {

		private bool isEnabled = false;
		private bool isAdjusting = false;

		private float rpm = 0;

		private VehicleController Vehicle;

		public void Enable() {
			this.isEnabled = true;
		}

		public void Disable() {
			this.isAdjusting = false;
			this.rpm = 0;
			this.isEnabled = false;
		}

		public void SetSpeed(float percent) {
			this.Vehicle.Drive.DriveForwardPercent(percent);
			this.rpm = this.Vehicle.Drive.LeftWheelRpm;
		}


		void Start() {
			this.Vehicle = this.GetComponent<VehicleController>();
		}

		void Update() {

			if (!this.isEnabled || this.rpm == 0) {
				return;
			}


			// Handle stopping of adjusting
			if (this.isAdjusting) {

				// Check if only the middle sensor is on the line
				if (
					!Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[1]) // Left not
					&& 
					Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[2]) // Middle yes
					&&
					!Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[3]) // Right not
				) {

					// Stop adjusting
					this.Vehicle.Drive.SetLeftWheelRpm(this.rpm);
					this.Vehicle.Drive.SetRightWheelRpm(this.rpm);
					this.isAdjusting = false;
				}

			}

			// Check if the right line follow sensor is on the line
			if (!this.isAdjusting && Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[3])) {

				// If the outer right sensor is on the line its because we hit another line
				if (Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[4])) {
					return;
				}

				this.isAdjusting = true;

				// If the middle sensor is not on the line we have to drastically adjust
				if (!Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[2])) {

					// Only need to adjust drastically
					this.Vehicle.Drive.SetLeftWheelRpm(this.rpm + 150);
					this.Vehicle.Drive.SetRightWheelRpm(this.rpm - 50);
				}
				else {
					// Only need to adjust slightly
					this.Vehicle.Drive.SetLeftWheelRpm(this.rpm + 50);
					this.Vehicle.Drive.SetRightWheelRpm(this.rpm);
				}

			}

			// Check if the left line follow sensor is on the line
			if (!this.isAdjusting && Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[1])) {

				// If the outer left sensor is on the line its because we hit another line
				if (Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[0])) {
					return;
				}

				
				this.isAdjusting = true;

				// If the middle sensor is not on the line we have to drastically adjust
				if (!Pathing.IsOnLine(this.Vehicle.SensorBoard.FrontLineFollowSensors[2])) {

					// Only need to adjust drastically
					this.Vehicle.Drive.SetRightWheelRpm(this.rpm + 150);
					this.Vehicle.Drive.SetLeftWheelRpm(this.rpm - 50);
				}
				else {

					// Only need to adjust slightly
					this.Vehicle.Drive.SetRightWheelRpm(this.rpm + 50);
					this.Vehicle.Drive.SetLeftWheelRpm(this.rpm);
				}

			}

		}

	}

}