using System.Collections.Generic;
using Assets.Src.Util;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Src.Vehicle.Graph {

	public class MapNode {

		// The position of the node
		public Vector2 Position { get; }

		public List<Vector2> OutgoingPathScanPositions = new List<Vector2>();

		// The directions of the outgoing paths
		public List<MapPath> OutgoingPaths = new List<MapPath>();

		public GameObject DebugObject;

		public bool OutgoingPathsScanned = false;

		public MapNode(Vector2 position) {
			this.Position = position;

			Vector3 circlePos = Pathing.ToWorldPosition(this.Position);
			circlePos.y = 0.01f;
			Draw.DrawCircle(circlePos, Color.cyan);
		}

		public void AddOutgoingPath(MapPath path) {

			// Don't add twice
			if (this.OutgoingPaths.Contains(path)) {
				return;
			}

			this.OutgoingPaths.Add(path);
		}

		public void UpdatePathMapping() {
			
			foreach (MapPath path in this.OutgoingPaths) {

				// Map the outgoing path positions to the actual path
				bool isStart = path.Start == this;

				// Get the actual direction between the nodes
				Vector2 actualDirection = isStart ? path.End.Position - path.Start.Position : path.Start.Position - path.End.Position;
				float minAngle = 5f;
				foreach (Vector2 outgoingPathPosition in this.OutgoingPathScanPositions) {

					Vector2 directionByPosition = outgoingPathPosition - this.Position;
					float angleDeg = Mathf.Abs(Vector2.SignedAngle(actualDirection, directionByPosition));

					// Get the one with the smallest angle between
					if (angleDeg <= minAngle) {
						minAngle = angleDeg;
					
						if (isStart) {
							path.StartOutgoingPathPosition = outgoingPathPosition;
						}
						else {
							path.EndOutgoingPathPosition = outgoingPathPosition;
						}
					}
				}
			}
		}

		public void AddOutgoingPathPosition(Vector2 position) {
			this.OutgoingPathScanPositions.Add(position);

			Vector3 positionVec3 = Pathing.ToWorldPosition(position);
			positionVec3.y = 0.01f;
			Draw.DrawCircle(positionVec3, Color.magenta);
		}

	}

}