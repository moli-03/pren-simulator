using UnityEngine;
using System.Linq; // For LINQ methods like All

namespace Assets.Src.Util
{

	public static class Pathing
	{

		public static readonly float MinLineReflectionValue = 0.8f;

		public static readonly float BlackLineReflectionValue = 0.05f; // Black line's max reflectance

		public static bool IsOnLine(IRSensor sensor)
		{
			return sensor.GetReflectedLight() >= MinLineReflectionValue;
		}

		public static bool IsOnGround(IRSensor sensor)
		{
			float reflectedLight = sensor.GetReflectedLight();
			return sensor.GetRecentReadings().All(value => value > BlackLineReflectionValue && value < 0.9f);
		}
		public static bool IsBlackLine(IRSensor sensor)
		{
			float reflectedLight = sensor.GetReflectedLight();
			return sensor.GetRecentReadings().All(value => value < 0.1f);
		}


		public static Vector2 Vec3ToVec2(Vector3 vector)
		{
			return new Vector2(vector.x, vector.z);
		}

		public static Vector3 Vec2ToVec3(Vector2 vector, float y = 0)
		{
			return new Vector3(vector.x, y, vector.y);
		}

		public static Vector3 ToWorldPosition(Vector2 position)
		{
			return VehicleController.Instance.Drive.InitialWorldPosition + Vec2ToVec3(position);
		}

		public static Vector2 ApplyVehicleRotation(Vector2 vector)
		{

			float cos = Mathf.Cos(VehicleController.Instance.Drive.Orientation);
			float sin = Mathf.Sin(VehicleController.Instance.Drive.Orientation);

			float newX = vector.x * cos - vector.y * sin;
			float newY = vector.x * sin + vector.y * cos;

			return new Vector2(newX, newY);
		}
	}

}