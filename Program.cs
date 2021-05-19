using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Guilded.NET.Website
{
    class Program
    {
        static readonly string relative = "<!-- relative -->", template = "./template.html";
        static void Main(string[] args)
        {
            // Gets path as string
            string pathStr = args.Length != 0 ? string.Join(" ", args) : "src";
            // Gets the full path to the directory
            string path = Path.GetFullPath(pathStr);
            // Tells us what dir it is compiling
            Console.WriteLine("Starting the project. Compiling `{0}` directory.", path);
            // If directory doesn't exist, throw an exception
            if(!Directory.Exists(path)) throw new FileLoadException($"Could not find a source directory. Full path: {path}");
            // All of the documents
            List<GuildedDocument> docs = GuildedDocument.Fetch(new List<GuildedDocument>(), new DirectoryInfo(path));
            // Tells us how many documents were found
            Console.WriteLine("Found {0} documents", docs.Count);
            // Creates new encoding for file writing
            UTF8Encoding encoding = new(true);
            // Gets template source
            string templateSrc = File.ReadAllText(Path.Join(path, template));
            // Tells us that it started writing documents
            Console.WriteLine("Writing documents:");
            // Gets all of the documents
            foreach(GuildedDocument doc in docs) {
                // Relative path of the directory
                string dirRelative = Path.GetRelativePath(path, doc.InDirectory.FullName);
                // Tells us that it started writing
                Console.WriteLine("    -----\n    Writing a new document: {0}", dirRelative);
                // Gets directory name
                string name = Path.GetDirectoryName(dirRelative);
                // To what it should replace <!-- relative -->
                string relativeVar = "./";
                // If it is in a child directory
                if(!string.IsNullOrWhiteSpace(name)) {
                    // Gets new path relative to ./page
                    string inPage = Path.GetFullPath(Path.Combine("./page", dirRelative, ".."));
                    // Tells us what directory it is creating
                    Console.WriteLine("    Creating directory: {0}", inPage);
                    // Creates a directory if it doesn't exist
                    Directory.CreateDirectory(inPage);
                    // Sets new relative path
                    relativeVar = Path.GetRelativePath(inPage, "./page").Replace("\\", "/") + "/";
                }
                // Creates a source for the page
                string source = GuildedDocument.With(templateSrc, doc.Build()).Replace(relative, relativeVar);
                // Creates a file
                using FileStream stream = File.Create(Path.Combine("./page", dirRelative + ".html"));
                // Writes the source
                stream.Write(encoding.GetBytes(source), 0, source.Length);
                // Tells us that it is done
                Console.WriteLine("    Wrote new document: {0}", name);
            }
        }
    }
}
