using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Shell__OS_
{
    class Directory_Entry
    {
        public char[] File_Name = new char[11];
        public byte File_Attribute;
        byte[] File_Empty = new byte[12];
        public int First_Cluster;
        public int File_Size;

        public Directory_Entry() { }

        public Directory_Entry(string name, byte f_attribute, int f_Cluster, int f_size = 0)
        {
            if (name.Length > 11)
                name = name.Substring(0, 11);

            File_Name = name.PadRight(11, '\0').ToCharArray();
            File_Attribute = f_attribute;
            First_Cluster = f_Cluster;
            File_Size = f_size;
        }

        public byte[] Get_Bytes()
        {
            byte[] Bytes = new byte[32];

            // File Name (11 bytes)
            byte[] file_name_bytes = Encoding.ASCII.GetBytes(File_Name);
            Array.Copy(file_name_bytes, Bytes, Math.Min(file_name_bytes.Length, 11));

            // File Attribute (1 byte)
            Bytes[11] = File_Attribute;

            // Empty Space (12 bytes)
            Array.Copy(File_Empty, 0, Bytes, 12, 12);

            // First Cluster (4 bytes)
            byte[] firstClusterBytes = BitConverter.GetBytes(First_Cluster);
            Array.Copy(firstClusterBytes, 0, Bytes, 24, 4);

            // File Size (4 bytes)
            byte[] fileSizeBytes = BitConverter.GetBytes(File_Size);
            Array.Copy(fileSizeBytes, 0, Bytes, 28, 4);

            return Bytes;
        }

        public Directory_Entry Get_Directory_Entry(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 32)
                throw new ArgumentException("Invalid directory entry data");

            Directory_Entry d = new Directory_Entry();

            // File Name
            char[] fileNameChars = new char[11];
            for (int i = 0; i < 11; i++)
            {
                fileNameChars[i] = (char)bytes[i];
            }
            d.File_Name = fileNameChars;

            // File Attribute
            d.File_Attribute = bytes[11];

            // Empty Space (not used)
            Array.Copy(bytes, 12, d.File_Empty, 0, 12);

            // First Cluster
            byte[] firstClusterBytes = new byte[4];
            Array.Copy(bytes, 24, firstClusterBytes, 0, 4);
            d.First_Cluster = BitConverter.ToInt32(firstClusterBytes, 0);

            // File Size
            byte[] fileSizeBytes = new byte[4];
            Array.Copy(bytes, 28, fileSizeBytes, 0, 4);
            d.File_Size = BitConverter.ToInt32(fileSizeBytes, 0);

            return d;
        }

        public Directory_Entry Get_Directory_Entry()
        {
            return new Directory_Entry(new string(this.File_Name).Trim(),
                                    this.File_Attribute,
                                    this.First_Cluster,
                                    this.File_Size);
        }
    }
}
