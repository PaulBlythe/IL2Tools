using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IL2Modder.IL2
{
    public interface CockpitInterface
    {
        void update();
        List<AircraftActions> GetResults();

        void reflectWorldToInstruments(float f, FlightVariables fv);


        
    }
}
