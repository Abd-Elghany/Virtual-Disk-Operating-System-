using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Shell__OS_
{
    class File_Entry : Directory_Entry
    {
        public Directory Parent;
        public string File_Content;

        public File_Entry(string name, byte attribute, int firstCluster, int size, Directory par, string content)
            : base(name, attribute, firstCluster, size)
        {
            File_Content = content;
            Parent = par;
        }

        public void Write_File_Content()
        {
            byte[] contentBytes = Encoding.ASCII.GetBytes(File_Content);
            int requiredBlocks = (int)Math.Ceiling(contentBytes.Length / 1024.0);
            int remainingBytes = contentBytes.Length % 1024;

            if (requiredBlocks > FAT.Get_Available_Blocks())
            {
                throw new Exception("Not enough space to write file content");
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
                Array.Copy(contentBytes, b * 1024, block, 0, 1024);
                blocks.Add(block);
            }

            if (remainingBytes > 0)
            {
                byte[] lastBlock = new byte[1024];
                int startIndex = (requiredBlocks - 1) * 1024;
                Array.Copy(contentBytes, startIndex, lastBlock, 0, remainingBytes);
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

            File_Size = contentBytes.Length;

            if (Parent != null)
            {
                Parent.Update_Directory_Content(this);
            }

            FAT.Write_FAT_Table();
        }

        public void Read_File_Content()
        {
            if (First_Cluster != 0)
            {
                List<byte> allBytes = new List<byte>();
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

                File_Content = Encoding.ASCII.GetString(allBytes.ToArray()).Substring(0, File_Size);
            }
            else
            {
                File_Content = string.Empty;
            }
        }

        public void Delete_File_Content()
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
    }
}
