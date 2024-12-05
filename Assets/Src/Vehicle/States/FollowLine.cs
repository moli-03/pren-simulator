using System.Linq;
using System.Threading;
using Assets.Src.Util;
using Assets.Src.Vehicle.Graph;
using Assets.Src.Vehicle.States.Barrier;
using UnityEngine;

namespace Assets.Src.Vehicle.States
{

	public class FollowLine : VehicleState
	{
		public override string Name => "FollowLine";

		private float MinDistance = 0.3f;
		private Vector2 StartingPosition;
		private Vector2? FrontSensorsFirstHitAt = null;
		private Vector2? TargetPosition = null;
		private float MinDistanceToObstacleBeforeRecognition = 0.15f;

		public FollowLine(VehicleController vehicle) : base(vehicle)
		{
			this.StartingPosition = this.Vehicle.Position;
			this.Vehicle.LineFollower.Enable();
			this.Vehicle.LineFollower.SetSpeed(0.3f);
		}


		private void HandleObstacleRecognition()
		{

			// Check if the bottom sensor found something
			float bottomSensorDistance = this.Vehicle.BottomDistanceSensor.GetDistance();

			//  Nope
			if (bottomSensorDistance == -1)
			{
				return;
			}

			// Drive towards the obstacle until its closer
			if (bottomSensorDistance > this.MinDistanceToObstacleBeforeRecognition)
			{
				return;
			}

			// Get the top sensor distance
			float topSensorDistance = this.Vehicle.TopDistanceSensor.GetDistance();

			float topSensorDistanceX = Mathf.Cos(this.Vehicle.TopDistanceSensor.transform.rotation.eulerAngles.x * Mathf.Deg2Rad) * topSensorDistance;

			// If both the top and bottom sensor are roughly the same distance away
			if (Mathf.Abs(topSensorDistanceX - bottomSensorDistance) <= Constants.CONE_RADIUS_AT_BOTTOM_SENSOR_HEIGHT)
			{

				// Stop
				this.Vehicle.LineFollower.Disable();
				this.Vehicle.Drive.Stop();

				// Next state
				this.Vehicle.SetState(new ConeDetected(this.Vehicle, bottomSensorDistance));
			}
			else
			{

				// Stop
				this.Vehicle.LineFollower.Disable();
				this.Vehicle.Drive.Stop();

				// Next state
				this.Vehicle.SetState(new BarrierDetected(this.Vehicle, bottomSensorDistance));
			}
		}

		public override void Update()
		{
			// Update sensor readings for trend detection
			foreach (var sensor in this.Vehicle.SensorBoard.FrontLineFollowSensors)
			{
				sensor.UpdateSensorReadings(sensor.GetReflectedLight());
			}

			// Debugging: Log sensor trends
			foreach (var sensor in this.Vehicle.SensorBoard.FrontLineFollowSensors)
			{
				//Debug.Log($"Sensor Readings: {string.Join(", ", sensor.GetRecentReadings())}");
			}

			// Drive for at least the min distance before checking for nodes (temporarily disabled and probably forever)
			if ((this.StartingPosition - this.Vehicle.Position).magnitude >= this.MinDistance || true)
			{
				// Check for a first hit of the front sensors
				if (!this.FrontSensorsFirstHitAt.HasValue)
				{

					if (this.Vehicle.SensorBoard.FrontLineFollowSensors.All(sensor => Pathing.IsOnLine(sensor)))
					{
						this.Vehicle.LineFollower.SetSpeed(0.2f);
						this.FrontSensorsFirstHitAt = this.Vehicle.Position;
					}
				}
				else if (this.FrontSensorsFirstHitAt.HasValue && !this.TargetPosition.HasValue)
				{
					// Now any of the front sensors has to leave the circle
					if (this.CheckIfAnySensorHasLeftCircle())
					{
						float distanceTraveled = (this.Vehicle.Position - this.FrontSensorsFirstHitAt.Value).magnitude;
						this.TargetPosition = this.Vehicle.Position + this.Vehicle.Forward * (distanceTraveled / 2f);


						//Debug.Log($"Target Position: {this.TargetPosition}");
						this.Vehicle.LineFollower.SetSpeed(0.1f);
					}
				}
				else if (this.TargetPosition.HasValue)
				{
					float distanceToTarget = (this.TargetPosition.Value - this.Vehicle.Position).magnitude;

					if (distanceToTarget <= 0.007f) // 7mm tolerance
					{
						// Stop
						this.Vehicle.LineFollower.Disable();
						this.Vehicle.Drive.Stop();

						this.Vehicle.SetState(new NodeReached(this.Vehicle));
						return;
					}
				}
			}

			this.HandleObstacleRecognition();
		}



		private bool CheckIfAnySensorHasLeftCircle()
		{

			return this.Vehicle.SensorBoard.FrontLineFollowSensors.Any(sensor =>
			{
				if (Pathing.IsBlackLine(sensor))
				{
					//Debug.Log($"Sensor Readings: (IsBlackLine) {string.Join(", ", sensor.GetRecentReadings())}");
					return false;
				}
				if (sensor.IsTransitioning())
				{
					//Debug.Log($"Sensor Readings: (IsTransitioning) {string.Join(", ", sensor.GetRecentReadings())}");
					return false;
				}
				if (!Pathing.IsOnGround(sensor))
				{
					return false;
				}
				//Debug.Log($"Sensor Readings: {string.Join(", ", sensor.GetRecentReadings())}");
				return true;
			});
		}

	}
}