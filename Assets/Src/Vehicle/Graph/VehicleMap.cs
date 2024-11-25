using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Src.Vehicle.Graph {

	public class VehicleMap {

		public List<MapNode> Nodes = new List<MapNode>();

		public MapNode AddNodeAt(Vector2 position) {
			MapNode node = new MapNode(position);
			this.Nodes.Add(node);

			Minimap.Instance.UpdateMap();

			return node;
		}

		public MapPath AddPathBetween(MapNode a, MapNode b) {

			MapPath path = new MapPath();
			path.Start = a;
			path.End = b;

			MapPath pathFromA = a.GetOutgoingPathInDirection(b.Position - a.Position);
			MapPath pathFromB = b.GetOutgoingPathInDirection(a.Position - b.Position);

			if (pathFromA != null) {
				a.OutgoingPaths.Remove(pathFromA);
				path.StartOutgoingPathPosition = pathFromA.GetOutgoingPositionFor(a);
			}
			
			if (pathFromB != null) {
				b.OutgoingPaths.Remove(pathFromB);
				path.EndOutgoingPathPosition = pathFromB.GetOutgoingPositionFor(b);
			}

			a.AddOutgoingPath(path);
			b.AddOutgoingPath(path);

			Minimap.Instance.UpdateMap();

			return path;
		}

		public MapNode GetNodeAt(Vector2 position, float tolerance = 0.07f) {

			foreach (MapNode node in this.Nodes) {

				if ((position - node.Position).magnitude <= tolerance) {
					return node;
				}

			}

			return null;
		}

	}

}