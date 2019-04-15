using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace SneakyConverter
{
    internal class Program
    {
        private static string _converterLocation;
        private static string _sourceLocation;
        private static string _dropLocation;
        private static bool _isParallel;
        private static readonly ProcessStartInfo ConverterInfo = new ProcessStartInfo();
        private static bool settingsSet;
        private static string stringBuffer;
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
                            if (i < args.Length - 1 && !args[i + 1].Contains("--"))
                            {
                                stringBuffer = args[i + 1].Replace("\"", "");
                                if (Directory.Exists(stringBuffer)) _converterLocation = stringBuffer;
                                else
                                {
                                    Console.WriteLine("The folder does not exist.\nPress Enter/Return to exit");
                                    Console.ReadLine();
                                    Environment.Exit(0);
                                }
                                if (File.Exists(Path.Combine(_converterLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows)? "ffmpeg.exe" : "ffmpeg")))
                                Console.WriteLine("The location for the converter is set.");
                                else
                                {
                                    Console.WriteLine("There is no FFMPEG in the folder.\nPress Enter/Return to exit");
                                    Console.ReadLine();
                                    Environment.Exit(0);
                                }
                            }
                            break;
                        case "--SourceLocation":
                            if (i < args.Length - 1 && !args[i + 1].Contains("--"))
                            {
                                stringBuffer = args[i + 1].Replace("\"", "");
                                if (Directory.Exists(stringBuffer))
                                {
                                    _sourceLocation = stringBuffer;
                                    Console.WriteLine("The location for source is set.");
                                }
                                else
                                {
                                    Console.WriteLine("The source location doesn't exist.");
                                    Console.WriteLine("Press Enter/Return to exit");
                                    Console.ReadLine();
                                    Environment.Exit(0);
                                }
                            }
                            break;
                        case "--OutputLocation":
                            if (i < args.Length - 1 && !args[i + 1].Contains("--"))
                            {
                                stringBuffer = args[i + 1].Replace("\"", "");
                                if (!Directory.Exists(stringBuffer)) Directory.CreateDirectory(stringBuffer);
                                    _dropLocation = stringBuffer;
                                Console.WriteLine("The location for the output is set.");
                            }
                            break;
                    }
                }
                if (_converterLocation != null && _sourceLocation != null && _dropLocation != null)
                    settingsSet = true;
                Console.WriteLine();
            }

            if (!settingsSet)
            {
                if (_converterLocation == null)
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
                Console.WriteLine();

            }
            ConverterInfo.FileName = Path.Combine(_converterLocation,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg");            
            ConverterInfo.CreateNoWindow = true;
            ConverterInfo.WindowStyle = ProcessWindowStyle.Hidden;
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
                        ConverterInfo.Arguments = standardFlags + " -i \"" + soundFile + "\" \"" + tempString + "\"";
                        Console.WriteLine("[{0}]Thread No.{1} is converting sound file {2} to Opus...", DateTime.Now, Thread.CurrentThread.ManagedThreadId, soundFile);
                        Process.Start(ConverterInfo)?.WaitForExit();
                        Console.WriteLine("[{0}]Thread No.{1} finished the conversion of {2}.", DateTime.Now, Thread.CurrentThread.ManagedThreadId, soundFile);
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
                        ConverterInfo.Arguments = standardFlags + " -i \"" + soundFile + "\" \"" + tempString + "\"";
                        Console.WriteLine("[{0}]Converting {1} to Opus...", DateTime.Now, soundFile);
                        Process.Start(ConverterInfo)?.WaitForExit();
                        Console.WriteLine("[{0}]Finished converting {1}.", DateTime.Now, soundFile);
                    }
                }
                Console.WriteLine("\nThe task was completed.");
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.ToString());
            }
            
            Console.WriteLine("Press Enter/Return to exit.");
            Console.ReadLine();
        }
    }
}