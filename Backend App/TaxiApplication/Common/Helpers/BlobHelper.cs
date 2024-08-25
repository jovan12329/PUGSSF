using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public class BlobHelper
    {

        private CloudStorageAccount storageAccount;
        private CloudBlobClient blobStorage;

        public BlobHelper()
        {
            storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("DataConnectionString"));
            blobStorage = storageAccount.CreateCloudBlobClient();
        }

        public CloudBlockBlob GetBlockBlobReference(string containerName, string blobName)
        {
            CloudBlobContainer container = blobStorage.GetContainerReference(containerName);
            container.CreateIfNotExistsAsync().GetAwaiter().GetResult();
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            return blob;

        }


        public async Task<byte[]> DownloadImage(string id, string nameOfContainer)
        {

            CloudBlockBlob blob = GetBlockBlobReference(nameOfContainer, $"image_{id}");
            await blob.FetchAttributesAsync();
            long blobLength = blob.Properties.Length;
            byte[] byteArray = new byte[blobLength];
            await blob.DownloadToByteArrayAsync(byteArray, 0);

            return byteArray;

        }



    }
}
