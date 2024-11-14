using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Src.Vehicle.Graph {

	public class VehicleMap {

		private static int MAX_NODES = 8 + 1;

		private Vector3 Origin;

		private List<MapNode> Nodes = new List<MapNode>();

		private bool[,] Paths = new bool[MAX_NODES, MAX_NODES];

		public MapNode AddMapNodeFromWorldPosition(Vector3 position) {
			MapNode node = new MapNode(position - this.Origin);
			this.Nodes.Add(new MapNode(position - this.Origin));
			return node;
		}

		public void SetOrigin(Vector3 origin) {
			this.Origin = origin;
		}


		public MapNode GetNodeAt(Vector3 position, float tolerance = 0.1f) {

			foreach (MapNode node in this.Nodes) {

				if (
					(Math.Abs(position.x - node.Position.x) <= tolerance)
					&&
					(Math.Abs(position.z - node.Position.z) <= tolerance)
				) {
					return node;
				}

			}

			return null;
		}

	}

}