using System.Collections.Generic;
using UnityEngine;

namespace Assets.Src.Vehicle.Graph {

	public class MapNode {

		// The position of the node
		public Vector3 Position { get; }

		// The directions of the outgoing paths (not explored yet)
		private List<Vector3> OutgoingPaths = new List<Vector3>();

		public MapNode(Vector3 position) {
			this.Position = position;
		}

		public void AddOutgoingPath(Vector3 path) {
			this.OutgoingPaths.Add(path);
		}
	}

}