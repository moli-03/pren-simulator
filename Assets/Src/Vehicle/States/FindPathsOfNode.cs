using System.Collections.Generic;
using System.Linq;
using Assets.Src.Util;
using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States
{

	public class FindPathsOfNode : VehicleState
	{
		private bool SensorVertical1WasOnLine = false;
		private bool SensorVertical2WasOnLine = false;
		private bool SensorHorizontal1WasOnLine = false;
		private bool SensorHorizontal2WasOnLine = false;

		private bool SensorDiagonal1WasOnLine = false;
		private bool SensorDiagonal2WasOnLine = false;
		private bool SensorDiagonal3WasOnLine = false;
		private bool SensorDiagonal4WasOnLine = false;

		private bool SensorMiddle1WasOnLine = false;

		public override string Name => "FindPathsOfNode";
		private const float LineThreshold = 1f;

		private bool StartedOnLine = false;
		private bool StartedOnLineFoundLine = false;
		private bool LinePreviouslyHovered = false;
		private MapNode CurrentNode;
		private List<LineEdgePoints> EdgePoints = new List<LineEdgePoints>();
		private List<string> DetectedCharacters = new List<string>();
		private int SensorHorizontal1Count = 0;
		private int SensorHorizontal2Count = 0;

		private int SensorVertical1Count = 0;
		private int SensorVertical2Count = 0;

		private int SensorDiagonal1Count = 0;
		private int SensorDiagonal2Count = 0;
		private int SensorDiagonal3Count = 0;
		private int SensorDiagonal4Count = 0;

		private int SensorMiddle1Count = 0;



		private IRSensor SensorHorizontal1;
		private IRSensor SensorHorizontal2;

		private IRSensor SensorVertical1;
		private IRSensor SensorVertical2;

		private IRSensor SensorDiagonal1;
		private IRSensor SensorDiagonal2;
		private IRSensor SensorDiagonal3;
		private IRSensor SensorDiagonal4;

		private IRSensor SensorMiddle1;

		struct LineEdgePoints
		{
			public Vector2 Left;
			public Vector2 Right;
		}


		public FindPathsOfNode(VehicleController vehicle) : base(vehicle)
		{
			this.CurrentNode = this.Vehicle.NodeStack.Last();
		}

		public override void Start()
		{
			this.SensorHorizontal1 = this.Vehicle.SensorBoard.HorizontalSensors[0];
			this.SensorHorizontal2 = this.Vehicle.SensorBoard.HorizontalSensors[1];

			this.SensorVertical1 = this.Vehicle.SensorBoard.VerticalSensors[0];
			this.SensorVertical2 = this.Vehicle.SensorBoard.VerticalSensors[1];

			this.SensorDiagonal1 = this.Vehicle.SensorBoard.DiagonalSensors[0];
			this.SensorDiagonal2 = this.Vehicle.SensorBoard.DiagonalSensors[1];
			this.SensorDiagonal3 = this.Vehicle.SensorBoard.DiagonalSensors[2];
			this.SensorDiagonal4 = this.Vehicle.SensorBoard.DiagonalSensors[3];

			this.SensorMiddle1 = this.Vehicle.SensorBoard.MiddleSensor;
			// Check if outgoing paths are already scanned
			if (this.CurrentNode.OutgoingPathsScanned)
			{
				this.Vehicle.SetState(new ChooseNextPath(this.Vehicle));
				return;
			}

			// Check if front middle sensor is on a line
			if (Pathing.IsOnLine(this.Vehicle.SensorBoard.PathDetectionSensor))
			{
				this.StartedOnLine = true;
				this.Vehicle.Drive.TurnLeftOnSpot(0.2f); // Slow rotation
			}
			else
			{
				StartScanningForPathsAndCharacters();
			}
		}

		private void AddPaths()
		{
			// Add outgoing paths
			this.EdgePoints.ForEach(edgePoint =>
			{
				if (edgePoint.Left == null || edgePoint.Right == null)
				{
					return;
				}

				// Figure out the middle of the line
				Vector2 middle = edgePoint.Left + 0.5f * (edgePoint.Right - edgePoint.Left);
				this.CurrentNode.AddOutgoingPathPosition(middle);
			});
			this.CurrentNode.OutgoingPathsScanned = true;
		}

		private void StartScanningForPathsAndCharacters()
		{
			this.LinePreviouslyHovered = false;

			// Rotate the vehicle and simultaneously scan
			this.Vehicle.Drive.TurnDeg(360, () =>
			{
				// Add the found paths to the node
				this.AddPaths();
				Minimap.Instance.UpdateMap();
				DetectCharacters();

				this.Vehicle.SetState(new ChooseNextPath(this.Vehicle));
			});
		}

		private void DetectCharacters()
		{
			//TODO: Implement character detection

			int totalLinesDetected = this.SensorHorizontal1Count + this.SensorHorizontal2Count + this.SensorVertical1Count + this.SensorVertical2Count + this.SensorDiagonal1Count + this.SensorDiagonal2Count + this.SensorDiagonal3Count + this.SensorDiagonal4Count + this.SensorMiddle1Count;
			if (totalLinesDetected > 30)
			{
				this.DetectedCharacters.Add("B");
			}
			if (this.SensorDiagonal2Count < 18)
			{
				this.DetectedCharacters.Add("C");
			}
			this.DetectedCharacters.Add("A");

			Debug.Log($"Detected characters: {string.Join(", ", this.DetectedCharacters)}");

			if (Physics.Raycast(this.Vehicle.Position, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.NameToLayer("Graph")))
			{
				// Check if we hit a node
				Debug.Log("Gameobbject: " + hit.collider.gameObject.name);
				Node node = hit.collider.GetComponent<Node>();
				if (node != null && node.GetName() == "endPlease")
				{
					// Stop the vehicle
					this.Vehicle.Drive.Stop();
					// Disable the line follower
					this.Vehicle.LineFollower.Disable();
					// Set the state to end
					this.Vehicle.SetState(new EndReached(this.Vehicle));
					return;
				}
			}
			return;
		}

		public void DidSensorCrossALine(IRSensor sensor, ref bool wasOnLine, ref int lineCount)
		{
			sensor.UpdateBlackWhiteSensorReadings(sensor.GetReflectedLight());

			bool isOnLine = sensor.GetBlackWhiteRecentReadings().All(value => value < 0.4f);

			// Debounce mechanism for Sensor1
			if (!wasOnLine && isOnLine)
			{
				Debug.Log($"Sensor detected a black line! sdf {string.Join(", ", sensor.GetRecentReadings())}");

				wasOnLine = true;
			}
			else if (wasOnLine && !(sensor.GetReflectedLightAndCheckBlackWhite() < 0.4f))
			{
				lineCount++;
				wasOnLine = false;
				Debug.Log("Sensor detected a black line!");
			}
		}


		public override void Update()
		{

			//Debug.Log("Sensor1: " + Sensor1.GetReflectedLightAndCheckBlackWhite() + ", Sensor2: " + Sensor2.GetReflectedLightAndCheckBlackWhite());
			// Debounce mechanism for Sensor1

			DidSensorCrossALine(SensorDiagonal1, ref SensorDiagonal1WasOnLine, ref SensorDiagonal1Count);
			DidSensorCrossALine(SensorDiagonal2, ref SensorDiagonal2WasOnLine, ref SensorDiagonal2Count);
			DidSensorCrossALine(SensorDiagonal3, ref SensorDiagonal3WasOnLine, ref SensorDiagonal3Count);
			DidSensorCrossALine(SensorDiagonal4, ref SensorDiagonal4WasOnLine, ref SensorDiagonal4Count);

			DidSensorCrossALine(SensorHorizontal1, ref SensorHorizontal1WasOnLine, ref SensorHorizontal1Count);
			DidSensorCrossALine(SensorHorizontal2, ref SensorHorizontal2WasOnLine, ref SensorHorizontal2Count);

			DidSensorCrossALine(SensorVertical1, ref SensorVertical1WasOnLine, ref SensorVertical1Count);
			DidSensorCrossALine(SensorVertical2, ref SensorVertical2WasOnLine, ref SensorVertical2Count);

			DidSensorCrossALine(SensorMiddle1, ref SensorMiddle1WasOnLine, ref SensorMiddle1Count);

			if (this.CurrentNode.OutgoingPathsScanned) return;

			bool isFrontSensorOnLine = Pathing.IsOnLine(this.Vehicle.SensorBoard.PathDetectionSensor);

			// Handle rotating until no longer on a line
			if (this.StartedOnLine && !this.StartedOnLineFoundLine)
			{


				if (isFrontSensorOnLine)
				{
					return;
				}

				this.StartedOnLineFoundLine = true;
				// Behind line now
				StartScanningForPathsAndCharacters();
				return;
			}


			// Check if we are hovering a line
			if (isFrontSensorOnLine)
			{
				// Were we previously on a line?
				if (this.LinePreviouslyHovered)
				{
					return;
				}

				this.LinePreviouslyHovered = true;

				// Initially store the position
				this.EdgePoints.Add(new LineEdgePoints()
				{
					Left = this.Vehicle.Position + this.Vehicle.Forward * this.Vehicle.SensorBoard.PathDetectionSensorDistanceFromCenter
				});
			}
			else
			{
				// No longer on a line?
				if (this.LinePreviouslyHovered)
				{

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
