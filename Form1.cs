using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO; // File
using System.Diagnostics;
using System.Windows.Forms.VisualStyles;
using System.Runtime.Remoting.Messaging;

namespace BUILD_Picnum_Checker
{
    public partial class Form1 : Form
    {
        // Constants
        private static int SectorStruct = 40;
        private static int WallStruct = 32;
        private static int SpriteStruct = 44;

        // Variables
        private static string ProgramName = "BUILD Picnum Checker";

        private static string CurrentFilePath;
        
        private static byte[] CurrentFilesz;
        private static byte[] MapHeader;
        
        private static Map CurrentMap;
        private static Sprite CurrentSprite;
        private static List<Sprite> CurrentSprites = new List<Sprite>();


        public Form1()
        {
            InitializeComponent();

            this.AllowDrop = true;

            this.DragDrop += new DragEventHandler(Form1_DragDrop);
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //
            toolStripLabel1.Text = "No .MAP loaded";
        }


        private void Form1_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }


        private void Form1_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (files.Length > 0)
            {
                if (File.Exists(files[0]))
                {
                    // Save file path
                    CurrentFilePath = files[0];

                    // Update stuff
                    Update();
                }
            }
        }


        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }


        // Reads the entire BUILD map into memory, lol
        private void ReadMap(string FileName)
        {
            // Read the map into memory
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);

            //
            CurrentMap = new Map();

            try
            {
                // Read map
                fs.Read(CurrentMap.mapVersion, 0, 4);
                
                fs.Read(CurrentMap.playerX, 0, 4);
                fs.Read(CurrentMap.playerY, 0, 4);
                fs.Read(CurrentMap.playerZ, 0, 4);
                
                fs.Read(CurrentMap.playerAng, 0, 2);
                fs.Read(CurrentMap.playerSect, 0, 2);
                
                fs.Read(CurrentMap.numSectors, 0, 2);
                int numSectors = SectorStruct * BitConverter.ToInt16(CurrentMap.numSectors, 0);
                byte[] tempSectors = new byte[numSectors];
                fs.Read(tempSectors, 0, numSectors);
                CurrentMap.Sectors = tempSectors.ToList();

                fs.Read(CurrentMap.numWalls, 0, 2);
                int numWalls = WallStruct * BitConverter.ToInt16(CurrentMap.numWalls, 0);
                byte[] tempWalls = new byte[numWalls];
                fs.Read(tempWalls, 0, numWalls);
                CurrentMap.Walls = tempWalls.ToList();

                fs.Read(CurrentMap.numSprites, 0, 2);

                // Console.WriteLine("Sprite struct start position " + fs.Position);

                int numSprites = SpriteStruct * BitConverter.ToInt16(CurrentMap.numSprites, 0);
                byte[] tempSprites = new byte[numSprites];
                fs.Read(tempSprites, 0, numSprites);
                CurrentMap.Sprites = tempSprites.ToList();

                
                //
                Console.WriteLine(BitConverter.ToString(CurrentMap.Sprites.ToArray()).Replace("-", " "));


                toolStripLabel1.Text = "Found " + BitConverter.ToInt16(CurrentMap.numSprites, 0) + " total sprites.";
            }
            finally
            {
                fs.Close();
            }

            // Read sprites into memory
            //FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            MemoryStream ms = null;
            CurrentSprites = new List<Sprite>();
            
            // Loop through all the sprites in the map
            for (int i = 0; i < BitConverter.ToInt16(CurrentMap.numSprites, 0); i++)
            {
                try
                {
                    using (ms = new MemoryStream(CurrentMap.Sprites.ToArray()))
                    {
                        // Yoo
                        CurrentSprite = new Sprite();

                        CurrentSprite.Number = i;

                        // Seek the position
                        int SpritePos = i * SpriteStruct;
                        ms.Seek(SpritePos, SeekOrigin.Current);

                        // Read the sprite struct
                        ms.Read(CurrentSprite.X, 0, 4);
                        ms.Read(CurrentSprite.Y, 0, 4);
                        ms.Read(CurrentSprite.Z, 0, 4);

                        ms.Read(CurrentSprite.Cstat, 0, 2);
                        ms.Read(CurrentSprite.Picnum, 0, 2);
                        ms.Read(CurrentSprite.Palette, 0, 2);

                        ms.Read(CurrentSprite.Unknown, 0, 10);

                        ms.Read(CurrentSprite.Ang, 0, 2);

                        ms.Read(CurrentSprite.Unknown2, 0, 14);

                        // Yo
                        // Console.WriteLine("Sprite #" + i + " picnum " + BitConverter.ToInt16(CurrentSprite.Picnum, 0).ToString());

                        // Add the complete sprite struct to list
                        CurrentSprites.Add(CurrentSprite);
                    }
                }
                finally
                {
                    ms.Close();
                }
            }
        }


        private void ReadHeader(string FileName)
        {
            // Read the entire header because faster? lazy?
            MapHeader = ReadBytes(FileName, 0, 42);

            Console.WriteLine(MapHeader.Length);

            // public static void Copy (Array sourceArray, long sourceIndex, Array destinationArray, long destinationIndex, long length);

            // Read the current bytes in memory as a map header
            /*
            CurrentMap = new Map()
            {
                playerX = ReadBytes
            }
            */
        }


        private void Update()
        {
            // Change statuses
            //textBox1.Text = Path.GetFileName(CurrentFilePath);
            textBox1.Text = CurrentFilePath;
            toolStripLabel1.Text = "Loaded " + Path.GetFileName(CurrentFilePath);

            // Make sure it's a build map!
            // if (IsBuildMap(CurrentFilePath))
            if (File.Exists(CurrentFilePath))
            {
                // Read everything already?
                ReadMap(CurrentFilePath);
            }
            else
            {
                toolStripLabel1.Text = "Not a valid .MAP file";
            }

            // Clear listbox for populating
            // listBox1.Items.Clear();
            ListSprites(numericUpDown1.Value);

            // textBox1.Text = CurrentFile.Length.ToString();
        }
        

        // Returns true if loaded file is an actual build map
        private bool IsBuildMap(string FileName)
        {
            if (File.Exists(FileName))
            {
                FileInfo fi = new FileInfo(CurrentFilePath);

                // if ()
                return true;
            }
            else
                return false;

            // if (CurrentFilePath)
        }


        // Read specific amount of bytes at a specific point in a given file
        private byte[] ReadBytes(string FileName, int Offset, int Bytes)
        {
            byte[] buffer;
            
            FileStream fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            
            try
            {
                buffer = new byte[Bytes];
                
                fileStream.Read(buffer, Offset, Bytes);
            }
            finally
            {
                fileStream.Close();
            }
            
            return buffer;
        }


        // Read the entire file to memory
        private byte[] ReadFile(string FileName)
        {
            // This is a very memory heavy and an unoptimized way of reading a file into memory!
            return File.ReadAllBytes(FileName);
        }


        private void ListSprites(decimal Input)
        {
            int Picnum = (int)Input;

            if (Picnum > -1 && CurrentSprites.Count > 0)
            {
                // Clear the list
                listView1.Items.Clear();

                // Add index
                // listView1.Items.Add("#\tX\tY\tZ\tPalette");

                // List all found ones on the list
                foreach (Sprite sprite in CurrentSprites)
                {
                    if (Picnum == BitConverter.ToInt16(sprite.Picnum, 0))
                    {
                        ListViewItem Item = new ListViewItem(new string[] {
                            sprite.Number.ToString(),
                            BitConverter.ToInt32(sprite.X, 0).ToString(),
                            BitConverter.ToInt32(sprite.Y, 0).ToString(),
                            BitConverter.ToInt32(sprite.Z, 0).ToString(),
                            BitConverter.ToInt16(sprite.Palette, 0).ToString()
                        });
                        
                        /*
                        string Item = sprite.Number + ".\t" +
                            BitConverter.ToInt16(sprite.X, 0) + "\t" +
                            BitConverter.ToInt16(sprite.Y, 0) + "\t" +
                            BitConverter.ToInt16(sprite.Z, 0) + "\t" +
                            BitConverter.ToInt16(sprite.Palette, 0);
                        */

                        listView1.Items.Add(Item);
                    }
                }

                // Display number of matchs found
                toolStripLabel1.Text = "Found " + listView1.Items.Count + " entries.";
            }
            else
            {
                toolStripLabel1.Text = "No matching sprites found.";
            }
        }


        // 
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // Only allow numbers!
            if (System.Text.RegularExpressions.Regex.IsMatch(textBox1.Text, "  ^ [0-9]"))
            {
                // ListSprites(textBox2.Text);
                // textBox1.Text = "";
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            ListSprites(numericUpDown1.Value);
        }


        //
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Change the contents of listView2
            UpdateExamples(comboBox1.SelectedItem.ToString());
        }

        private void PopulateItems(Game Game)
        {
            // Clear
            listView2.Items.Clear();

            // Items
            List<Item> Items = new List<Item>()
            {
                // Duke Nukem 3D
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 21, Name = "Pistol", Group = Group.Weapons },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 28, Name = "Shotgun", Group = Group.Weapons },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 22, Name = "Ripper", Group = Group.Weapons },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 23, Name = "RPG", Group = Group.Weapons },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 26, Name = "Pipebomb", Group = Group.Weapons },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 47, Name = "Pipebomb Box", Group = Group.Weapons },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 25, Name = "Shrinker", Group = Group.Weapons },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 32, Name = "Expander", Group = Group.Weapons },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 29, Name = "Devastator", Group = Group.Weapons },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 27, Name = "Tripbomb", Group = Group.Weapons },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 24, Name = "FreezeThrower", Group = Group.Weapons },

                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 40, Name = "Pistol Magazine", Group = Group.Ammo },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 49, Name = "Shotgun Shells", Group = Group.Ammo },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 41, Name = "Ripper Ammo Box", Group = Group.Ammo },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 44, Name = "RPG Rocket Box", Group = Group.Ammo },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 26, Name = "Pipebomb", Group = Group.Ammo },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 47, Name = "Pipebomb Box", Group = Group.Ammo },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 46, Name = "Shrinker Crystal", Group = Group.Ammo },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 45, Name = "Expander Ammo", Group = Group.Ammo },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 42, Name = "Devastator Rocket Box", Group = Group.Ammo },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 27, Name = "Tripbomb", Group = Group.Ammo },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 37, Name = "FreezeThrower Ammo", Group = Group.Ammo },
                
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 51, Name = "Small Medkit +10", Group = Group.Items },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 52, Name = "Large Medkit +30", Group = Group.Items },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 100, Name = "Atomic Health", Group = Group.Items },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 54, Name = "Armor", Group = Group.Items },
                
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 53, Name = "Portable Medkit", Group = Group.Items },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 55, Name = "Steroids", Group = Group.Items },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 1348, Name = "Holoduke", Group = Group.Items },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 57, Name = "Jetpack", Group = Group.Items },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 59, Name = "Night Vision Goggles", Group = Group.Items },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 56, Name = "Scuba Gear", Group = Group.Items },
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 61, Name = "Protective Boots", Group = Group.Items },
                
                new Item() { Game = Game.Duke_Nukem_3D, Picnum = 60, Name = "Access Card", Group = Group.Items },

                // Redneck Rampage
                new Item() { Game = Game.Redneck_Rampage, Picnum = 21, Name = "Revolver", Group = Group.Weapons },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 28, Name = "Scattergun", Group = Group.Weapons },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 22, Name = "Huntin' Rifle", Group = Group.Weapons },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 26, Name = "Dyn-O-Mite", Group = Group.Weapons },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 47, Name = "Dyn-O-Mite Box", Group = Group.Weapons },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 23, Name = "Crossbow", Group = Group.Weapons },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 25, Name = "Ripsaw", Group = Group.Weapons },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 29, Name = "Alien Arm Gun", Group = Group.Weapons },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 27, Name = "Powder Keg", Group = Group.Weapons },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 24, Name = "Alien Teat Gun", Group = Group.Weapons },

                new Item() { Game = Game.Redneck_Rampage, Picnum = 40, Name = "Speedloader", Group = Group.Ammo },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 49, Name = "Scattergun Shells", Group = Group.Ammo },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 41, Name = "Rifle Ammo", Group = Group.Ammo },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 26, Name = "Dyn-O-Mite", Group = Group.Ammo },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 47, Name = "Dyn-O-Mite Box", Group = Group.Ammo },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 43, Name = "Ripsaw Ammo", Group = Group.Ammo },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 42, Name = "Alien Arm Gun Ammo", Group = Group.Ammo },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 27, Name = "Powder Keg", Group = Group.Ammo },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 32, Name = "Teat Gun Ammo", Group = Group.Ammo },
                
                new Item() { Game = Game.Redneck_Rampage, Picnum = 51, Name = "Beer", Group = Group.Items },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 52, Name = "Large Pork Rinds", Group = Group.Items },

                new Item() { Game = Game.Redneck_Rampage, Picnum = 53, Name = "Cheap Ass Whiskey", Group = Group.Items },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 55, Name = "XXX Moonshine", Group = Group.Items },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 53, Name = "Beer (Sixpack)", Group = Group.Items },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 57, Name = "Cow Pie", Group = Group.Items },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 56, Name = "Vacuum Cleaner Snorkle", Group = Group.Items },
                new Item() { Game = Game.Redneck_Rampage, Picnum = 61, Name = "Hip Waders", Group = Group.Items },

                new Item() { Game = Game.Redneck_Rampage, Picnum = 60, Name = "Skeleton Key", Group = Group.Items },

                // Shadow Warrior
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1793, Name = "Shurikens", Group = Group.Weapons },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1794, Name = "Riot Gun", Group = Group.Weapons },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1797, Name = "UZI Submachine Gun", Group = Group.Weapons },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1807, Name = "UZI Submachine Gun (Floor)", Group = Group.Weapons },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1818, Name = "Missile Launcher", Group = Group.Weapons },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1817, Name = "Grenade Launcher", Group = Group.Weapons },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1842, Name = "Sticky Bombs", Group = Group.Weapons },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1811, Name = "Railgun", Group = Group.Weapons },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1814, Name = "Guardian Head", Group = Group.Weapons },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1824, Name = "Ripper Heart", Group = Group.Weapons },

                new Item() { Game = Game.Shadow_Warrior, Picnum = 1793, Name = "Shurikens", Group = Group.Ammo },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1823, Name = "Shotshells", Group = Group.Ammo },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1799, Name = "UZI Clip", Group = Group.Ammo },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1800, Name = "Missiles", Group = Group.Ammo },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1819, Name = "Heat Seeker Card", Group = Group.Ammo },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1809, Name = "Nuclear Warhead", Group = Group.Ammo },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1831, Name = "Grenade Shells", Group = Group.Ammo },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1842, Name = "Sticky Bombs", Group = Group.Ammo },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1812, Name = "Railgun Rods", Group = Group.Ammo },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1814, Name = "Guardian Head", Group = Group.Ammo },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1824, Name = "Ripper Heart", Group = Group.Ammo },

                new Item() { Game = Game.Shadow_Warrior, Picnum = 1802, Name = "Medkit +20", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1810, Name = "Fortune Cookie (+50)", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 3030, Name = "Armor Vest", Group = Group.Items },

                new Item() { Game = Game.Shadow_Warrior, Picnum = 1803, Name = "Portable Medkit", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1813, Name = "Repair Kit", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1804, Name = "Smoke Bomb", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 3031, Name = "Night Vision", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1808, Name = "Gas Bomb", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1805, Name = "Flash Bomb", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1829, Name = "Caltrops", Group = Group.Items },

                new Item() { Game = Game.Shadow_Warrior, Picnum = 1765, Name = "Gold Master Key", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1766, Name = "Blue Key", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1767, Name = "Blue Key (card)", Group = Group.Items },

                new Item() { Game = Game.Shadow_Warrior, Picnum = 1769, Name = "Silver Master Key", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1770, Name = "Red Key", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1771, Name = "Red Key (card)", Group = Group.Items },

                new Item() { Game = Game.Shadow_Warrior, Picnum = 1773, Name = "Bronze Master Key", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1774, Name = "Green Key", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1775, Name = "Green Key (card)", Group = Group.Items },

                new Item() { Game = Game.Shadow_Warrior, Picnum = 1777, Name = "Red Master Key", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1778, Name = "Yellow Key", Group = Group.Items },
                new Item() { Game = Game.Shadow_Warrior, Picnum = 1779, Name = "Yellow Key (card)", Group = Group.Items },

                // Blood
                new Item() { Game = Game.Blood, Picnum = 524, Name = "Flare Pistol", Group = Group.Weapons },
                new Item() { Game = Game.Blood, Picnum = 559, Name = "Sawed-Off", Group = Group.Weapons },
                new Item() { Game = Game.Blood, Picnum = 558, Name = "Tommy Gun", Group = Group.Weapons },
                new Item() { Game = Game.Blood, Picnum = 526, Name = "Napalm Launcher", Group = Group.Weapons },
                new Item() { Game = Game.Blood, Picnum = 589, Name = "Bundle of TNT", Group = Group.Weapons },
                new Item() { Game = Game.Blood, Picnum = 809, Name = "Case of TNT", Group = Group.Weapons },
                new Item() { Game = Game.Blood, Picnum = 811, Name = "Proximity Detonator", Group = Group.Weapons },
                new Item() { Game = Game.Blood, Picnum = 810, Name = "Remote Detonator", Group = Group.Weapons },
                new Item() { Game = Game.Blood, Picnum = 618, Name = "Spray Can", Group = Group.Weapons },
                new Item() { Game = Game.Blood, Picnum = 618, Name = "Tesla Cannon ", Group = Group.Weapons },
                new Item() { Game = Game.Blood, Picnum = 800, Name = "Life Leech", Group = Group.Weapons },
                new Item() { Game = Game.Blood, Picnum = 525, Name = "Voodoo Doll", Group = Group.Weapons },

                new Item() { Game = Game.Blood, Picnum = 816, Name = "Flares", Group = Group.Ammo },
                new Item() { Game = Game.Blood, Picnum = 619, Name = "4 Shotgun Shells", Group = Group.Ammo },
                new Item() { Game = Game.Blood, Picnum = 812, Name = "Box of Shotgun Shells", Group = Group.Ammo },
                new Item() { Game = Game.Blood, Picnum = 814, Name = "A Few Bullets", Group = Group.Ammo },
                new Item() { Game = Game.Blood, Picnum = 548, Name = "Full Drum of Bullets", Group = Group.Ammo },
                new Item() { Game = Game.Blood, Picnum = 801, Name = "Gasoline Can", Group = Group.Ammo },
                new Item() { Game = Game.Blood, Picnum = 589, Name = "Bundle of TNT", Group = Group.Ammo },
                new Item() { Game = Game.Blood, Picnum = 809, Name = "Case of TNT", Group = Group.Ammo },
                new Item() { Game = Game.Blood, Picnum = 811, Name = "Proximity Detonator", Group = Group.Ammo },
                new Item() { Game = Game.Blood, Picnum = 810, Name = "Remote Detonator", Group = Group.Ammo },
                new Item() { Game = Game.Blood, Picnum = 618, Name = "Spray Can", Group = Group.Ammo },
                new Item() { Game = Game.Blood, Picnum = 548, Name = "Tesla Charge", Group = Group.Ammo },
                new Item() { Game = Game.Blood, Picnum = 548, Name = "Trapped Soul", Group = Group.Ammo },
                new Item() { Game = Game.Blood, Picnum = 525, Name = "Voodoo Doll", Group = Group.Ammo },

                new Item() { Game = Game.Blood, Picnum = 822, Name = "Medicine Pouch", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 2169, Name = "Life Essence", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 2433, Name = "Life Seed", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 2628, Name = "Basic Armour", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 2586, Name = "Body Armour", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 2578, Name = "Fire Armour", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 2594, Name = "Super Armour", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 896, Name = "Cloak of Invisibility", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 768, Name = "Cloak of Shadow", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 2428, Name = "Reflective Shots", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 829, Name = "Guns Akimbo", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 825, Name = "Deathmask", Group = Group.Items },

                new Item() { Game = Game.Blood, Picnum = 519, Name = "Doctor's Bag", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 830, Name = "Diving Suit", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 760, Name = "Crystal Ball", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 839, Name = "Beast Vision", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 827, Name = "Jumping Boots", Group = Group.Items },

                new Item() { Game = Game.Blood, Picnum = 2552, Name = "Skull Key", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 2553, Name = "Eye Key", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 2554, Name = "Fire Key", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 2555, Name = "Dagger Key", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 2556, Name = "Spider Key", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 2557, Name = "Moon Key", Group = Group.Items },
                new Item() { Game = Game.Blood, Picnum = 2558, Name = "Key 7", Group = Group.Items },

                // Ion Fury
                new Item() { Game = Game.Ion_Fury, Picnum = 209, Name = "Electrifryer", Group = Group.Weapons },
                new Item() { Game = Game.Ion_Fury, Picnum = 210, Name = "Loverboy", Group = Group.Weapons },
                new Item() { Game = Game.Ion_Fury, Picnum = 224, Name = "Shotgun", Group = Group.Weapons },
                new Item() { Game = Game.Ion_Fury, Picnum = 230, Name = "Penetrator", Group = Group.Weapons },
                new Item() { Game = Game.Ion_Fury, Picnum = 141, Name = "Chaingun", Group = Group.Weapons },
                new Item() { Game = Game.Ion_Fury, Picnum = 232, Name = "Bowling Bomb (Pickup)", Group = Group.Weapons },
                new Item() { Game = Game.Ion_Fury, Picnum = 6786, Name = "Bowling Bomb (Active)", Group = Group.Weapons },
                new Item() { Game = Game.Ion_Fury, Picnum = 6800, Name = "Bowling Bomb (Item)", Group = Group.Weapons },
                new Item() { Game = Game.Ion_Fury, Picnum = 231, Name = "Grenade Launcher", Group = Group.Weapons },
                new Item() { Game = Game.Ion_Fury, Picnum = 220, Name = "Ion Bow", Group = Group.Weapons },
                new Item() { Game = Game.Ion_Fury, Picnum = 222, Name = "Clusterpuck", Group = Group.Weapons },
                
                new Item() { Game = Game.Ion_Fury, Picnum = 229, Name = "Ammo for Loverboy", Group = Group.Ammo },
                new Item() { Game = Game.Ion_Fury, Picnum = 211, Name = "Ammo for Loverboy (Box)", Group = Group.Ammo },
                new Item() { Game = Game.Ion_Fury, Picnum = 225, Name = "Shells for Disperser", Group = Group.Ammo },
                new Item() { Game = Game.Ion_Fury, Picnum = 227, Name = "Flechettes for Penetrator", Group = Group.Ammo },
                new Item() { Game = Game.Ion_Fury, Picnum = 211, Name = "Bullets for Chaingun", Group = Group.Ammo },
                new Item() { Game = Game.Ion_Fury, Picnum = 232, Name = "Bowling Bomb (Pickup)", Group = Group.Ammo },
                new Item() { Game = Game.Ion_Fury, Picnum = 6786, Name = "Bowling Bomb (Active)", Group = Group.Ammo },
                new Item() { Game = Game.Ion_Fury, Picnum = 6800, Name = "Bowling Bomb (Item)", Group = Group.Ammo },
                new Item() { Game = Game.Ion_Fury, Picnum = 226, Name = "Grenades for Disperser", Group = Group.Ammo },
                new Item() { Game = Game.Ion_Fury, Picnum = 232, Name = "Quiver for Ion Bow", Group = Group.Ammo },
                new Item() { Game = Game.Ion_Fury, Picnum = 222, Name = "Clusterpuck", Group = Group.Ammo },

                new Item() { Game = Game.Ion_Fury, Picnum = 233, Name = "Emergency Syringe", Group = Group.Items },
                new Item() { Game = Game.Ion_Fury, Picnum = 212, Name = "Stim Pack +10", Group = Group.Items },
                new Item() { Game = Game.Ion_Fury, Picnum = 213, Name = "Health Kit +25", Group = Group.Items },
                new Item() { Game = Game.Ion_Fury, Picnum = 214, Name = "Portable Medkit", Group = Group.Items },
                new Item() { Game = Game.Ion_Fury, Picnum = 228, Name = "Armor Fragment", Group = Group.Items },
                new Item() { Game = Game.Ion_Fury, Picnum = 215, Name = "Light Armor (+50)", Group = Group.Items },
                new Item() { Game = Game.Ion_Fury, Picnum = 216, Name = "Medium Armor (+100)", Group = Group.Items },
                new Item() { Game = Game.Ion_Fury, Picnum = 223, Name = "Heavy Armor (+200)", Group = Group.Items },
                new Item() { Game = Game.Ion_Fury, Picnum = 242, Name = "Ultrasonic Radar", Group = Group.Items },
                new Item() { Game = Game.Ion_Fury, Picnum = 243, Name = "Jump Boots", Group = Group.Items },
                new Item() { Game = Game.Ion_Fury, Picnum = 244, Name = "Super Damage", Group = Group.Items },
                new Item() { Game = Game.Ion_Fury, Picnum = 241, Name = "Blast Accelerator", Group = Group.Items },
                new Item() { Game = Game.Ion_Fury, Picnum = 240, Name = "Hazard Suit", Group = Group.Items },
                new Item() { Game = Game.Ion_Fury, Picnum = 217, Name = "Access Card", Group = Group.Items },
            };

            // Add items
            foreach (Item item in Items)
            {
                if (item.Game == Game)
                {
                    // Generate listview item
                    ListViewItem lvi = new ListViewItem(new string[] { item.Picnum.ToString(), item.Name });

                    // Add item
                    listView2.Items.Add(lvi);

                    // Group item
                    switch (item.Group)
                    {
                        case Group.Weapons:
                            listView2.Items[listView2.Items.Count - 1].Group = listView2.Groups[0];
                            break;
                        case Group.Ammo:
                            listView2.Items[listView2.Items.Count - 1].Group = listView2.Groups[1];
                            break;
                        case Group.Items:
                            listView2.Items[listView2.Items.Count - 1].Group = listView2.Groups[2];
                            break;
                        case Group.Enemies:
                            listView2.Items[listView2.Items.Count - 1].Group = listView2.Groups[3];
                            break;
                        case Group.Props:
                            listView2.Items[listView2.Items.Count - 1].Group = listView2.Groups[4];
                            break;
                        case Group.Effects:
                            listView2.Items[listView2.Items.Count - 1].Group = listView2.Groups[5];
                            break;
                    }
                }
            }
        }


        private void UpdateExamples(string GameName)
        {
            // Empty the list
            listView2.Items.Clear();

            // Populate the list based on the selection, this is really generic but maybe it works
            switch (GameName)
            {
                case "Duke Nukem 3D":
                    PopulateItems(Game.Duke_Nukem_3D);
                    break;
                case "Blood":
                    PopulateItems(Game.Blood);
                    break;
                case "Shadow Warrior":
                    PopulateItems(Game.Shadow_Warrior);
                    break;
                case "Redneck Rampage":
                    PopulateItems(Game.Redneck_Rampage);
                    break;
                case "Ion Fury":
                    PopulateItems(Game.Ion_Fury);
                    break;
            }
        }

        private void listView2_ItemSelectionChanged(Object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                // Bounce our selection to the Picnum selector
                numericUpDown1.Value = Int16.Parse(listView2.SelectedItems[0].Text);
            }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        /*
        private void listView2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems.OfType<ListViewItem>())
            {
                Trace.WriteLine("ListViewItem Selected");
                item.IsSelected = false;
            }
        }

        private void MySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("Selected: {0}", e.AddedItems[0]);
        }
        */
    }
    }
