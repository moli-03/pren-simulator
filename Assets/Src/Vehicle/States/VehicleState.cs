using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.Vehicle.States
{
    public abstract class VehicleState
    {

        protected GameObject Vehicle;

        public VehicleState(GameObject vehicle)
        {
            this.Vehicle = vehicle;
        }

        public abstract void Update();

    }
}
