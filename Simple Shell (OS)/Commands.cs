using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Simple_Shell__OS_
{
    class Commands
    {
        public static void Make_Directory(string dir_name)
        {
            try
            {
                int index = Program.Current_Directory.Search_Directory(dir_name);
                if (index == -1)
                {
                    Directory_Entry dir = new Directory_Entry(dir_name, 0x10, 0);
                    Program.Current_Directory.Directory_Table.Add(dir);
                    Program.Current_Directory.Write_Directory();

                    if (Program.Current_Directory.Parent != null)
                    {
                        Program.Current_Directory.Parent.Update_Directory_Content(Program.Current_Directory.Get_Directory_Entry());
                        Program.Current_Directory.Parent.Write_Directory();
                    }

                    FAT.Write_FAT_Table();
                }
                else
                {
                    Console.WriteLine($"Directory \"{dir_name}\" already exists");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating directory: {ex.Message}");
            }
        }

        public static void Remove_Directory(string dir_name)
        {
            try
            {
                int index = Program.Current_Directory.Search_Directory(dir_name);
                if (index != -1)
                {
                    if (Program.Current_Directory.Directory_Table[index].File_Attribute == 0x10)
                    {
                        int cluster = Program.Current_Directory.Directory_Table[index].First_Cluster;
                        Directory dir = new Directory(dir_name, 0x10, cluster, Program.Current_Directory);
                        dir.Delete_Directory();
                    }
                    else
                    {
                        Console.WriteLine($"\"{dir_name}\" is not a directory");
                    }
                }
                else
                {
                    Console.WriteLine($"Directory \"{dir_name}\" not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing directory: {ex.Message}");
            }
        }

        public static void Change_Directory(string dir_name)
        {
            try
            {
                if (dir_name == "..")
                {
                    if (Program.Current_Directory.Parent != null)
                    {
                        Program.Current_Directory = Program.Current_Directory.Parent;
                        Program.Current_Path = Program.Current_Path.Substring(0, Program.Current_Path.LastIndexOf('\\'));
                    }
                    return;
                }

                int index = Program.Current_Directory.Search_Directory(dir_name);
                if (index != -1)
                {
                    if (Program.Current_Directory.Directory_Table[index].File_Attribute == 0x10)
                    {
                        int cluster = Program.Current_Directory.Directory_Table[index].First_Cluster;
                        Directory dir = new Directory(dir_name, 0x10, cluster, Program.Current_Directory);
                        Program.Current_Directory = dir;
                        Program.Current_Path += "\\" + dir_name;
                        Program.Current_Directory.Read_Directory();
                    }
                    else
                    {
                        Console.WriteLine($"\"{dir_name}\" is not a directory");
                    }
                }
                else
                {
                    Console.WriteLine($"Directory \"{dir_name}\" not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error changing directory: {ex.Message}");
            }
        }

        public static void Show_Content_Directory()
        {
            try
            {
                Program.Current_Directory.Read_Directory();
                int fileCount = 0;
                int dirCount = 0;
                long totalSize = 0;

                Console.WriteLine($" Directory of {Program.Current_Path}\n");

                foreach (var entry in Program.Current_Directory.Directory_Table)
                {
                    if (entry.File_Attribute == 0x0)
                    {
                        Console.WriteLine($"\t{entry.File_Size,10}  {new string(entry.File_Name).Trim()}");
                        fileCount++;
                        totalSize += entry.File_Size;
                    }
                    else
                    {
                        Console.WriteLine($"\t<DIR>      {new string(entry.File_Name).Trim()}");
                        dirCount++;
                    }
                }

                Console.WriteLine($"\n\t{fileCount} File(s)\t{totalSize} bytes");
                Console.WriteLine($"\t{dirCount} Dir(s)\t{FAT.Get_Free_Space()} bytes free");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing directory contents: {ex.Message}");
            }
        }

        public static void Import(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string name = Path.GetFileName(path);
                    string content = File.ReadAllText(path);
                    int size = content.Length;

                    int index = Program.Current_Directory.Search_Directory(name);
                    if (index == -1)
                    {
                        int cluster = (size > 0) ? FAT.Get_Available_Block() : 0;
                        if (cluster == -1) throw new Exception("No space available");

                        File_Entry file = new File_Entry(name, 0x0, cluster, size, Program.Current_Directory, content);
                        file.Write_File_Content();

                        Directory_Entry dirEntry = new Directory_Entry(name, 0x0, cluster, size);
                        Program.Current_Directory.Directory_Table.Add(dirEntry);
                        Program.Current_Directory.Write_Directory();
                    }
                    else
                    {
                        Console.WriteLine($"File \"{name}\" already exists");
                    }
                }
                else
                {
                    Console.WriteLine("Source file not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing file: {ex.Message}");
            }
        }

        public static void Type(string file_name)
        {
            try
            {
                int index = Program.Current_Directory.Search_Directory(file_name);
                if (index != -1)
                {
                    if (Program.Current_Directory.Directory_Table[index].File_Attribute == 0x0)
                    {
                        int cluster = Program.Current_Directory.Directory_Table[index].First_Cluster;
                        int size = Program.Current_Directory.Directory_Table[index].File_Size;
                        File_Entry file = new File_Entry(file_name, 0x0, cluster, size, Program.Current_Directory, null);
                        file.Read_File_Content();
                        Console.WriteLine(file.File_Content);
                    }
                    else
                    {
                        Console.WriteLine($"\"{file_name}\" is not a file");
                    }
                }
                else
                {
                    Console.WriteLine($"File \"{file_name}\" not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error displaying file: {ex.Message}");
            }
        }

        public static void Export(string source, string destination)
        {
            try
            {
                if (!System.IO.Directory.Exists(destination))
                {
                    Console.WriteLine("Destination directory does not exist");
                    return;
                }

                int index = Program.Current_Directory.Search_Directory(source);
                if (index != -1)
                {
                    if (Program.Current_Directory.Directory_Table[index].File_Attribute == 0x0)
                    {
                        int cluster = Program.Current_Directory.Directory_Table[index].First_Cluster;
                        int size = Program.Current_Directory.Directory_Table[index].File_Size;
                        File_Entry file = new File_Entry(source, 0x0, cluster, size, Program.Current_Directory, null);
                        file.Read_File_Content();

                        string exportPath = Path.Combine(destination, source);
                        File.WriteAllText(exportPath, file.File_Content);
                        Console.WriteLine($"File exported to {exportPath}");
                    }
                    else
                    {
                        Console.WriteLine($"\"{source}\" is not a file");
                    }
                }
                else
                {
                    Console.WriteLine($"File \"{source}\" not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting file: {ex.Message}");
            }
        }

        public static void Copy(string source, string destination)
        {
            try
            {
                int srcIndex = Program.Current_Directory.Search_Directory(source);
                if (srcIndex != -1)
                {
                    int destIndex = Program.Current_Directory.Search_Directory(destination);
                    if (destIndex != -1 &&
                        Program.Current_Directory.Directory_Table[destIndex].File_Attribute == 0x10)
                    {
                        int cluster = Program.Current_Directory.Directory_Table[destIndex].First_Cluster;
                        Directory destDir = new Directory(destination, 0x10, cluster, Program.Current_Directory);

                        Directory_Entry entry = Program.Current_Directory.Directory_Table[srcIndex].Get_Directory_Entry();
                        destDir.Directory_Table.Add(entry);
                        destDir.Write_Directory();
                    }
                    else
                    {
                        Console.WriteLine($"Destination directory \"{destination}\" not found");
                    }
                }
                else
                {
                    Console.WriteLine($"Source file \"{source}\" not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying file: {ex.Message}");
            }
        }

        public static void Delete(string file_name)
        {
            try
            {
                int index = Program.Current_Directory.Search_Directory(file_name);
                if (index != -1)
                {
                    if (Program.Current_Directory.Directory_Table[index].File_Attribute == 0x0)
                    {
                        int cluster = Program.Current_Directory.Directory_Table[index].First_Cluster;
                        int size = Program.Current_Directory.Directory_Table[index].File_Size;
                        File_Entry file = new File_Entry(file_name, 0x0, cluster, size, Program.Current_Directory, null);
                        file.Delete_File_Content();
                    }
                    else
                    {
                        Console.WriteLine($"\"{file_name}\" is not a file");
                    }
                }
                else
                {
                    Console.WriteLine($"File \"{file_name}\" not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
            }
        }

        public static void Rename(string old_name, string new_name)
        {
            try
            {
                int oldIndex = Program.Current_Directory.Search_Directory(old_name);
                if (oldIndex != -1)
                {
                    int newIndex = Program.Current_Directory.Search_Directory(new_name);
                    if (newIndex == -1)
                    {
                        Directory_Entry entry = Program.Current_Directory.Directory_Table[oldIndex];
                        entry.File_Name = new_name.PadRight(11, '\0').ToCharArray();
                        Program.Current_Directory.Directory_Table[oldIndex] = entry;
                        Program.Current_Directory.Write_Directory();
                    }
                    else
                    {
                        Console.WriteLine($"\"{new_name}\" already exists");
                    }
                }
                else
                {
                    Console.WriteLine($"\"{old_name}\" not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error renaming: {ex.Message}");
            }
        }

        public static void Help()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("cd [directory] - Change current directory");
            Console.WriteLine("cd .. - Move to parent directory");
            Console.WriteLine("cls - Clear screen");
            Console.WriteLine("dir - List directory contents");
            Console.WriteLine("md [directory] - Create directory");
            Console.WriteLine("rd [directory] - Remove directory");
            Console.WriteLine("import [path] - Import file from host system");
            Console.WriteLine("export [source] [dest] - Export file to host system");
            Console.WriteLine("type [file] - Display file contents");
            Console.WriteLine("del [file] - Delete file");
            Console.WriteLine("copy [source] [dest] - Copy file");
            Console.WriteLine("rename [old] [new] - Rename file/directory");
            Console.WriteLine("help - Show this help");
            Console.WriteLine("quit - Exit the shell");
        }

        public static void Help(string command)
        {
            switch (command.ToLower())
            {
                case "cd":
                    Console.WriteLine("cd - Change the current directory");
                    Console.WriteLine("Usage: cd [directory] or cd ..");
                    break;
                case "cls":
                    Console.WriteLine("cls - Clear the console screen");
                    break;
                case "dir":
                    Console.WriteLine("dir - List contents of current directory");
                    break;
                case "md":
                    Console.WriteLine("md - Create a new directory");
                    Console.WriteLine("Usage: md [directory_name]");
                    break;
                case "rd":
                    Console.WriteLine("rd - Remove a directory");
                    Console.WriteLine("Usage: rd [directory_name]");
                    break;
                case "import":
                    Console.WriteLine("import - Import a file from host system");
                    Console.WriteLine("Usage: import [host_file_path]");
                    break;
                case "export":
                    Console.WriteLine("export - Export a file to host system");
                    Console.WriteLine("Usage: export [virtual_file] [host_destination]");
                    break;
                case "type":
                    Console.WriteLine("type - Display contents of a text file");
                    Console.WriteLine("Usage: type [file_name]");
                    break;
                case "del":
                    Console.WriteLine("del - Delete a file");
                    Console.WriteLine("Usage: del [file_name]");
                    break;
                case "copy":
                    Console.WriteLine("copy - Copy a file");
                    Console.WriteLine("Usage: copy [source] [destination]");
                    break;
                case "rename":
                    Console.WriteLine("rename - Rename a file or directory");
                    Console.WriteLine("Usage: rename [old_name] [new_name]");
                    break;
                case "help":
                    Console.WriteLine("help - Show help information");
                    Console.WriteLine("Usage: help or help [command]");
                    break;
                case "quit":
                    Console.WriteLine("quit - Exit the shell");
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    break;
            }
        }
    }
}