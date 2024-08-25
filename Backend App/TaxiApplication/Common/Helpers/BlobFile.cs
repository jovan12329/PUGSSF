using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    [DataContract]
    public class BlobFile
    {

        [DataMember]
        public string? FileName { get; set; }

        [DataMember]
        public string? ContentType { get; set; }

        [DataMember]
        public byte[]? FileContent { get; set; }

        public BlobFile(byte[] fileContent)
        {
            FileContent = fileContent;
        }

        public BlobFile(string fileName, string contentType, byte[] fileContent)
        {
            FileName = fileName;
            ContentType = contentType;
            FileContent = fileContent;
        }

        public BlobFile()
        {
        }

    }
}
