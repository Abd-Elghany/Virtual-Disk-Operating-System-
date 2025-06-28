Virtual Disk Operating System
A simple shell interface and FAT-based file system implementation in C# that simulates a basic operating system environment with file and directory management capabilities.

Features
Virtual Disk Management: Simulates disk blocks with a virtual disk file

FAT (File Allocation Table) Implementation: Manages file storage and allocation

Directory Structure: Supports creating, navigating, and managing directories

File Operations:

Create, delete, rename files

Import/export files between host system and virtual disk

View file contents

Shell Commands: Familiar command-line interface with common commands

Commands
Command	Description	Example
cd	Change directory	cd folder
cd ..	Move to parent directory	cd ..
cls	Clear screen	cls
dir	List directory contents	dir
md	Create directory	md newfolder
rd	Remove directory	rd oldfolder
import	Import file from host system	import C:\file.txt
export	Export file to host system	export file.txt C:\backup
type	Display file contents	type document.txt
del	Delete file	del file.txt
copy	Copy file	copy file.txt backup
rename	Rename file/directory	rename old.txt new.txt
help	Show help information	help or help cd
quit	Exit the shell	quit
Getting Started
Prerequisites
.NET Core SDK (version 3.1 or later)

Visual Studio or VS Code (optional)

Installation
Clone the repository:

bash
[git clone https://github.com/Abd-Elghany/virtual-disk-os.git](https://github.com/Abd-Elghany/Virtual-Disk-Operating-System-.git)
cd virtual-disk-os
Build the project:

bash
dotnet build
Run the application:

bash
dotnet run
File System Structure
The virtual file system uses a FAT-like structure with:

Super Block: First block of the disk (1024 bytes)

FAT Table: File Allocation Table (4096 bytes)

Data Blocks: File/directory storage (1019 blocks × 1024 bytes each)

Class Structure
Virtual_Disk: Manages disk blocks and I/O operations

FAT: Implements the File Allocation Table

Directory_Entry: Base class for file/directory entries

Directory: Represents directories in the file system

File_Entry: Represents files in the file system

Commands: Implements all shell commands

Program: Main application entry point

License
This project is licensed under the MIT License - see the LICENSE file for details.

Contributing
Contributions are welcome! Please open an issue or submit a pull request.

Acknowledgments
Inspired by traditional FAT file systems

Designed for educational purposes in operating system concepts
