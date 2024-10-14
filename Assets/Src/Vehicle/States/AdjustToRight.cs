using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.Vehicle.States
{
    public class AdjustToRight : VehicleState
    {
     
        private LineFollower LineFollower;

        private float SpinSpeed = 20f;

        public AdjustToRight(GameObject vehicle) : base(vehicle)
        {
            this.LineFollower = this.Vehicle.GetComponent<LineFollower>();
        }

        public override void Update()
        {
            if (!this.LineFollower.IsRightDetected())
            {
                this.Vehicle.transform.Rotate(this.Vehicle.transform.up, this.SpinSpeed * Time.deltaTime);
                return;
            }

            this.LineFollower.SetState(new MoveForward(this.Vehicle));
        }
    }
}
