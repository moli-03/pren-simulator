using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.Vehicle.States
{
    public class CircleReached : VehicleState
    {

        private LineFollower LineFollower;

        private float SpinSpeed = 30f;

        private bool LeftPreviouslyDetected = false;

        private float MoveSpeed = 2f;

        public CircleReached(GameObject vehicle) : base(vehicle)
        {
            this.LineFollower = this.Vehicle.GetComponent<LineFollower>();
        }

        public override void Update()
        {
            bool leftDetected = this.LineFollower.IsLeftDetected();
            bool rightDetected = this.LineFollower.IsRightDetected();

            // No line detected -> spin left
            if (!leftDetected && !rightDetected)
            {
                this.Vehicle.transform.Rotate(this.Vehicle.transform.up, this.SpinSpeed * Time.deltaTime);
            }

            // Previously undetected and now are both detected -> found a way
            else if (!LeftPreviouslyDetected && leftDetected && rightDetected)
            {
                // Go forward
                this.Vehicle.transform.Translate(this.Vehicle.transform.forward * this.MoveSpeed * Time.deltaTime);
            }
            // Both sensors in the circle
            else if (leftDetected && rightDetected)
            {
                // Go forward
                this.Vehicle.transform.Translate(this.Vehicle.transform.forward * this.MoveSpeed * Time.deltaTime);
            }
            // Only right detected -> go straight
            else if (!leftDetected && rightDetected)
            {
                // Go forward
                this.Vehicle.transform.Translate(this.Vehicle.transform.forward * this.MoveSpeed * Time.deltaTime);
            }

            // Both on the line -> spin right
            else if (!leftDetected && !rightDetected)
            {
                this.Vehicle.transform.Rotate(this.Vehicle.transform.up, this.SpinSpeed * Time.deltaTime);
            }
            else
            {
                Debug.Log("Nuting");
            }

            this.LeftPreviouslyDetected = leftDetected;
        }
    }
}
