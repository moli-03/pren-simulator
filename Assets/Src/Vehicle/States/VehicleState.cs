using UnityEngine;

namespace Assets.Src.Vehicle.States
{
    public abstract class VehicleState
    {

        protected VehicleController Vehicle;

        public VehicleState(VehicleController vehicle)
        {
            this.Vehicle = vehicle;
        }

        public void Update() {

		}

		public void FixedUpdate() {
			
		}

    }
}
