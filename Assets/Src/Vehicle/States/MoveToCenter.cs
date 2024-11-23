using System.Collections.Generic;
using System.Linq;
using Assets.Src.Util;
using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States {

	public class MoveToCenter : VehicleState
	{
		public override string Name => "MoveToCenter";

		private bool TurnedMinDistance = false;
		private Vector2? CurrentAdjustmentDirection = null;
		private Vector2? PositionBeforeAdjustment = null;
		private bool TurnedToAdjustmentDirection = false;
		private float MaxAdjustmentDirection = 0.05f;

		struct AdjustmentDirection {
			public IRSensor sensor;
			public Vector2 adjustmentDirection;
		}

		private List<AdjustmentDirection> AdjustmentDirections = new List<AdjustmentDirection>();

		public MoveToCenter(VehicleController vehicle) : base(vehicle)
		{
			// Horizontal
			this.AdjustmentDirections.Add(new AdjustmentDirection() { sensor = this.Vehicle.SensorBoard.HorizontalSensors[0], adjustmentDirection = new Vector2(1, 0) });
			this.AdjustmentDirections.Add(new AdjustmentDirection() { sensor = this.Vehicle.SensorBoard.HorizontalSensors[1], adjustmentDirection = new Vector2(-1, 0) });

			// Vertical
			this.AdjustmentDirections.Add(new AdjustmentDirection() { sensor = this.Vehicle.SensorBoard.VerticalSensors[0], adjustmentDirection = new Vector2(0, -1) });
			this.AdjustmentDirections.Add(new AdjustmentDirection() { sensor = this.Vehicle.SensorBoard.VerticalSensors[1], adjustmentDirection = new Vector2(0, 1) });

			// Diagonal
			this.AdjustmentDirections.Add(new AdjustmentDirection() { sensor = this.Vehicle.SensorBoard.DiagonalSensors[0], adjustmentDirection = new Vector2(-1, -1) });
			this.AdjustmentDirections.Add(new AdjustmentDirection() { sensor = this.Vehicle.SensorBoard.DiagonalSensors[1], adjustmentDirection = new Vector2(-1, 1) });
			this.AdjustmentDirections.Add(new AdjustmentDirection() { sensor = this.Vehicle.SensorBoard.DiagonalSensors[2], adjustmentDirection = new Vector2(1, 1) });
			this.AdjustmentDirections.Add(new AdjustmentDirection() { sensor = this.Vehicle.SensorBoard.DiagonalSensors[3], adjustmentDirection = new Vector2(1, -1) });


			// Start
			this.StartSearchingForCenter();
		}


		private void StartSearchingForCenter() {

			// Reset everything
			this.CurrentAdjustmentDirection = null;
			this.PositionBeforeAdjustment = null;
			this.TurnedToAdjustmentDirection = false;
			this.TurnedMinDistance = false;

			// Turn 90 deg and check if at any point we were not on the line with any sensor
			this.Vehicle.Drive.TurnDeg(90f, () => {
				this.TurnedMinDistance = true;
			});
		}


		private void NextState() {

			// Check if we have already found a node on that position
			MapNode node = this.Vehicle.Map.GetNodeAt(this.Vehicle.Position);

			if (node != null) {

				// Add the already visited node to the history
				this.Vehicle.NodeHistory.Add(node);
			}
			else {

				// Add the new node to the map
				this.Vehicle.StoreNode(this.Vehicle.Position);
			}

			this.Vehicle.SetState(new FindPathsOfNode(this.Vehicle));
		}


		public override void Update()
		{

			// Handle moving to the middle of the node if a ir sensor is off the node
			if (this.CurrentAdjustmentDirection.HasValue) {
				this.HandleAdjustment();
			}

			// Handle moving the 90 deg
			else if (!this.TurnedMinDistance) {
				this.HandleCenterChecking();
			}

			// Turned the 90 deg and we were always on the node :)
			else {
				this.NextState();
			}

		}


		private void HandleAdjustment() {

			// Wait until we are fully turned to direction we want to correct to
			if (!this.TurnedToAdjustmentDirection) {
				return;
			}

			// Check if we have already moved for the max adjustment distance
			if ((this.Vehicle.Position - this.PositionBeforeAdjustment.Value).magnitude >= this.MaxAdjustmentDirection) {
				this.CurrentAdjustmentDirection = null;
				this.PositionBeforeAdjustment = null;
				this.Vehicle.Drive.Stop();

				// Do the same again
				this.StartSearchingForCenter();
				return;
			}

			// Move forward until all sensors are on the line
			if (this.AdjustmentDirections.All(adjustmentDirection => Pathing.IsOnLine(adjustmentDirection.sensor))) {

				// Pretty much at the center now
				this.CurrentAdjustmentDirection = null;
				this.PositionBeforeAdjustment = null;
				this.Vehicle.Drive.Stop();

				this.NextState();
				return;
			}

		}

		private void HandleCenterChecking() {

			Vector2 addedDirection = Vector2.zero;

			foreach (AdjustmentDirection adjustment in this.AdjustmentDirections) {
				if (!Pathing.IsOnLine(adjustment.sensor)) {
					addedDirection += adjustment.adjustmentDirection;
				}
			}

			// No Adjustment needed?
			if (addedDirection == Vector2.zero) {
				return;
			}

			// Adjustment needed
			this.Vehicle.Drive.StopAndCancelAction();

			// Calculate the direction we have to face
			this.TurnedToAdjustmentDirection = false;
			this.CurrentAdjustmentDirection = Pathing.ApplyVehicleRotation(addedDirection);
			this.PositionBeforeAdjustment = this.Vehicle.Position;

			// Rotate to that direction
			this.Vehicle.Drive.RotateFacing(this.CurrentAdjustmentDirection.Value, () => {
				this.TurnedToAdjustmentDirection = true;

				// Start moving forward slowly
				this.Vehicle.Drive.DriveForwardPercent(0.1f);
			});
		}
	}

}