using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.Vehicle.States
{
    public class AdjustToLeft : VehicleState
    {

        private LineFollower LineFollower;

        private float SpinSpeed = 30f;

        public AdjustToLeft(GameObject vehicle) : base(vehicle)
        {
            this.LineFollower = this.Vehicle.GetComponent<LineFollower>();
        }

        public override void Update()
        {
            if (!this.LineFollower.IsLeftDetected())
            {
                this.Vehicle.transform.Rotate(this.Vehicle.transform.up, -this.SpinSpeed * Time.deltaTime);
                return;
            }

            this.LineFollower.SetState(new MoveForward(this.Vehicle));
        }
    }
}
