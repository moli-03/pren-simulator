using System.Collections.Generic;
using Assets.Src.Vehicle;
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
	public List<MapNode> NodeStack = new List<MapNode>();

	[HideInInspector]
	public MapPath CurrentPath = null;

	[HideInInspector]
	public DifferentialDrive Drive { get; private set; }

	[HideInInspector]
	public LineFollower LineFollower { get; private set; }

	[HideInInspector]
	public ClawController Claw { get; private set; }

	[HideInInspector]
	public LineSensorBoard SensorBoard { get; private set; }

	[HideInInspector]
	public DistanceSensor BottomDistanceSensor { get; private set; }
	
	[HideInInspector]
	public DistanceSensor TopDistanceSensor { get; private set; }

	public Vector2 Position => this.Drive.Position;
	public float Orientation => this.Drive.Orientation;
	public Vector2 Forward => this.Drive.Forward;

	public void SetState(VehicleState state) {
		this.State = state;

		UIController.Instance.UpdateState(state);

		this.State.Start();
	}


    // Start is called before the first frame update
    void Start()
    {
		Instance = this;
		this.Drive = this.GetComponent<DifferentialDrive>();
		this.Map = new VehicleMap();
        this.SetState(new WaitingOnStartingPosition(this));
		this.SensorBoard = this.GetComponentInChildren<LineSensorBoard>();
		this.BottomDistanceSensor = this.transform.Find("BottomDistanceSensor").GetComponent<DistanceSensor>();
		this.TopDistanceSensor = this.transform.Find("TopDistanceSensor").GetComponent<DistanceSensor>();
		this.Claw = this.GetComponentInChildren<ClawController>();
		this.LineFollower = this.GetComponent<LineFollower>();
		Minimap.Instance.Vehicle = this;
		Minimap.Instance.SetStartingPosition(this.transform.position);
    }


    void FixedUpdate()
    {
        this.State.FixedUpdate();
    }

	void Update() {
		this.State.Update();
	}

}
