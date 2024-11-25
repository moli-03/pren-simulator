using System.Linq;
using Assets.Src.Util;
using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States {

	public class ConeDetected : VehicleState
	{
		public override string Name => "ConeDetected";

		private float bottomSensorDistance;

		public ConeDetected(VehicleController vehicle, float bottomSensorDistance) : base(vehicle) {
			this.bottomSensorDistance = bottomSensorDistance;
		}


		public override void Start()
		{
			// Calculate rough center of the cone
			Vector2 coneCenter = this.Vehicle.Position + this.Vehicle.Forward * (this.Vehicle.BottomDistanceSensor.transform.localPosition.z + bottomSensorDistance + Constants.CONE_RADIUS_AT_BOTTOM_SENSOR_HEIGHT);

			// Get the node
			MapNode node = this.Vehicle.Map.GetNodeAt(coneCenter);

			// If it does not exist yet we need to add it
			if (node == null) {
				node = this.Vehicle.Map.AddNodeAt(coneCenter);
				node.HasCone = true;
			}

			MapNode lastNode = this.Vehicle.NodeStack.Last();

			// Add a path between them
			this.Vehicle.Map.AddPathBetween(lastNode, node);

			// Turn around
			this.Vehicle.Drive.TurnDeg(180f, () => {

				this.Vehicle.SetState(new FollowLine(this.Vehicle));
			});
		}

	}

}