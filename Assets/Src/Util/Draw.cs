using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Src.Util {

	public class Draw {

		public static GameObject DrawCircle(Vector3 position, Color color) {
			var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			obj.transform.localScale = new Vector3(0.03f, 0.005f, 0.03f);
			obj.transform.position = position;
			obj.GetComponent<Renderer>().material.color = color;
			GameObject.Destroy(obj.GetComponent<CapsuleCollider>());

			return obj;
		}


		public static GameObject DrawLine(Vector3 origin, Vector3 direction, Color color) {
			var obj = new GameObject("Line");
			var lineRenderer = obj.AddComponent<LineRenderer>();
			lineRenderer.startColor = color;
			lineRenderer.endColor = color;
			lineRenderer.useWorldSpace = true;
        	lineRenderer.startWidth = 0.008f;
        	lineRenderer.endWidth = 0.008f;
        	lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
			lineRenderer.SetPosition(0, origin);
			lineRenderer.SetPosition(1, origin + direction);

			return obj;
		}


		public static GameObject DrawLine(Vector3 origin, Vector3 direction) {
			return DrawLine(origin, direction, Color.cyan);	
		}

	}

}