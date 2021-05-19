using System.IO;
using System.Linq;
using System.Collections.Generic;

using Markdig;

namespace Guilded.NET.Website {
    /// <summary>
    /// A page document.
    /// </summary>
    public class GuildedDocument {
        /// <summary>
        /// A built Markdown pipeline.
        /// </summary>
        /// <returns>Markdown pipeline</returns>
        protected static readonly MarkdownPipeline DocumentMarkdown =
            new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseFootnotes()
                .UseAutoLinks()
                .UsePipeTables()
                .UseAutoIdentifiers()
                .UseEmphasisExtras()
                .UseGenericAttributes()
                .UseSoftlineBreakAsHardlineBreak()
                .Build();
        /// <summary>
        /// The directory this document is in.
        /// </summary>
        /// <value>Document directory</value>
        public DirectoryInfo InDirectory
        {
            get; set;
        }
        /// <summary>
        /// A page document.
        /// </summary>
        /// <param name="dir">Directory this document is in</param>
        public GuildedDocument(DirectoryInfo dir) =>
            InDirectory = dir;
        /// <summary>
        /// Builds this document from given files.
        /// </summary>
        /// <returns>Document</returns>
        public Dictionary<string, string> Build() {
            // All files in the directory
            FileInfo[] files = InDirectory.GetFiles();
            // Gets all files as { file_name, file_source } dictionary
            IEnumerable<KeyValuePair<string, string>> dict = files.Select(file => {
                // Gets file's name
                string filename = Path.GetFileName(file.Name);
                // Gets file's source
                string source = File.ReadAllText(file.FullName);
                // Uses Markdown pipeline to make a new source
                string md = Markdown.ToHtml(source, DocumentMarkdown);
                // Returns key value pair of the name and the source
                return new KeyValuePair<string, string>(filename, md);
            });
            // Turns it to a normal dictionary
            return dict.ToDictionary(x => x.Key, x => x.Value);
        }
        /// <summary>
        /// Applies Markdown parts to the given HTML document.
        /// </summary>
        /// <param name="source">HTML document source</param>
        /// <param name="docDict">A dictionary of Markdown parts</param>
        /// <returns>Document with parts</returns>
        public static string With(string source, Dictionary<string, string> docDict) {
            // A new source with the dictionary applied
            string newSource = source;
            // Adds in the parts
            foreach(KeyValuePair<string, string> file in docDict)
                newSource = newSource.Replace($"<!-- Template: {file.Key} -->", file.Value);
            // Returns the new source with the new parts
            return newSource;
        }
        /// <summary>
        /// Checks if the given directory is a document directory.
        /// </summary>
        /// <param name="dir">Directory to check</param>
        /// <returns>Is document directory</returns>
        public static bool IsDocument(DirectoryInfo dir) =>
            dir.GetFiles().Any(f => Path.GetExtension(f.FullName).ToLower() == ".md");
        /// <summary>
        /// Gets all documents from a directory.
        /// </summary>
        /// <param name="list">A list to add all documents to</param>
        /// <param name="dir">Directory to get documents from</param>
        /// <returns>List of documents</returns>
        public static List<GuildedDocument> Fetch(List<GuildedDocument> list, DirectoryInfo dir) {
            // If it is a document directory, add it as a document
            if(IsDocument(dir))
                list.Add(new GuildedDocument(dir));
            // Gets sub-directories
            foreach(DirectoryInfo subdir in dir.GetDirectories())
                Fetch(list, subdir);
            // Returns the list
            return list;
        }
    }
}