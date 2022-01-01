using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace IL2Modder.IL2
{
    public class AircraftActions
    {
        public String name;
        public int type;
        public float y;
        public float p;
        public float r;
        public bool visible;

    };
    public interface AircraftInterface
    {
        void update(String [] keys);
        void moveGear(float f);
        void moveRudder(float f);
        void doMurderPilot(int i);
        void moveBayDoor(float f);
        void moveWingFold(float f);
        void moveElevator(float f);
        void moveFlap(float f);
        void moveCockpitDoor(float f);
        void moveAirBrake(float f);
        void moveAileron(float f);
        void moveArrestorHook(float f);
        void moveFan(float f);
        void onLoad();
        

        bool turretAngles(int paramInt, float[] paramArrayOfFloat); 

        List<AircraftActions> GetResults();
        
    }
}
