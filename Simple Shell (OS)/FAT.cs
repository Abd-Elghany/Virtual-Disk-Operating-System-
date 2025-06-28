using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Shell__OS_
{
    class FAT
    {
        public static int[] FAT_Table = new int[1024];

        static public void Initialize_FAT_Table()
        {
            for (int i = 0; i < 1024; i++)
            {
                FAT_Table[i] = (i < 5) ? -1 : 0;
            }
        }

        static public void Write_FAT_Table()
        {
            try
            {
                using (FileStream fw = new FileStream(Virtual_Disk.VirtualDiskPath, FileMode.Open, FileAccess.Write))
                {
                    fw.Seek(1024, SeekOrigin.Begin);
                    byte[] W_FAT_Table = new byte[4096];
                    Buffer.BlockCopy(FAT_Table, 0, W_FAT_Table, 0, 4096);
                    fw.Write(W_FAT_Table, 0, 4096);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing FAT table: {ex.Message}");
                throw;
            }
        }

        static public void Get_FAT_Table()
        {
            try
            {
                Initialize_FAT_Table();
                using (FileStream fr = new FileStream(Virtual_Disk.VirtualDiskPath, FileMode.Open, FileAccess.Read))
                {
                    fr.Seek(1024, SeekOrigin.Begin);
                    byte[] rb_FAT_Table = new byte[4096];
                    fr.Read(rb_FAT_Table, 0, 4096);
                    Buffer.BlockCopy(rb_FAT_Table, 0, FAT_Table, 0, 4096);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading FAT table: {ex.Message}");
                throw;
            }
        }

        static public int Get_Available_Block()
        {
            for (int i = 5; i < 1024; i++)
            {
                if (FAT_Table[i] == 0)
                    return i;
            }
            return -1;
        }

        static public int Get_Next_Block(int index)
        {
            if (index >= 0 && index < FAT_Table.Length)
                return FAT_Table[index];
            return -1;
        }

        static public void Set_Next_Block(int index, int val)
        {
            if (index >= 0 && index < FAT_Table.Length)
                FAT_Table[index] = val;
        }

        static public int Get_Available_Blocks()
        {
            int counter = 0;
            for (int i = 5; i < 1024; i++)
            {
                if (FAT_Table[i] == 0)
                    counter++;
            }
            return counter;
        }

        public static int Get_Free_Space()
        {
            return Get_Available_Blocks() * 1024;
        }
    }
}
