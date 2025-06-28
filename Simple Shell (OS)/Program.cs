// See https://aka.ms/new-console-template for more information
using Simple_Shell__OS_;
class Program
{
    public static Simple_Shell__OS_.Directory Current_Directory;
    public static string Current_Path;

    static void Main(string[] args)
    {
        try
        {
            InitializeSystem();
            RunShell();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    static void InitializeSystem()
    {
        Virtual_Disk.Initialize_Virtual_Disk();
        Current_Path = new string(Current_Directory.File_Name).Trim();
    }

    static void RunShell()
    {
        Console.WriteLine("Virtual Disk Operating System - Type 'help' for commands");

        while (true)
        {
            try
            {
                Console.Write($"{Current_Path}> ");
                string input = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(input)) continue;

                string[] parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0].ToLower();

                switch (command)
                {
                    case "cd":
                        if (parts.Length > 1) Commands.Change_Directory(parts[1]);
                        else Console.WriteLine(Current_Path);
                        break;
                    case "cls":
                        Console.Clear();
                        break;
                    case "dir":
                        Commands.Show_Content_Directory();
                        break;
                    case "md":
                        if (parts.Length > 1) Commands.Make_Directory(parts[1]);
                        else Console.WriteLine("Syntax: md [directory_name]");
                        break;
                    case "rd":
                        if (parts.Length > 1) Commands.Remove_Directory(parts[1]);
                        else Console.WriteLine("Syntax: rd [directory_name]");
                        break;
                    case "import":
                        if (parts.Length > 1) Commands.Import(parts[1]);
                        else Console.WriteLine("Syntax: import [host_file_path]");
                        break;
                    case "export":
                        if (parts.Length > 2) Commands.Export(parts[1], parts[2]);
                        else Console.WriteLine("Syntax: export [virtual_file] [host_destination]");
                        break;
                    case "type":
                        if (parts.Length > 1) Commands.Type(parts[1]);
                        else Console.WriteLine("Syntax: type [file_name]");
                        break;
                    case "del":
                        if (parts.Length > 1) Commands.Delete(parts[1]);
                        else Console.WriteLine("Syntax: del [file_name]");
                        break;
                    case "copy":
                        if (parts.Length > 2) Commands.Copy(parts[1], parts[2]);
                        else Console.WriteLine("Syntax: copy [source] [destination]");
                        break;
                    case "rename":
                        if (parts.Length > 2) Commands.Rename(parts[1], parts[2]);
                        else Console.WriteLine("Syntax: rename [old_name] [new_name]");
                        break;
                    case "help":
                        if (parts.Length > 1) Commands.Help(parts[1]);
                        else Commands.Help();
                        break;
                    case "quit":
                        return;
                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}