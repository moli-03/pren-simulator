using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Src.Vehicle.Graph {

	public class VehicleMap {

		private Vector2 Origin;

		private List<MapNode> Nodes = new List<MapNode>();

		public MapNode AddMapNodeFromWorldPosition(Vector2 position) {
			MapNode node = new MapNode(position - this.Origin);
			this.Nodes.Add(new MapNode(position - this.Origin));
			return node;
		}

		public void SetOrigin(Vector2 origin) {
			this.Origin = origin;
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