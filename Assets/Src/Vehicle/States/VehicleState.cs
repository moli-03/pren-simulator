using UnityEngine;

namespace Assets.Src.Vehicle.States
{
    public abstract class VehicleState
    {
		public abstract string Name { get; }

        protected VehicleController Vehicle;

        public VehicleState(VehicleController vehicle)
        {
            this.Vehicle = vehicle;
        }

		public virtual void Start() {

		}

        public virtual void Update() {

		}

		public virtual void FixedUpdate() {
			
		}

    }
}
