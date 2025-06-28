using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;

namespace Simple_Shell__OS_
{
    class Virtual_Disk
    {
        public static readonly string VirtualDiskPath = "VirtualDisk.txt";

        static public void Initialize_Virtual_Disk()
        {
            try
            {
                if (!File.Exists(VirtualDiskPath))
                {
                    CreateNewVirtualDisk();
                }
                else
                {
                    LoadExistingVirtualDisk();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing virtual disk: {ex.Message}");
            }
        }

        private static void CreateNewVirtualDisk()
        {
            using (StreamWriter vd = new StreamWriter(VirtualDiskPath))
            {
                // Super Block (all zeros)
                vd.Write(new string('0', 1024));

                // FAT Table (all asterisks)
                vd.Write(new string('*', 4 * 1024));

                // Data Blocks (all hashes)
                vd.Write(new string('#', 1019 * 1024));
            }

            FAT.Initialize_FAT_Table();
            Directory Root = new Directory("B:", 0x10, 5, null);
            Root.Write_Directory();
            FAT.Write_FAT_Table();
            Program.Current_Directory = Root;
            Program.Current_Path = new string(Program.Current_Directory.File_Name).Trim();
        }

        private static void LoadExistingVirtualDisk()
        {
            FAT.Get_FAT_Table();
            Directory Root = new Directory("B:", 0x10, 5, null);
            Root.Read_Directory();
            Program.Current_Directory = Root;
            Program.Current_Path = new string(Program.Current_Directory.File_Name).Trim();
        }

        static public void Write_Block(byte[] data, int index)
        {
            if (data == null || data.Length != 1024)
            {
                throw new ArgumentException("Data block must be exactly 1024 bytes");
            }

            try
            {
                using (FileStream fwb = new FileStream(VirtualDiskPath, FileMode.Open, FileAccess.Write))
                {
                    fwb.Seek(1024 * index, SeekOrigin.Begin);
                    fwb.Write(data, 0, 1024);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing block {index}: {ex.Message}");
                throw;
            }
        }

        static public byte[] Read_Block(int index)
        {
            try
            {
                using (FileStream frb = new FileStream(VirtualDiskPath, FileMode.Open, FileAccess.Read))
                {
                    frb.Seek(1024 * index, SeekOrigin.Begin);
                    byte[] rblock = new byte[1024];
                    frb.Read(rblock, 0, 1024);
                    return rblock;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading block {index}: {ex.Message}");
                throw;
            }
        }
    }
}
