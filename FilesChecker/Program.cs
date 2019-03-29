using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace FilesChecker {
    public class Program {
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
            Dictionary<string, uint> hashCodeDict = new Dictionary<string, uint>();
            Dictionary<string, StringBuilder> hashCodeFileList = new Dictionary<string, StringBuilder>();
            foreach (string path in filesPath) {
                try {
                    Console.WriteLine($"Computing {path} ...");
                    string md5Code = GetMD5HashCode(md5Hash, new FileStream(path, FileMode.Open));
                    if (!hashCodeDict.ContainsKey(md5Code)) {
                        hashCodeDict.Add(md5Code, 1);
                        hashCodeFileList.Add(md5Code, new StringBuilder(path + "\n"));
                    } else {
                        ++hashCodeDict[md5Code];
                        hashCodeFileList[md5Code].Append(path + "\n");
                    }
                } catch (FileNotFoundException) {
                    Console.WriteLine($"File {path} is not exists.");
                }
            }
            if (hashCodeDict.Count == 0) {
                Console.WriteLine("Not any files exist.");
                return;
            }

            List<string> pluralMD5 = new List<string>();
            foreach (string e in hashCodeDict.Keys)
                if (hashCodeDict[e] > 1)
                    pluralMD5.Add(e);
            if (pluralMD5.Count == 0) {
                Console.WriteLine("Not any files are same.");
                return;
            }
            Console.WriteLine();
            foreach (string e in pluralMD5) 
                Console.WriteLine($"{hashCodeFileList[e].ToString()}are same(md5: {e})\n===================");
        }

        private static void ConsolePause() {
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }

        private static void GetDictionaryFilesDeep(string path, List<string> output) {
            foreach (string e in Directory.GetFiles(path))
                output.Add(e);
            foreach (string e in Directory.GetDirectories(path))
                GetDictionaryFilesDeep(e, output);
        }

        private static int Main(string[] args) {
            using (MD5 md5 = MD5.Create()) {

                List<string> checkList = new List<string>();

                Console.WriteLine("Add checking file(single line filename to one file, " +
                    "dictionary name to a dictionary, end to start checking):");

                for (string path; (path = Console.ReadLine()) != "end";) {
                    if (File.GetAttributes(path) == FileAttributes.Directory)
                        GetDictionaryFilesDeep(path, checkList);
                    else
                        checkList.Add(path);
                }

                CheckFilesListSame(md5, checkList);
            }

            ConsolePause();
            return 0;
        }
    }
}
