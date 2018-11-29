using System;
using System.Collections.Generic;
using Ionic.Zip;
using System.IO;

//requires .net 4.6.2 to support long paths > 260 characters

namespace CxZip
{
    class Program
    {
        static string VERSION = "1.5";
        static string dest = "";
        static string src = "";
        static string whitelist_src = "";
        static List<cxFile> file_list = new List<cxFile>();
        static List<string> whiteList = new List<string>();

        static int longest_path = 0;
        static int file_count = 0;

        static void Main(string[] args)
        {
            if (args.Length == 3 || args.Length == 2)
            {
                try
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();

                    src = args[0]; //required
                    dest = args[1]; //required

                    if (args.Length == 2)
                        whitelist_src = "CxExt.txt"; //added to emulate existing CxZip functionality, which looks for CxExt.txt in working directory
                    else
                        whitelist_src = args[2];  //optional; uses CxExt.txt if not provided

                    buildWhiteList(whitelist_src);
                    main();

                    watch.Stop();
                    TimeSpan t = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);

                    Console.WriteLine();
                    Console.WriteLine("Files added to archived:  " + file_count);
                    Console.WriteLine("Length of longest path in archive:  " + longest_path);
                    Console.WriteLine("Time to complete:  " + string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", t.Hours, t.Minutes, t.Seconds, t.Milliseconds));
                }//end try
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.Write(ex.StackTrace);
                }//end catch
            }//end if
            else
                usage();
        }//end Main

        private static void usage()
        {
            Console.WriteLine("CxZip for Windows supporting long paths (> 260 characters)");
            Console.WriteLine("version " + VERSION);
            Console.WriteLine("(c) 2018 Checkmarx | www.checkmarx.com");
            Console.WriteLine("Runs on .Net Framework v. 4.6.2+");
            Console.WriteLine();
            Console.WriteLine("USAGE:  CxZip [src path] [dest path] [whitelist path]");
            Console.WriteLine(@"E.g.,  CxZip c:\mycode C:\mycode.zip C:\extensions.txt");
        }//end usage

        private static void main()
        {
            try
            {
                ProcessDirectory(src);
            }//end try
            catch (Exception ex) { throw ex; }

            using (ZipFile archive = new ZipFile())
            {
                Console.WriteLine("Total files to add:  " + file_list.Count);
                foreach (cxFile f in file_list)
                {
                    string rel_f = f.path.Substring(0, f.path.LastIndexOf(f.name)).Replace(src, "");
                    if ((rel_f + f.name).Length > 220)
                        Console.WriteLine("Adding: " + rel_f + f.name);

                    if ((rel_f + f.name).Length > longest_path)
                        longest_path = (rel_f + f.name).Length;

                    try
                    {
                        archive.AddFile(f.path, rel_f);
                    }//end try
                    catch(Exception ex) { throw ex; }
                }//end foreach
                Console.WriteLine();
                Console.WriteLine("Compressing files...");

                archive.UseZip64WhenSaving = Zip64Option.AsNecessary; //add in v1.4
                archive.Save(dest);
                Console.WriteLine("Archive saved.");
            }//end using
        }//end main

        private static void buildWhiteList(string path)
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                foreach (string line in lines)
                    whiteList.Add(line.ToLower());
            }//end try
            catch(Exception ex) { throw ex; }
        }//end buildWhiteList

        private static void ProcessDirectory(string targetDirectory)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }//end ProcessDirectory

        private static void ProcessFile(string path)
        {
            cxFile f = new cxFile(path);
            if (whiteList.Contains(f.extension.ToLower()))
            {
                file_list.Add(new cxFile(path));
                file_count++;
            }//end if
        }//end processFile
    }//end Program

    class cxFile
    {
        public string path, name;
        public string extension;

        public cxFile(string path)
        {
            this.path = path;
            this.name = new FileInfo(path).Name;
            try
            {
                this.extension = path.Substring(path.LastIndexOf("."));
            }//end try
            catch { this.extension = ""; }
        }//end cxFile
    }//end cxFile
}//end CxZip
