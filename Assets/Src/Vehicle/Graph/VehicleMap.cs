using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Src.Vehicle.Graph {

	public class VehicleMap {

		private List<MapNode> Nodes = new List<MapNode>();

		public MapNode AddNodeAt(Vector2 position) {
			MapNode node = new MapNode(position);
			this.Nodes.Add(node);
			return node;
		}

		public MapNode GetNodeAt(Vector2 position, float tolerance = 0.1f) {

			foreach (MapNode node in this.Nodes) {

				if (
					(Math.Abs(position.x - node.Position.x) <= tolerance)
					&&
					(Math.Abs(position.y - node.Position.y) <= tolerance)
				) {
					return node;
				}

			}

			return null;
		}

	}

}