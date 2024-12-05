using System.Linq;
using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States
{

    public class EndReached : VehicleState
    {
        public override string Name => "EndReached";

        public EndReached(VehicleController vehicle) : base(vehicle) { }

        public override void Start()
        {

            // Stop the timer
            UIController.Instance.StopTimer();

            //Stop the vehicle
            this.Vehicle.Drive.Stop();

            //Stop the line follower
            this.Vehicle.LineFollower.Disable();
        }
    }

}