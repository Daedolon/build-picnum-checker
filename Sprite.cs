using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUILD_Picnum_Checker
{
    class Sprite
    {
        public int Number;

        public byte[] X;
        public byte[] Y;
        public byte[] Z;

        public byte[] Cstat;
        public byte[] Picnum;
        public byte[] Palette;
        
        public byte[] Unknown;
        
        public byte[] Ang;

        public byte[] Unknown2;

        public Sprite()
        {
            X = new byte[4];
            Y = new byte[4];
            Z = new byte[4];

            Cstat = new byte[2];
            Picnum = new byte[2];
            Palette = new byte[2];

            Unknown = new byte[10];

            Ang = new byte[2];

            Unknown2 = new byte[14];
        }
    }
}
