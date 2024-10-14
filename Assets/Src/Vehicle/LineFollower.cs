using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Src.Vehicle.States;

public class LineFollower : MonoBehaviour
{

    public IRSensor LeftSensor;

    public IRSensor RightSensor;

    public static readonly float MinLineReflectionValue = 0.7f;

    private VehicleState State;

    // Start is called before the first frame update
    void Start()
    {
        this.State = new OnStartingPosition(this.gameObject);
    }


    private bool IsOnLine(float reflectedValue)
    {
        return reflectedValue >= MinLineReflectionValue;
    }

    public void SetState(VehicleState state)
    {
        this.State = state;
    }

    public bool IsLeftDetected()
    {
        return this.IsOnLine(this.LeftSensor.GetReflectedLight());
    }

    
    public bool IsRightDetected()
    {
        return this.IsOnLine(this.RightSensor.GetReflectedLight());
    }

    // Update is called once per frame
    void Update()
    {

        this.State.Update();
    }

}
