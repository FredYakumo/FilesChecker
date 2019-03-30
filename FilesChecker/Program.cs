/* Author: FredYakumo
 * Date: 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace FilesChecker {
    public class Program {
        public static string Version {
            get {
                return "0.2";
            }
        }

        public static string ReleaseDate {
            get {
                return "2019-03-31";
            }
        }

        public static string GetMD5HashCode(MD5 md5Hash, byte[] bytes) {
            byte[] resultByte = md5Hash.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            foreach (byte e in resultByte)
                sb.Append(e.ToString("x2").ToUpper());
            return sb.ToString();
        }

        public static string GetMD5HashCode(MD5 md5Hash, string str) {
            return GetMD5HashCode(md5Hash, Encoding.UTF8.GetBytes(str));
        }

        public static string GetMD5HashCode(MD5 md5Hash, Stream stream) {
            byte[] resultByte = md5Hash.ComputeHash(stream);
            StringBuilder sb = new StringBuilder();
            foreach (byte e in resultByte)
                sb.Append(e.ToString("x2").ToUpper());
            return sb.ToString();
        }

        private static void CheckFilesListSame(MD5 md5Hash, List<string> filesPath) {
            if (filesPath.Count <= 1) {
                WriteLineWithColor("Too few files.(Need at least 2)", ConsoleColor.Red);
                return;
            }

            Dictionary<string, uint> hashCodeDict = new Dictionary<string, uint>();
            Dictionary<string, StringBuilder> hashCodeFileList = new Dictionary<string, StringBuilder>();

            foreach (string path in filesPath) {
                try {
                    WriteLineWithColor($"Computing {path} ...", ConsoleColor.DarkGray);

                    Stream file = new FileStream(path, FileMode.Open);
                    string md5Code = GetMD5HashCode(md5Hash, file);
                    file.Close();

                    if (!hashCodeDict.ContainsKey(md5Code)) {
                        hashCodeDict.Add(md5Code, 1);
                        hashCodeFileList.Add(md5Code, new StringBuilder(path + "\n"));
                    } else {
                        ++hashCodeDict[md5Code];
                        hashCodeFileList[md5Code].Append(path + "\n");
                    }
                } catch (FileNotFoundException) {
                    WriteLineWithColor($"File {path} is not exists.", ConsoleColor.Yellow);
                }
            }

            if (hashCodeDict.Count == 0) {
                WriteLineWithColor("Not any files exist.", ConsoleColor.Red);
                return;
            }

            List<string> pluralMD5 = new List<string>();

            foreach (string e in hashCodeDict.Keys)
                if (hashCodeDict[e] > 1)
                    pluralMD5.Add(e);

            if (pluralMD5.Count == 0) {
                WriteLineWithColor("Not any files are same.", ConsoleColor.Red);
                return;
            }

            Console.WriteLine();

            foreach (string e in pluralMD5)
                WriteLineWithColor($"{hashCodeFileList[e].ToString()}are same" +
                    $"(md5: {e})\n===================", ConsoleColor.Green);
        }

        private static void ConsolePause() {
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }

        private static void GetDictionaryFiles(string path, List<string> output) {
            try {
                foreach (string e in Directory.GetFiles(path))
                    output.Add(e);
            } catch (UnauthorizedAccessException) {
                WriteLineWithColor($"Access {path} is denied.", ConsoleColor.Red);
            } catch (DirectoryNotFoundException) {
                WriteLineWithColor($"{path} not found.", ConsoleColor.Red);
            }
        }

        private static void GetDictionaryFilesRecursive(string path, List<string> output) {
            try {
                foreach (string e in Directory.GetFiles(path))
                    output.Add(e);
                foreach (string e in Directory.GetDirectories(path))
                    GetDictionaryFilesRecursive(e, output);
            } catch (UnauthorizedAccessException) {
                WriteLineWithColor($"Access {path} is denied.", ConsoleColor.Red);
            } catch (DirectoryNotFoundException) {
                WriteLineWithColor($"{path} not found.", ConsoleColor.Red);
            }
        }

        private static void WriteWithColor(string text, ConsoleColor color) {
            ConsoleColor previous = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = previous;
        }

        private static void WriteLineWithColor(string text, ConsoleColor color) {
            WriteWithColor(text + '\n', color);
        }

        private static string Input() {
            return Console.ReadLine();
        }

        private static string Input(string prompt) {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        private static void DisplayVersionInfo() {
            WriteLineWithColor("This tool is created by FredYakumo.", ConsoleColor.DarkGray);
            WriteLineWithColor($"Version: {Version}, Date: {ReleaseDate}", ConsoleColor.DarkGray);
        }

        private static void ConsoleInputPrompt() {
            Console.WriteLine("Add checking file(single line filename to one file, ");
            WriteLineWithColor("Split by pressing enter.", ConsoleColor.Yellow);
            WriteLineWithColor("Enter dictionary name to add a dictionary, Enter \"end\" to stop adding", ConsoleColor.Green);
            WriteLineWithColor("If a dictionary name without / or \\ at the back, only checking surface files(No recursion).", ConsoleColor.Red);
        }

        private static int Main(string[] args) {
            DisplayVersionInfo();
            Console.WriteLine();

            MD5 md5 = MD5.Create();

            List<string> checkList = new List<string>();

            if (args.Length > 1) {
                for (int i = 1; i < args.Length; ++i) {
                    try {
                        if ((File.GetAttributes(args[i]) & FileAttributes.Directory) != 0) {
                            if (args[i][args[i].Length - 1] == '/' || args[i][args[i].Length - 1] == '\\')
                                GetDictionaryFilesRecursive(args[i], checkList);
                            else
                                GetDictionaryFiles(args[i] + '/', checkList);
                        } else
                            checkList.Add(args[i]);
                    } catch (FileNotFoundException) {
                        WriteLineWithColor($"File {args[i]} is not exists.", ConsoleColor.Yellow);
                    }
                }
            } else {
                ConsoleInputPrompt();

                for (string path; (path = Input("> ")) != "end";) {
                    try {
                        if ((File.GetAttributes(path) & FileAttributes.Directory) != 0) {
                            if (path[path.Length - 1] == '/' || path[path.Length - 1] == '\\')
                                GetDictionaryFilesRecursive(path, checkList);
                            else
                                GetDictionaryFiles(path + '/', checkList);
                        } else
                            checkList.Add(path);
                    } catch (FileNotFoundException) {
                        WriteLineWithColor($"File {path} is not exists.", ConsoleColor.Yellow);
                    }
                }
            }

            CheckFilesListSame(md5, checkList);

            ConsolePause();
            return 0;
        }
    }
}