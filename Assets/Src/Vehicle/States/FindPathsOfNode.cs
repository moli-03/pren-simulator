using System.Collections.Generic;
using System.Linq;
using Assets.Src.Util;
using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States {

	public class FindPathsOfNode : VehicleState
	{
		public override string Name => "FindPathsOfNode";

		private bool StartedOnLine = false;
		private bool StartedOnLineFoundLine = false;
		private bool LinePreviouslyHovered = false;
		private MapNode CurrentNode;
		private List<LineEdgePoints> EdgePoints = new List<LineEdgePoints>();

		struct LineEdgePoints {
			public Vector2 Left;
			public Vector2 Right;
		}


		public FindPathsOfNode(VehicleController vehicle) : base(vehicle) {
			this.CurrentNode = this.Vehicle.NodeStack.Last();
		}

		public override void Start()
		{
			// Check if we have already scanned the outgoing paths for that node
			if (this.CurrentNode.OutgoingPathsScanned) {

				this.Vehicle.SetState(new ChooseNextPath(this.Vehicle));
				return;
			}

			// Check if the front middle sensor is on the line
			if (Pathing.IsOnLine(this.Vehicle.SensorBoard.PathDetectionSensor)) {

				this.StartedOnLine = true;
				// Start slowly rotating left
				this.Vehicle.Drive.TurnLeftOnSpot(0.2f);
			}
			else {
				this.StartScanningForPaths();
			}
		}

		private void AddPaths() {

			// Add outgoing paths
			this.EdgePoints.ForEach(edgePoint => {

				if (edgePoint.Left == null || edgePoint.Right == null) {
					return;
				}

				// Figure out the middle of the line
				Vector2 middle = edgePoint.Left + 0.5f * (edgePoint.Right - edgePoint.Left);

				this.CurrentNode.AddOutgoingPathPosition(middle);

			});

			this.CurrentNode.OutgoingPathsScanned = true;

			this.CurrentNode.UpdatePathMapping();
		}


		private void StartScanningForPaths() {

			this.LinePreviouslyHovered = false;

			this.Vehicle.Drive.TurnDeg(360, () => {

				// Add the found paths to the node
				this.AddPaths();

				// Update minimap
				Minimap.Instance.UpdateMap();

				this.Vehicle.SetState(new ChooseNextPath(this.Vehicle));
			});
		}

		public override void Update()
		{
			if (this.CurrentNode.OutgoingPathsScanned) {
				return;
			}

			bool isFrontSensorOnLine = Pathing.IsOnLine(this.Vehicle.SensorBoard.PathDetectionSensor);

			// Handle rotating until no longer on a line
			if (this.StartedOnLine && !this.StartedOnLineFoundLine) {


				if (isFrontSensorOnLine) {
					return;
				}

				this.StartedOnLineFoundLine = true;

				// Behind line now
				this.StartScanningForPaths();
				return;
			}


			// Check if we are hovering a line
			if (isFrontSensorOnLine) {

				// Were we previously on a line?
				if (this.LinePreviouslyHovered) {
					return;
				}

				this.LinePreviouslyHovered = true;

				// Initially store the position
				this.EdgePoints.Add(new LineEdgePoints() {
					Left = this.Vehicle.Position + this.Vehicle.Forward * this.Vehicle.SensorBoard.PathDetectionSensorDistanceFromCenter
				});
			}
			else {

				// No longer on a line?
				if (this.LinePreviouslyHovered) {

					// Get the last edge point duo we hovered
					var currentEdgePoint = this.EdgePoints.Last();

					// Set the right
					currentEdgePoint.Right = this.Vehicle.Position + this.Vehicle.Forward * this.Vehicle.SensorBoard.PathDetectionSensorDistanceFromCenter;
					this.EdgePoints[this.EdgePoints.Count - 1] = currentEdgePoint;
					this.LinePreviouslyHovered = false;
				}
			}
		}

	}
}