using System.Collections.Generic;
using Assets.Src.Util;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Src.Vehicle.Graph {

	public class MapNode {

		// The position of the node
		public Vector2 Position { get; }

		// The directions of the outgoing paths
		public List<MapPath> OutgoingPaths = new List<MapPath>();

		public bool HasCone = false;

		public GameObject DebugObject;

		public bool OutgoingPathsScanned = false;

		public MapNode(Vector2 position) {
			this.Position = position;

			Vector3 circlePos = Pathing.ToWorldPosition(this.Position);
			circlePos.y = 0.01f;
			Draw.DrawCircle(circlePos, Color.cyan);
		}

		public MapPath GetOutgoingPathInDirection(Vector2 direction) {

			MapPath chosenPath = null;
			float minAngle = 4f; // Deg

			foreach (MapPath path in this.OutgoingPaths) {

				// First check visited direction
				Vector2? outgoingDirection = path.GetOutgoingDirectionFor(this);

				if (outgoingDirection.HasValue) {
					float angle = Mathf.Abs(Vector2.SignedAngle(direction, outgoingDirection.Value));

					if (minAngle > angle) {
						minAngle = angle;
						chosenPath = path;
					}
				}

				// Check the outgoing postions alternatively
				else {
					Vector2? outgoingPosition = path.GetOutgoingPositionFor(this);

					if (outgoingPosition.HasValue) {
						float angle = Mathf.Abs(Vector2.SignedAngle(direction, outgoingPosition.Value - this.Position));

						if (minAngle > angle) {
							minAngle = angle;
							chosenPath = path;
						}
					}
				}
			}

			return chosenPath;
		}

		public void AddOutgoingPath(MapPath path) {

			// Don't add twice
			if (this.OutgoingPaths.Contains(path)) {
				return;
			}

			this.OutgoingPaths.Add(path);
		}

		public void AddOutgoingPathPosition(Vector2 position) {

			// Is there already a path in that direction?
			MapPath path = this.GetOutgoingPathInDirection(position - this.Position);

			if (path == null) {
				path = new MapPath();
				path.Start = this;
				path.StartOutgoingPathPosition = position;
				this.OutgoingPaths.Add(path);
			}
			else {

				if (path.Start == this) {
					path.StartOutgoingPathPosition = position;
				} else if (path.End == this) {
					path.EndOutgoingPathPosition = position;
				}
			}


			// Debug stuff
			Vector3 positionVec3 = Pathing.ToWorldPosition(position);
			positionVec3.y = 0.01f;
			Draw.DrawCircle(positionVec3, Color.magenta);
		}

	}

}