using IrmaWeb.Models;
using IrmaWeb.Models.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace IrmaWeb.Classes
{
    /// <summary>
    /// Class that manages the generation of the final ZIP-File Response to Web Client
    /// </summary>
    public class ZipResponse
    {
        private List<RestFileModel> _fileCollection;
        private List<RestErrorModel> _fileSkipCollection;

        /// <summary>
        /// Constructor for a Zip response instance
        /// </summary>
        /// <param name="fileCollection"></param>
        /// <param name="fileSkipCollection"></param>
        public ZipResponse(List<RestFileModel> fileCollection, List<RestErrorModel> fileSkipCollection)
        {
            _fileCollection = fileCollection;
            _fileSkipCollection = fileSkipCollection;
        }

        /// <summary>
        /// Generates the final zip-file as a binary array from the file collections that were given over constructor
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateContainer()
        {
            //Create Empty File Log
            MemoryStream SkipLogFile = new MemoryStream();
            StreamWriter LogStreamWriter = new StreamWriter(SkipLogFile, System.Text.Encoding.UTF8);

            int _totalCount = _fileSkipCollection.Count + _fileCollection.Count;
            int _skipCount = _fileSkipCollection.Count;

            LogStreamWriter.Write("Generated at: {0} (Server Time)", DateTimeOffset.Now);
            LogStreamWriter.Write("{2}{0} from {1} files were skipped, because of received errors: {2}{2}", _skipCount, _totalCount, "\r\n");
            //Note: Forced CR LF linebreak on linux hosts (Don't use Environment.NewLine!)

            foreach (var skippedItem in _fileSkipCollection)
            {
                LogStreamWriter.Write("{0} - {1} ({2}) - Code {3}{4}",
                    skippedItem.Name, skippedItem.Error.Message, skippedItem.Error.Description, skippedItem.Error.Code, "\r\n");
            }
            
            LogStreamWriter.Flush();
            if (SkipLogFile.Length != 0) _fileCollection.Add(new RestFileModel { Name = "Log.txt", Data = SkipLogFile.ToArray() });

            MemoryStream buildMemorySegment = new MemoryStream();
            using (ZipArchive archive = new ZipArchive(buildMemorySegment, ZipArchiveMode.Create, true))
            {
                foreach (var rawFile in _fileCollection)
                {
                    ZipArchiveEntry fileInArchive = archive.CreateEntry(rawFile.Name, RuntimeSettings.YamiStatic.Settings.GetZipCompressionLevel);
                    using (Stream entryStream = fileInArchive.Open())
                    using (MemoryStream fileToCompressStream = new MemoryStream(rawFile.Data))
                    {
                        fileToCompressStream.CopyTo(entryStream);
                    }
                }
            }

            return buildMemorySegment.ToArray();
        }

    }
}