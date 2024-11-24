using System.Collections.Generic;
using System.Linq;
using Assets.Src.Util;
using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States {

	public class MoveToCenter : VehicleState
	{
		public override string Name => "MoveToCenter";

		private bool FinishedTurning = false;
		private Vector2 AccumulatedAdjustmentDirection = Vector2.zero;
		private Vector2? PositionBeforeAdjustment = null;
		private bool TurnedToAdjustmentDirection = false;
		private bool StartedTurningToAdjustmentDirection = false;

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
			this.PositionBeforeAdjustment = null;
			this.TurnedToAdjustmentDirection = false;

			this.Vehicle.Drive.TurnDeg(90f, () => {
				this.FinishedTurning = true;
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
			if (this.FinishedTurning && this.AccumulatedAdjustmentDirection != Vector2.zero) {
				this.HandleAdjustment();
			}

			// Handle moving the 90 deg
			else if (!this.FinishedTurning) {
				this.HandleCenterChecking();
			}

			// Turned the 90 deg and we were always on the node, yeyyy, nothing to do :)
			else {
				this.NextState();
			}

		}


		private void HandleAdjustment() {

			if (!this.StartedTurningToAdjustmentDirection) {
				this.TurnedToAdjustmentDirection = false;
				this.PositionBeforeAdjustment = this.Vehicle.Position;

				// Rotate to that direction
				this.Vehicle.Drive.RotateFacing(this.AccumulatedAdjustmentDirection, () => {
					this.TurnedToAdjustmentDirection = true;

					// Start moving forward slowly
					this.Vehicle.Drive.DriveForwardPercent(0.1f);
				});
			}

			// Wait until we are fully turned to direction we want to correct to
			if (!this.TurnedToAdjustmentDirection) {
				return;
			}

			// Move forward until all sensors are on the line
			if (this.AdjustmentDirections.All(adjustmentDirection => Pathing.IsOnLine(adjustmentDirection.sensor))) {

				// Pretty much at the center now
				this.Vehicle.Drive.Stop();

				this.NextState();
				return;
			}

		}

		private void HandleCenterChecking() {

			// Accumulate the direction we have to drive toward to get to the center
			foreach (AdjustmentDirection adjustment in this.AdjustmentDirections) {
				if (!Pathing.IsOnLine(adjustment.sensor)) {
					this.AccumulatedAdjustmentDirection += Pathing.ApplyVehicleRotation(adjustment.adjustmentDirection);
				}
			}

		}
	}

}