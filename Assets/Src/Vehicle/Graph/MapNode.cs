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

		public MapNode(Vector2 position) {
			this.Position = position;

			Draw.DrawCircle(Pathing.ToWorldPosition(this.Position), Color.cyan);
		}

		public void AddOutgoingPath(Vector2 direction) {
			MapPath outgoingPath = new MapPath();
			outgoingPath.Direction = direction.normalized;
			outgoingPath.Start = this;
			this.OutgoingPaths.Add(outgoingPath);

			outgoingPath.DrawDebugLines();
		}
	}

}