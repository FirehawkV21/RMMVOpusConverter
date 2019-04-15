using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SneakyConverter
{
    internal class Program
    {
        private static string _converterLocation;
        private static string _sourceLocation;
        private static string _dropLocation;
        private static bool _isParallel;
        private static readonly ProcessStartInfo converterInfo = new ProcessStartInfo();
        private static void Main(string[] args)
        {
            string standardFlags = "-c:a libopus -nostdin -y";

            Console.WriteLine("=====================================================");
            Console.WriteLine("= Convert to Opus and Disguise Tool for RPG Maker MV");
            Console.WriteLine("= Version D1.00");
            Console.WriteLine("= Developed by AceOfAces.");
            Console.WriteLine("= Licensed under the MIT license.");
            Console.WriteLine("=====================================================\n");
            if (args.Length >= 1)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "--Parallel":
                            _isParallel = true;
                            Console.WriteLine("Parallel mode is active.");
                            break;
                        case "--ConverterLocation":
                            if (i >= args.Length - 1 && !args[i + 1].Contains("--"))
                            {
                                if (Directory.Exists(args[i + 1])) _converterLocation = args[i + 1];
                                Console.WriteLine("The location for the converter is set.");
                            }
                            break;
                        case "--SourceLocation":
                            if (i < args.Length - 1 && !args[i + 1].Contains("--"))
                            {
                                if (Directory.Exists(args[i + 1])) _sourceLocation = args[i + 1];
                                Console.WriteLine("The location for source is set.");
                            }
                            break;
                        case "--OutputLocation":
                            if (i < args.Length - 1 && !args[i + 1].Contains("--"))
                            {
                                if (Directory.Exists(args[i + 1])) _dropLocation = args[i + 1];
                                Console.WriteLine("The location for the output is set.");
                            }
                            break;
                    }
                }
            }

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

            converterInfo.FileName = Path.Combine(_converterLocation,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg");            
            converterInfo.CreateNoWindow = true;
            string[] fileMap = Directory.GetFiles(_sourceLocation, "*.ogg", SearchOption.AllDirectories);
            try
            {
                if (_isParallel)
                {
                    Parallel.ForEach(fileMap, soundFile =>
                    {
                        string fileLocBuffer = soundFile.Replace(_sourceLocation, "");
                        string fileName = Path.GetFileName(soundFile);
                        fileLocBuffer = fileLocBuffer.Replace(fileName, "");
                        string tempString = _dropLocation + fileLocBuffer + fileName;
                        if (!Directory.Exists(Path.Combine(_dropLocation, fileLocBuffer)))
                            Directory.CreateDirectory(Path.Combine(_dropLocation, fileLocBuffer));
                        converterInfo.Arguments = standardFlags + " -i \"" + soundFile + "\" \"" + tempString + "\"";
                        Process.Start(converterInfo)?.WaitForExit();
                    });
                }
                else
                {
                    foreach (string soundFile in fileMap)
                    {
                        string fileLocBuffer = soundFile.Replace(_sourceLocation, "");
                        string fileName = Path.GetFileName(soundFile);
                        fileLocBuffer = fileLocBuffer.Replace(fileName, "");
                        string tempString = _dropLocation + fileLocBuffer + fileName;
                        if (!Directory.Exists(Path.Combine(_dropLocation, fileLocBuffer)))
                            Directory.CreateDirectory(Path.Combine(_dropLocation, fileLocBuffer));
                        converterInfo.Arguments = standardFlags + " -i \"" + soundFile + "\" \"" + tempString + "\"";
                        Process.Start(converterInfo)?.WaitForExit();
                    }
                }
                Console.WriteLine("The task was completed.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
            Console.WriteLine("Press Enter/Return to exit.");
            Console.ReadLine();
        }
    }
}