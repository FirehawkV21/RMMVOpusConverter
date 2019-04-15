using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace RMMVOpusConverter
{
    internal static class Program
    {
        private static string _converterLocation;
        private static string _sourceLocation;
        private static string _dropLocation;
        private static bool _isParallel;
        private static readonly ProcessStartInfo ConverterInfo = new ProcessStartInfo();
        private static bool _settingsSet;
        private static bool _diagnosticsMode;
        private static string _stringBuffer;
        private static void Main(string[] args)
        {
            //This is the format of the flags used for FFMPEG.
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
                        case "--DiagnosticsMode":
                            if (_isParallel)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine("Turning off Parallel Mode as it may screw up ffmpeg's output.");
                                _isParallel = false;
                            }
                            _diagnosticsMode = true;
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine("Diagnostics Mode is turned on. FFMPEG will print its output here.");
                            break;
                        case "--Parallel":
                            if (!_diagnosticsMode)
                            {
                                _isParallel = true;
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine("Parallel mode is active.");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.WriteLine("Cannot turn on Parallel mode when Diagnostics mode is active.");
                            }
                            break;
                        case "--ConverterLocation":
                            if (i < args.Length - 1 && !args[i + 1].Contains("--"))
                            {
                                _stringBuffer = args[i + 1].Replace("\"", "");
                                if (Directory.Exists(_stringBuffer)) _converterLocation = _stringBuffer;
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Console.WriteLine("The folder does not exist.");
                                    Console.ResetColor();
                                    Console.WriteLine("Press Enter/Return to exit.");
                                    Console.ReadLine();
                                    Environment.Exit(0);
                                }

                                if (File.Exists(Path.Combine(_converterLocation,
                                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg")))
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine("The location for the converter is set.");
                                }
                                else
                                {

                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Console.WriteLine("There is no FFMPEG in the folder.");
                                    Console.ResetColor();
                                    Console.WriteLine("Press Enter/Return to exit.");
                                    Console.ReadLine();
                                    Environment.Exit(0);
                                }
                            }
                            break;
                        case "--SourceLocation":
                            if (i < args.Length - 1 && !args[i + 1].Contains("--"))
                            {
                                _stringBuffer = args[i + 1].Replace("\"", "");
                                if (Directory.Exists(_stringBuffer))
                                {
                                    _sourceLocation = _stringBuffer;
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine("The location for source is set.");
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Console.WriteLine("The source location doesn't exist.");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.WriteLine("Press Enter/Return to exit");
                                    Console.ReadLine();
                                    Environment.Exit(0);
                                }
                            }
                            break;
                        case "--OutputLocation":
                            if (i < args.Length - 1 && !args[i + 1].Contains("--"))
                            {
                                _stringBuffer = args[i + 1].Replace("\"", "");
                                if (!Directory.Exists(_stringBuffer)) Directory.CreateDirectory(_stringBuffer);
                                _dropLocation = _stringBuffer;
                                Console.WriteLine("The location for the output is set.");
                            }
                            break;
                    }
                }
                if (_converterLocation != null && _sourceLocation != null && _dropLocation != null)
                    _settingsSet = true;
                Console.ResetColor();
                Console.WriteLine();
            }

            if (!_settingsSet)
            {
                if (_converterLocation == null)
                    do
                    {
                        //Ask the user where is FFMPEG. Check if the folder's there.
                        Console.WriteLine("Where's the FFMPEG location? ");
                        _converterLocation = Console.ReadLine();
                        if (_converterLocation == null) Console.WriteLine("Please insert the path for FFMPEG please.\n");
                        else if (!Directory.Exists(_converterLocation))
                            Console.Write("The directory isn't there. Please select an existing folder.\n");
                    } while (_converterLocation == null || !Directory.Exists(_converterLocation));

                do
                {
                    //Ask the user where are the audio files. Check if the folder is there.
                    Console.WriteLine("\nWhere are the files you want to convert to? ");
                    _sourceLocation = Console.ReadLine();

                    if (_sourceLocation == null) Console.WriteLine("Please specify the location of the folder.\n");
                    else if (!Directory.Exists(_sourceLocation))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("The folder you've selected isn't present.\n");
                        Console.ResetColor();
                    }
                } while (_sourceLocation == null || !Directory.Exists(_sourceLocation));

                do
                {
                    //Ask the user where to put the processed audio files. If the folder isn't there create it.
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
            //Quick preparation of FFMPEG.
            ConverterInfo.FileName = Path.Combine(_converterLocation,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ffmpeg.exe" : "ffmpeg");
            if (!_diagnosticsMode)
            {
                ConverterInfo.CreateNoWindow = true;
                ConverterInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }

            //Index the files to an array.
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
                        if (!Directory.Exists(_dropLocation + fileLocBuffer))
                            Directory.CreateDirectory(_dropLocation + fileLocBuffer);
                        ConverterInfo.Arguments = " -i \"" + soundFile + "\" " + standardFlags + " \"" + tempString + "\"";
                        Console.WriteLine("[{0}]Thread No.{1} is converting {2} to Opus...", DateTime.Now, Thread.CurrentThread.ManagedThreadId, soundFile);
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
                        if (!Directory.Exists(_dropLocation + fileLocBuffer))
                            Directory.CreateDirectory(_dropLocation + fileLocBuffer);
                        ConverterInfo.Arguments = " -i \"" + soundFile + "\" " + standardFlags + " \"" + tempString + "\"";
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("[{0}] ", DateTime.Now);
                        Console.ResetColor();
                        Console.WriteLine("Converting {0} to Opus...", soundFile);
                        Process.Start(ConverterInfo)?.WaitForExit();
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("[{0}] ", DateTime.Now);
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("Finished converting {0}.", soundFile);
                    }
                }

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("\nThe task was completed.");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("\n" + e);
                Console.ResetColor();
            }

            Console.WriteLine("Press Enter/Return to exit.");
            Console.ReadLine();
        }
    }
}