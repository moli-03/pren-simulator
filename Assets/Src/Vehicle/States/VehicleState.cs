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

        public abstract void Update();

    }
}
