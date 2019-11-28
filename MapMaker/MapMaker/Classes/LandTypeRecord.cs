using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MapMaker
{
    // IL2 mapping
    //
    // 0  - 3   Low land
    // 4  - 7   Midland
    // 8  - 11  Mountain
    // 12 - 15  Country
    // 16 - 19  City
    // 20 - 23  Airfield
    // 24 - 27  Wood
    // 28 - 31  Water
    //
    // My mapping
    // 0    Lowland near water
    // 1    Desert
    // 2    Dry scrub
    // 3    Grass
    // 4    Hill
    // 5    Farmland
    // 6    Tropical savannah
    // 7    Swamp
    // 8    Snow
    // 9    Mountain
    // 10
    // 11
    // 12   Steppe
    // 13   Mixed scrub
    // 14   Desert scrub
    // 15   Sand scrub
    // 16   City
    // 17
    // 18
    // 19
    // 20
    // 21
    // 22
    // 23
    // 24   Forest
    // 25
    // 26
    // 27   Jungle
    // 28   Sea
    // 29   Water in land 
    // 30   Lake
    // 31
    //
    public class LandTypeRecord
    {
        public byte Red;
        public byte Green;
        public byte Blue;
        public String Type;

        public LandTypeRecord(byte r, byte g, byte b, string s)
        {
            Red = r;
            Green = g;
            Blue = b;
            Type = s;

        }

        public Int32 ToInt32()
        {
            return (Int32)(0x40000000 + (Red << 16) + (Green << 8) + Blue);
        }
    }
}
