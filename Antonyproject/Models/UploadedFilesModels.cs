using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Antonyproject.Models
{
    public class UploadedFilesModels
    {

        public int FileId { get; set; }
        public string FileName { get; set; }
        public int FileSize { get; set; }
        public string ContentType { get; set; }
        public string FileExtension { get; set; }
        public byte[] FileContent { get; set; }
        public string UserName { get; set; }
    }
}