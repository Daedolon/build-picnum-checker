using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUILD_Picnum_Checker
{
    public class Item
    {
        public Game Game;

        public int Picnum;
        public string Name;

        public Group Group;
    }

    public enum Game
    {
        Duke_Nukem_3D,
        Blood,
        Shadow_Warrior,
        Redneck_Rampage,
        Ion_Fury
    }
    public enum Group
    {
        Weapons,
        Ammo,
        Items,
        Enemies,
        Effects,
        Props
    }
}
