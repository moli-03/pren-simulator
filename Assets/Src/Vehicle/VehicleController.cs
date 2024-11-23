using System.Collections;
using System.Collections.Generic;
using Assets.Src.Vehicle.Graph;
using Assets.Src.Vehicle.States;
using TMPro;
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

	private TMP_Text StateLabel;

	public void SetState(VehicleState state) {
		this.State = state;

		this.StateLabel.text = "State: " + state.Name;
	}


	public MapNode StoreNode(Vector2 nodeWorldPosition) {
		MapNode node = this.Map.AddNodeAt(nodeWorldPosition);
		this.NodeHistory.Add(node);
		return node;
	}
	

    // Start is called before the first frame update
    void Start()
    {
		Instance = this;
		this.StateLabel = GameObject.Find("VehicleState").GetComponent<TMP_Text>();
		this.Drive = this.GetComponent<DifferentialDrive>();
		this.Map = new VehicleMap();
        this.SetState(new WaitingOnStartingPosition(this));
		this.SensorBoard = this.GetComponentInChildren<LineSensorBoard>();
    }

    void FixedUpdate()
    {
        this.State.FixedUpdate();
    }

	void Update() {
		this.State.Update();
	}

}
