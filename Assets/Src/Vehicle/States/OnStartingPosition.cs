using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.Vehicle.States
{
    public class OnStartingPosition : VehicleState
    {

        private LineFollower LineFollower;
        private float Speed = 2f;
     
        public OnStartingPosition(GameObject vehicle) : base(vehicle)
        {
            this.LineFollower = this.Vehicle.GetComponent<LineFollower>();
        }

        public override void Update()
        {
            bool leftDetected = this.LineFollower.IsLeftDetected();

            // First circle reached?
            if (!leftDetected)
            {
                this.LineFollower.SetState(new MoveForward(this.Vehicle));
                return;
            }

            // Move forward
            this.Vehicle.transform.Translate(this.Vehicle.transform.forward * this.Speed * Time.deltaTime);
        }
    }
}
