using System.Collections.Generic;
using Assets.Src.Util;
using UnityEngine;

namespace Assets.Src.Vehicle.Graph {

	public class MapNode {

		// The position of the node
		public Vector2 Position { get; }

		// The directions of the outgoing paths
		public List<MapPath> OutgoingPaths = new List<MapPath>();

		public GameObject DebugObject;

		public bool OutgoingPathsScanned = false;

		public MapNode(Vector2 position) {
			this.Position = position;

			Vector3 circlePos = Pathing.ToWorldPosition(this.Position);
			circlePos.y = 0.05f;
			Draw.DrawCircle(circlePos, Color.cyan);
		}

		public void AddOutgoingPath(Vector2 direction) {
			MapPath outgoingPath = new MapPath();
			outgoingPath.Direction = direction.normalized;
			outgoingPath.Start = this;
			this.OutgoingPaths.Add(outgoingPath);

			outgoingPath.DrawDebugLines();
		}

		public void AddOutgoingPath(MapPath path) {
			this.OutgoingPaths.Add(path);
		}
	}

}