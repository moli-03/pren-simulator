using Assets.Src.Util;
using UnityEngine;

namespace Assets.Src.Vehicle.Graph {

	public class MapPath {

		public bool HasBarrier = false;
		public bool IsVisited => this.Start != null && this.End != null;
		public MapNode Start = null;
		public Vector2? StartOutgoingPathPosition = null;
		public MapNode End = null;
		public Vector2? EndOutgoingPathPosition = null;
		public float Length => (End.Position - Start.Position).magnitude;

		public LineRenderer lineRenderer;

		public Vector2? GetOutgoingPositionFor(MapNode node) {

			if (this.Start == node) {
				return this.StartOutgoingPathPosition;
			}
			else if (this.End == node) {
				return this.EndOutgoingPathPosition;
			}

			return null;
		}


		public Vector2? GetOutgoingDirectionFor(MapNode node) {

			if (!this.IsVisited) {
				return null;
			}

			if (this.Start == node) {
				return this.End.Position - this.Start.Position;
			}
			else if (this.End == node) {
				return this.Start.Position - this.End.Position;
			}

			return null;
		}

		public void DrawDebugLines() {

			Vector3 start = Pathing.ToWorldPosition(this.Start.Position);
			start.y = 0.05f;
			Vector3 direction = Pathing.ToWorldPosition(this.End.Position - this.Start.Position);
			direction.y = 0.05f;
			Draw.DrawLine(start, direction);

		}
	}

}