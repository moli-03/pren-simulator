using System.Collections.Generic;
using Assets.Src.Util;
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

		public void AddOutgoingPathPosition(Vector2 position) {
			this.OutgoingPathScanPositions.Add(position);

			Vector3 positionVec3 = Pathing.ToWorldPosition(position);
			positionVec3.y = 0.01f;
			Draw.DrawCircle(positionVec3, Color.magenta);
		}

	}

}