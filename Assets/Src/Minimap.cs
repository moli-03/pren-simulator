using Assets.Src.Util;
using Assets.Src.Vehicle.Graph;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{

	public static Minimap Instance { get; private set; }

	public VehicleController Vehicle;

	public RawImage drawingArea;  // Attach the UI panel here (RawImage component)
	private Color BackgroundColor = new Color(0, 0, 0, 0.7f);
    private Texture2D texture;    // The texture to draw on
	private Vector2	lastPosition = Vector2.zero;
	private float WorldToMapRatio;	// Ratio to convert world coordinates (m) to coordinates on the texture
	private int OffsetX = 0;
	private int OffsetY = 0;
	private int DistanceBottomToMap;

    void Start()
    {
		Instance = this;
		this.drawingArea = this.GetComponent<RawImage>();
        // Create a new texture with the same size as the RawImage
        texture = new Texture2D((int)drawingArea.rectTransform.rect.width, (int)drawingArea.rectTransform.rect.height, TextureFormat.RGBA32, false);
        // Set the texture to the RawImage to display it
        this.drawingArea.texture = texture;
        this.texture.filterMode = FilterMode.Point; // Optional: Set the filter mode

		this.DistanceBottomToMap = texture.height - texture.width;

		this.WorldToMapRatio = 1f / Constants.MAP_WIDTH * texture.width;

		// Initially clear the minimap
        ClearTexture();
    }

	public void SetStartingPosition(Vector3 startingPosition) {
		this.OffsetX = (int)(Pathing.Vec3ToVec2(startingPosition).x * this.WorldToMapRatio);
		this.OffsetY = (int)(Pathing.Vec3ToVec2(startingPosition).y * this.WorldToMapRatio);
	}


	public void AddCone(Vector2 position) {

	}


	public void AddBarrier(Vector2 position) {

	}


	public void AddNode(Vector2 position) {
		this.DrawCircle(position, Constants.NODE_RADIUS, Color.white);
	}

	private Vector2Int ToMapPosition(Vector2 worldPosition) {
		return new Vector2Int((int)(worldPosition.x * this.WorldToMapRatio + this.OffsetX), (int)(worldPosition.y * this.WorldToMapRatio + this.OffsetY + this.DistanceBottomToMap));
	}

	private int ToMapDistance(float distance) {
		return (int)(distance * this.WorldToMapRatio);
	}

	private void DrawCircle(Vector2 center, float radius, Color color) {

		Vector2Int mapPosition = this.ToMapPosition(center);
		int radiusPixel = this.ToMapDistance(radius);

		for (int x = mapPosition.x - radiusPixel; x <= mapPosition.x + radiusPixel; x++) {

			for (int y = mapPosition.y + radiusPixel; y >= mapPosition.y - radiusPixel; y--) {

				float distance = Vector2Int.Distance(new Vector2Int(x, y), mapPosition);

				if (distance <= radiusPixel) {
					texture.SetPixel(x, y, color);
				}
			}
		}
	}

	void DrawLine(Vector2 start, Vector2 end, Color color) {

		Vector2Int mapStart = this.ToMapPosition(start);
		Vector2Int mapEnd = this.ToMapPosition(end);

		// ChatGPT cooked

		int x0 = mapStart.x;
		int y0 = mapStart.y;
		int x1 = mapEnd.x;
		int y1 = mapEnd.y;

		int dx = Mathf.Abs(x1 - x0);
		int dy = Mathf.Abs(y1 - y0);

		int sx = x0 < x1 ? 1 : -1;
		int sy = y0 < y1 ? 1 : -1;

		int err = dx - dy;

		while (true)
		{
			// Set the pixel at the current position
			texture.SetPixel(x0, y0, color);

			// Check if the line has reached the endpoint
			if (x0 == x1 && y0 == y1)
				break;

			// Calculate the next pixel position
			int e2 = 2 * err;
			if (e2 > -dy)
			{
				err -= dy;
				x0 += sx;
			}
			if (e2 < dx)
			{
				err += dx;
				y0 += sy;
			}
		}
	}



	private void AddVehicle(Vector2 position) {

		int vehicleWidth = this.ToMapDistance(Constants.VEHICLE_WIDTH);
		int vehicleHeight = this.ToMapDistance(Constants.VEHICLE_HEIGHT);
		Vector2Int mapPosition = this.ToMapPosition(position);
		int xMin = mapPosition.x - vehicleWidth / 2;
		int xMax = mapPosition.x + vehicleWidth / 2;
		int yMax = mapPosition.y + vehicleHeight / 2;
		int yMin = mapPosition.y - vehicleHeight / 2;

		for (int x = xMin; x <= xMax; x++) {
			for (int y = yMax; y >= yMin; y--) {
				texture.SetPixel(x, y, Color.white);
			}
		}
	}


    public void UpdateMap()
    {

		if (this.Vehicle == null) {
			return;
		}

		// Clear the minimap
		ClearTexture();

		// Add all known nodes
		foreach (MapNode node in this.Vehicle.Map.Nodes) {

			// Draw all the outgoing paths
			foreach (Vector2 position in node.OutgoingPathScanPositions){
				Vector2 scaledPosition = node.Position + (position - node.Position).normalized * 0.3f;
				this.DrawLine(node.Position, scaledPosition, Color.magenta);
			}

			// Draw the actually figured out paths
			foreach (MapPath path in node.OutgoingPaths) {
				this.DrawLine(path.Start.Position, path.End.Position, Color.white);
			}

			// Draw the node itself
			this.AddNode(node.Position);
		}

		texture.Apply();
    }

    void ClearTexture()
    {
        // Clear the texture with a white background
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, this.BackgroundColor);
            }
        }
        texture.Apply();
    }
}
