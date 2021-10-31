using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Ionic.Zip;

namespace QC_Scale_Editor
{
    class Program
    {
        public static string FolderPath;
        public static List<string> QCList = new List<string>();
        public static string Scale;
        static void Main(string[] args)
        {
            Console.Title = "QC-Scale-Editor Built for gmod-HerobrinesMansion";
            while (true)
            {
                Console.WriteLine("Please enter folder full of QC files:");
                FolderPath = @Console.ReadLine();
                if (Directory.Exists(FolderPath))
                {
                    break;
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Invalid path, try again please!");
                    continue;
                }
            }
            Console.Clear();
            while (true)
            {
                Console.WriteLine("Please enter the new scale number (1 = Full Size | 0.5 = Half Size)");
                Scale = Console.ReadLine();
                if(Scale.Length > 8)
                {
                    Console.WriteLine("Input over 8 characters, try again!");
                    continue;
                }
                break;
            }
            Console.Clear();
            string[] AllQCFiles = @System.IO.Directory.GetFiles(@FolderPath, "*.qc");
            for(int i = 0; i < AllQCFiles.Length; i++)
            {
                QCList.Add(@AllQCFiles[i]);
                Console.WriteLine("Added \"{0}\"", @AllQCFiles[i]);
            }
            Console.WriteLine("\n\nAmount of QC Files: {0}\nIs this okay? (Type 'yes' or 'no')", QCList.Count);

            //need to make sure the user is ok with this
            while (true)
            {
                bool UserMessedUp = false;
                switch (Console.ReadLine()[0])
                {
                    case 'y':
                    case 'Y':
                        break;
                    case 'n':
                    case 'N':
                        Console.WriteLine("Press enter to restart...");
                        Console.ReadLine();
                        Main(null);
                        break;
                    default:
                        UserMessedUp = true;
                        break;
                }
                if (UserMessedUp)
                {
                    Console.WriteLine("Invalid Input, try again!");
                    continue;
                }
                break;
            }
            //time to make backup
            string backupzip;
            while (true)
            {
                Random rnd = new Random();
                backupzip = @FolderPath + "QCFILEBACKUP_" + DateTime.Now.ToString("yyyy_MM_dd") +rnd.Next(0,10000)+ ".zip";
                if (File.Exists(backupzip))
                    continue;
                break;
            }
            using (var zip = new Ionic.Zip.ZipFile())
            {
                zip.AddDirectory(@FolderPath, "QC-BACKUP");
                zip.Save(backupzip);
                zip.Dispose();
            }
            //k if theres no exceptions then theres a backup!
            Console.WriteLine("Successfully made backup at: \"{0}\"", backupzip);
            int QCCount = QCList.Count();
            byte[] ScaleByteArray = Encoding.ASCII.GetBytes(Scale);
            for (int i = 0; i < QCCount; i++)
            {
                FileStream File = new FileStream(@QCList[0], FileMode.Open);
                string buffer = "";
                long bufferposition = 0; //Position the buffer starts within the file
                for (UInt32 x = 0; x < File.Length; x++)
                {
                    switch (System.Text.Encoding.ASCII.GetString(new[] { Convert.ToByte(File.ReadByte()) }))
                    {
                        case "\n":
                            buffer = "";
                            bufferposition = File.Position;
                            break;
                        case "\t":
                            break;
                        default:
                            File.Position--;
                            buffer += System.Text.Encoding.ASCII.GetString(new[] { Convert.ToByte(File.ReadByte()) });
                            break;
                    }
                    if(buffer.Length >= 8)
                    {
                        if (buffer.Substring(0, 8) == "$scale \"")
                        {
                            Console.WriteLine("FOUND $SCALE COMMAND IN \"{0}\"", Path.GetFileName(@QCList[0]));
                            File.Position = bufferposition + 8;
                            for(int y = 0; y < Encoding.ASCII.GetBytes(Scale).Length; y++)
                            {
                                File.WriteByte(ScaleByteArray[y]);
                            }
                            Console.WriteLine("\tChanged Scale Command Successfully!");
                            break;
                        }
                    }
                }
                File.Dispose();
                QCList.RemoveAt(0);
            }
            Console.WriteLine("\n\nOperation done! Scroll up to see log!");
            Console.ReadLine();
        }
    }
}
