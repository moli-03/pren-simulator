using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.Vehicle.States
{
    public class MoveForward : VehicleState
    {

        private float Speed = 3f;
     
        public MoveForward(GameObject vehicle) : base(vehicle) {
            this.LineFollower = this.Vehicle.GetComponent<LineFollower>();
        }

        private LineFollower LineFollower;

        public override void Update()
        {
            bool leftDetected = this.LineFollower.IsLeftDetected();
            bool rightDetected = this.LineFollower.IsRightDetected();

            if (leftDetected && rightDetected)
            {
                this.LineFollower.SetState(new CircleReached(this.Vehicle));
                return;
            }

            if (leftDetected)
            {
                this.LineFollower.SetState(new AdjustToLeft(this.Vehicle));
                return;
            }

            if (!rightDetected)
            {
                this.LineFollower.SetState(new AdjustToRight(this.Vehicle));
                return;
            }

            // Move forward
            this.Vehicle.transform.Translate(this.Vehicle.transform.forward * this.Speed * Time.deltaTime);
        }
    }
}
