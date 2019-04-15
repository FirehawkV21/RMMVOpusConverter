using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SneakyConverter
{
    internal class Program
    {
        private static string _converterLocation;
        private static string _sourceLocation;
        private static string _dropLocation;
        private static void Main(string[] args)
        {

            do
            {
                //Ask the user where is the SDK. Check if the folder's there.
                Console.WriteLine("Where's the FFMPEG location? ");
                _converterLocation = Console.ReadLine();
                if (_converterLocation == null) Console.WriteLine("Please insert the path for FFMPEG please.\n");
                else if (!Directory.Exists(_converterLocation))
                    Console.Write("The directory isn't there. Please select an existing folder.\n");
            } while (_converterLocation == null || !Directory.Exists(_converterLocation));

            do
            {
                //Ask the user what project to compile. Check if the folder is there and there's a js folder.
                Console.WriteLine("\nWhere are the files you want to convert to? ");
                _sourceLocation = Console.ReadLine();

                if (_sourceLocation == null) Console.WriteLine("Please specify the location of the folder.\n");
                else if (!Directory.Exists(_sourceLocation))
                    Console.WriteLine("The folder you've selected isn't present.\n");
            } while (_sourceLocation == null || !Directory.Exists(_sourceLocation));

            do
            {
                //Ask the user what project to compile. Check if the folder is there and there's a js folder.
                Console.WriteLine("\nWhere to put the converted files?");
                _dropLocation = Console.ReadLine();

                if (_dropLocation == null)
                    Console.WriteLine("Please specify the location of the folder.\n");
                else if (!Directory.Exists(_dropLocation))
                {
                    Console.WriteLine("Creating folder...\n");
                    Directory.CreateDirectory(_dropLocation);
                }
            }
            while (_dropLocation == null);

            string[] fileMap = Directory.GetFiles(_sourceLocation, "*.ogg", SearchOption.AllDirectories);
            foreach (string soundFile in fileMap)
            {
                string fileLocBuffer = soundFile.Replace(_sourceLocation, "");
                string fileName = Path.GetFileName(soundFile);
                fileLocBuffer = fileLocBuffer.Replace(fileName, "");
                string tempString = _dropLocation + fileLocBuffer + fileName;
                if (!Directory.Exists(Path.Combine(_dropLocation, fileLocBuffer)))
                    Directory.CreateDirectory(Path.Combine(_dropLocation, fileLocBuffer));
                Process.Start(Path.Combine(_converterLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg"), "-i \"" + soundFile + "\" -c:a libopus -nostdin -y \"" + tempString + "\"")?.WaitForExit();
            }
        }
    }
}
