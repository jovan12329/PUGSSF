using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public class ImageStreamConverter
    {

        public static BlobFile ImgStreamConverter(IFormFile file)
        {
            BlobFile fileOverNetwork;

            using (var stream = file.OpenReadStream())
            {
                byte[] fileContent;
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    fileContent = memoryStream.ToArray();
                }

                fileOverNetwork = new BlobFile(file.FileName, file.ContentType, fileContent);
            }

            return fileOverNetwork;
        }

    }
}
