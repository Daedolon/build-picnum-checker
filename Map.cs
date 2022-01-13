using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUILD_Picnum_Checker
{
    class Map
    {
        public byte[] mapVersion;
        
        public byte[] playerX;
        public byte[] playerY;
        public byte[] playerZ;
        
        public byte[] playerAng;
        public byte[] playerSect;
        
        public byte[] numSectors;
        public List<byte> Sectors;
        
        public byte[] numWalls;
        public List<byte> Walls;

        public byte[] numSprites;
        public List<byte> Sprites;

        public Map()
        {
            mapVersion = new byte[4];
            
            playerX = new byte[4];
            playerY = new byte[4];
            playerZ = new byte[4];
            
            playerAng = new byte[2];
            playerSect = new byte[2];

            numSectors = new byte[2];
            Sectors = new List<byte>();
            
            numWalls = new byte[2];
            Walls = new List<byte>();
            
            numSprites = new byte[2];
            Sprites = new List<byte>();
        }
    }
}
