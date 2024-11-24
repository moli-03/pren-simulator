using Assets.Src.Util;
using UnityEngine;

namespace Assets.Src.Vehicle.Graph {

	public class MapPath {

		public bool HasBarrier = false;
		public MapNode Start = null;
		public Vector2? StartOutgoingPathPosition = null;
		public MapNode End = null;
		public Vector2? EndOutgoingPathPosition = null;
		public float Length => (End.Position - Start.Position).magnitude;

		public LineRenderer lineRenderer;

		public void DrawDebugLines() {

			Vector3 start = Pathing.ToWorldPosition(this.Start.Position);
			start.y = 0.05f;
			Vector3 direction = Pathing.ToWorldPosition(this.End.Position - this.Start.Position);
			direction.y = 0.05f;
			Draw.DrawLine(start, direction);

		}
	}

}