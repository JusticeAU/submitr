using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

enum progress
{
    Choosing,
    Scanning,
    Deleting
}

namespace submitr
{
    internal class Program
    {
        static bool ConfirmDeletion()
        {
            bool confirmed = false;
            while (!confirmed)
            {
                if (Console.ReadLine() == "delete")
                {
                    return true;
                }
            }
            return false;
        }

        static bool MarkForDelete()
        {
            while (true)
            {
                Console.WriteLine("Mark for Delete? Y/N");
                while (!Console.KeyAvailable)
                    Thread.Sleep(16);

                // Get input when in buffer
                ConsoleKeyInfo cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.Y:
                        return true;
                    case ConsoleKey.N:
                        return false;
                    default:
                        break;
                }
            }
        }
        static void PrintFolderContents(string folder, int depth)
        {
            string depthBuffer = " ";
            depthBuffer = depthBuffer.PadRight(depth);
            foreach (string folds in Directory.GetDirectories(folder))
            {
                Console.WriteLine(depthBuffer + folds.Substring(folder.Length));
                PrintFolderContents(folds, depth+1);
            }
            foreach (string files in Directory.GetFiles(folder))
            {
                Console.WriteLine(depthBuffer + files.Substring(folder.Length+1));
            }
        }
        static void ScanDirectory(string start, string root, List<string> toDelete)
        {
            bool SolutionFolder = false;
            foreach(string file in Directory.GetFiles(start))
            {
                if (file.EndsWith(".sln") || file.EndsWith(".csproj") || file.EndsWith(".vcxproj"))
                {
                    Console.WriteLine("Solution/Project Folder detected");
                    SolutionFolder = true;
                    break;
                }
            }

            foreach (string directory in Directory.GetDirectories(start))
            {
                Console.Clear();
                if (directory.EndsWith("\\.vs"))
                {
                    int lastSlash = directory.LastIndexOf("\\") + 1;
                    Console.WriteLine(directory.Substring(lastSlash) + " folder detected next to Solution or Project Folder.");
                    Console.WriteLine("Folder:\t" + directory);
                    Console.WriteLine("---CONTENTS---");
                    PrintFolderContents(directory, 1);
                    if (MarkForDelete())
                        toDelete.Add(directory);
                    else
                        ScanDirectory(directory, root, toDelete);
                }
                else if(SolutionFolder)
                {
                    if(directory.EndsWith("bin") || directory.EndsWith("obj") || directory.EndsWith("Debug") || directory.EndsWith("Release") || directory.EndsWith("x64") || directory.EndsWith("x86"))
                    {
                        int lastSlash = directory.LastIndexOf("\\")+1;
                        Console.WriteLine(directory.Substring(lastSlash) + " folder detected next to Solution or Project.");
                        Console.WriteLine("Folder:\t" + directory);
                        Console.WriteLine("---CONTENTS---");
                        PrintFolderContents(directory, 1);
                        if(MarkForDelete())
                            toDelete.Add(directory);
                        else
                            ScanDirectory(directory, root, toDelete);
                    }
                    else
                        ScanDirectory(directory, root, toDelete);
                }
                else
                    ScanDirectory(directory, root, toDelete);
            }
            return;
        }

        static void Main(string[] args)
        {
            progress status = progress.Choosing;

            // choosing where to process
            char selection = '>';
            int selectionIndex = 0;
            string currentDirectory = Directory.GetCurrentDirectory();
            List<string> browseDirectories = new List<string>();
            List<string> deleteDirs = new List<string>();
            while(status == progress.Choosing)
            {
                Console.Clear();
                bool confirmed = false;
                browseDirectories.Clear();
                // Get current directory structures
                Console.WriteLine("Backspace to go up a directory. Enter/Return to enter a directory. F1 to begin processing Current Directory.");
                Console.WriteLine("Current Directory: " + currentDirectory);
                Console.WriteLine("---FOLDERS---");
                foreach (string directory in Directory.GetDirectories(currentDirectory))
                {
                    browseDirectories.Add(directory);
                }

                foreach (string directory in browseDirectories)
                {
                    Console.WriteLine("\t" + directory);
                }

                Console.CursorTop = Console.CursorTop - browseDirectories.Count;
                for (int i = 0; i < browseDirectories.Count; i++)
                {
                    // Render cursor
                    if (selectionIndex == i) Console.WriteLine(selection);
                    else Console.WriteLine(" ");
                }

                while (!confirmed)
                {
                    while (!Console.KeyAvailable)
                        Thread.Sleep(16);

                    // Get input when in buffer
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                        case ConsoleKey.Backspace:
                            currentDirectory = currentDirectory.Substring(0, currentDirectory.Length-(currentDirectory.Length - currentDirectory.LastIndexOf('\\')));
                            selectionIndex = 0;
                            Console.Clear();
                            confirmed = true;
                            break;
                        case ConsoleKey.Enter:
                            string folder = browseDirectories[selectionIndex];
                            currentDirectory += folder.Substring(folder.LastIndexOf('\\'), folder.Length-folder.LastIndexOf('\\'));
                            selectionIndex = 0;
                            confirmed = true;
                            break;
                        case ConsoleKey.UpArrow:
                            selectionIndex--;
                            confirmed = true;
                            break;
                        case ConsoleKey.DownArrow:
                            selectionIndex++;
                            confirmed = true;
                            break;
                        case ConsoleKey.F1:
                            status = progress.Scanning;
                            confirmed = true;
                            break;
                    }
                }

                // do stuff with input.
                // Wrap selectionIndex
                if (selectionIndex < 0)
                    selectionIndex = 0;
                else if(selectionIndex>=browseDirectories.Count)
                    selectionIndex = browseDirectories.Count;
            }

            // Process selected folder
            ScanDirectory(currentDirectory, currentDirectory, deleteDirs);

            // Confirm Deletion
            Console.Clear();
            Console.WriteLine("Folders Marked for Delete:");

            foreach(string dir in deleteDirs)
            {
                Console.WriteLine(dir);
            }
            
            // perform deletion
            Console.WriteLine("Enter the word 'delete' to confirm.");
            if(ConfirmDeletion())
            {
                foreach (string dir in deleteDirs)
                {
                    Directory.Delete(dir, true);
                }
                Console.WriteLine("All folders deleted");
                Console.ReadKey();
            }
        }
    }
}