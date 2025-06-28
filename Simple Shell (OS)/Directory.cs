using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Shell__OS_
{
    class Directory : Directory_Entry
    {
        public List<Directory_Entry> Directory_Table;
        public Directory Parent;

        public Directory(string name, byte f_attribute, int f_Cluster, Directory par)
            : base(name, f_attribute, f_Cluster)
        {
            Directory_Table = new List<Directory_Entry>();
            Parent = par;
        }

        public Directory() : this("", 0x10, 0, null) { }

        public void Write_Directory()
        {
            byte[] directoryBytes = new byte[32 * Directory_Table.Count];

            for (int i = 0; i < Directory_Table.Count; i++)
            {
                byte[] entryBytes = Directory_Table[i].Get_Bytes();
                Array.Copy(entryBytes, 0, directoryBytes, i * 32, 32);
            }

            int requiredBlocks = (int)Math.Ceiling(directoryBytes.Length / 1024.0);
            int remainingBytes = directoryBytes.Length % 1024;

            if (requiredBlocks > FAT.Get_Available_Blocks())
            {
                throw new Exception("Not enough space to write directory");
            }

            int firstBlock;
            if (First_Cluster != 0)
            {
                firstBlock = First_Cluster;
            }
            else
            {
                firstBlock = FAT.Get_Available_Block();
                if (firstBlock == -1) throw new Exception("No available blocks");
                First_Cluster = firstBlock;
            }

            List<byte[]> blocks = new List<byte[]>();
            for (int b = 0; b < requiredBlocks - (remainingBytes > 0 ? 1 : 0); b++)
            {
                byte[] block = new byte[1024];
                Array.Copy(directoryBytes, b * 1024, block, 0, 1024);
                blocks.Add(block);
            }

            if (remainingBytes > 0)
            {
                byte[] lastBlock = new byte[1024];
                int startIndex = (requiredBlocks - 1) * 1024;
                Array.Copy(directoryBytes, startIndex, lastBlock, 0, remainingBytes);
                blocks.Add(lastBlock);
            }

            int currentBlock = firstBlock;
            int lastBlockIndex = -1;

            for (int i = 0; i < blocks.Count; i++)
            {
                Virtual_Disk.Write_Block(blocks[i], currentBlock);
                FAT.Set_Next_Block(currentBlock, -1);

                if (lastBlockIndex != -1)
                {
                    FAT.Set_Next_Block(lastBlockIndex, currentBlock);
                }

                lastBlockIndex = currentBlock;

                if (i < blocks.Count - 1)
                {
                    currentBlock = FAT.Get_Available_Block();
                    if (currentBlock == -1)
                    {
                        break;
                    }
                }
            }

            if (Parent != null)
            {
                Parent.Update_Directory_Content(this.Get_Directory_Entry());
            }

            FAT.Write_FAT_Table();
        }

        public void Read_Directory()
        {
            Directory_Table = new List<Directory_Entry>();
            List<byte> allBytes = new List<byte>();

            if (First_Cluster != 0)
            {
                int currentBlock = First_Cluster;
                int nextBlock = FAT.Get_Next_Block(currentBlock);

                do
                {
                    byte[] blockData = Virtual_Disk.Read_Block(currentBlock);
                    allBytes.AddRange(blockData);

                    currentBlock = nextBlock;
                    if (currentBlock != -1)
                    {
                        nextBlock = FAT.Get_Next_Block(currentBlock);
                    }
                } while (currentBlock != -1);

                byte[] entryBytes = new byte[32];
                for (int i = 0; i < allBytes.Count; i++)
                {
                    entryBytes[i % 32] = allBytes[i];

                    if ((i + 1) % 32 == 0)
                    {
                        Directory_Entry entry = Get_Directory_Entry(entryBytes);
                        if (entry.File_Name[0] != '\0')
                        {
                            Directory_Table.Add(entry);
                        }
                        entryBytes = new byte[32];
                    }
                }
            }
        }

        public int Search_Directory(string name)
        {
            Read_Directory();

            if (name.Length < 11)
            {
                name = name.PadRight(11, '\0');
            }
            else
            {
                name = name.Substring(0, 11);
            }

            for (int i = 0; i < Directory_Table.Count; i++)
            {
                string entryName = new string(Directory_Table[i].File_Name);
                if (entryName.Equals(name))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Update_Directory_Content(Directory_Entry dir)
        {
            Read_Directory();
            int index = Search_Directory(new string(dir.File_Name));

            if (index != -1)
            {
                Directory_Table.RemoveAt(index);
                Directory_Table.Insert(index, dir);
                Write_Directory();
            }
        }

        public void Delete_Directory()
        {
            if (First_Cluster != 0)
            {
                int currentBlock = First_Cluster;
                int nextBlock = FAT.Get_Next_Block(currentBlock);

                do
                {
                    FAT.Set_Next_Block(currentBlock, 0);
                    currentBlock = nextBlock;
                    if (currentBlock != -1)
                    {
                        nextBlock = FAT.Get_Next_Block(currentBlock);
                    }
                } while (currentBlock != -1);
            }

            if (Parent != null)
            {
                Parent.Read_Directory();
                int index = Parent.Search_Directory(new string(File_Name));
                if (index != -1)
                {
                    Parent.Directory_Table.RemoveAt(index);
                    Parent.Write_Directory();
                }
            }

            FAT.Write_FAT_Table();
        }

        public bool Exists(string name)
        {
            return Search_Directory(name) != -1;
        }
    }
}
