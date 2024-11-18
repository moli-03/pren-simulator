using System.Collections;
using System.Collections.Generic;
using Assets.Src.Vehicle.Graph;
using Assets.Src.Vehicle.States;
using UnityEngine;

public class VehicleController : MonoBehaviour
{

	public static VehicleController Instance { get; private set; }

	private VehicleState State;

	[HideInInspector]
	public VehicleMap Map { get; private set; }

	[HideInInspector]
	public List<MapNode> NodeHistory = new List<MapNode>();
	
	[HideInInspector]
	public DifferentialDrive Drive;

	public LineSensorBoard SensorBoard;

	public Vector2 Position => this.Drive.Position;
	public float Orientation => this.Drive.Orientation;
	public Vector2 Forward => this.Drive.Forward;

	public void SetState(VehicleState state) {
		this.State = state;
	}


	public void StoreNode(Vector2 nodeWorldPosition) {
		MapNode node = this.Map.AddNodeAt(nodeWorldPosition);
		this.NodeHistory.Add(node);
	}
	

    // Start is called before the first frame update
    void Start()
    {
		Instance = this;
		this.Drive = this.GetComponent<DifferentialDrive>();
		this.Map = new VehicleMap();
        this.State = new WaitingOnStartingPosition(this);
		this.SensorBoard = this.GetComponentInChildren<LineSensorBoard>();
    }

    // Update is called once per frame
    void Update()
    {
        this.State.Update();
    }

}
