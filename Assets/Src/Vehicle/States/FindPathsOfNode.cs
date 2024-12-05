using System.Collections.Generic;
using System.Linq;
using Assets.Src.Util;
using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States
{

	public class FindPathsOfNode : VehicleState
	{
		private bool Sensor1WasOnLine = false;
		private bool Sensor2WasOnLine = false;
		public override string Name => "FindPathsOfNode";
		private const float LineThreshold = 1f;

		private bool StartedOnLine = false;
		private bool StartedOnLineFoundLine = false;
		private bool LinePreviouslyHovered = false;
		private MapNode CurrentNode;
		private List<LineEdgePoints> EdgePoints = new List<LineEdgePoints>();
		private List<string> DetectedCharacters = new List<string>();
		private int Sensor1LineCount = 0;
		private int Sensor2LineCount = 0;

		private IRSensor Sensor1; // First sensor
		private IRSensor Sensor2; // Second sensor

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
			this.Sensor1 = this.Vehicle.SensorBoard.HorizontalSensors[0];
			this.Sensor2 = this.Vehicle.SensorBoard.DiagonalSensors[2];
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
			Debug.Log($"Sensor1 Line Count: {Sensor1LineCount}");
			if (Sensor1LineCount > 4 && Sensor2LineCount > 4)
			{
				Debug.Log("Character B detected: " + Sensor1LineCount + ", Sensor2LineCount: " + Sensor2LineCount);
				CurrentNode.SetCharacter("B"); // Update node property if needed
				return;
			}
			// Determine character presence based on line count
			if (Sensor1LineCount > 3 && Sensor2LineCount > 4)
			{
				Debug.Log("Character A detected: " + Sensor1LineCount + ", Sensor2LineCount: " + Sensor2LineCount);
				CurrentNode.SetCharacter("A"); // Update node property if needed
				return;
			}

			if (Sensor1LineCount > 2 && Sensor2LineCount > 3)
			{
				Debug.Log("Character C detected: " + Sensor1LineCount + ", Sensor2LineCount: " + Sensor2LineCount);
				CurrentNode.SetCharacter("C"); // Update node property if needed
				return;
			}
			Debug.Log("Detected: " + Sensor1LineCount + ", Sensor2LineCount: " + Sensor2LineCount);

			return;
		}


		public override void Update()
		{

			bool isSensor1OnLine = Sensor1.GetReflectedLightAndCheckBlackWhite() < 0.4f;
			bool isSensor2OnLine = Sensor2.GetReflectedLightAndCheckBlackWhite() < 0.4f;

			// Debounce mechanism for Sensor1
			if (!Sensor1WasOnLine && isSensor1OnLine)
			{
				Sensor1WasOnLine = true;
			}
			else if (Sensor1WasOnLine && !isSensor1OnLine)
			{
				Sensor1LineCount++;
				Sensor1WasOnLine = false;
			}

			// Debounce mechanism for Sensor2
			if (!Sensor2WasOnLine && isSensor2OnLine)
			{
				Sensor2WasOnLine = true;
			}
			else if (Sensor2WasOnLine && !isSensor2OnLine)
			{
				Sensor2LineCount++;
				Sensor2WasOnLine = false;
				Debug.Log("Sensor2 detected a black line!");
			}

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
