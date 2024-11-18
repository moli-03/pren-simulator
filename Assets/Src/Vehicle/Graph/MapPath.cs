using Assets.Src.Util;
using UnityEngine;

namespace Assets.Src.Vehicle.Graph {

	public class MapPath {

		public bool IsVisited = false;
		public bool HasBarrier = false;
		public MapNode Start = null;
		public MapNode End = null;
		public Vector2 Direction = Vector3.zero;
		public float Length => (End.Position - Start.Position).magnitude;

		public LineRenderer lineRenderer;

		public void DrawDebugLines() {

			Draw.DrawLine(Pathing.ToWorldPosition(this.Start.Position), Pathing.Vec2ToVec3(this.Direction));

		}
	}

}