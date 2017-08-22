using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Amazon.S3;
using System.IO;

namespace WannaGo.Utility
{
    public class S3Uploader
    {
        string bucketName = "wannago-images";

        AmazonS3Client client;

        public S3Uploader()
        {
            client = new AmazonS3Client("key", "secret", Amazon.RegionEndpoint.USEast1);
        }


       public string GetBaseUrl()
        {
            return "http://" + bucketName + ".s3.amazonaws.com/";
        }

        public async Task<bool> UploadFile(MemoryStream stream, string keyName)
        {
            try
            {
                PutObjectRequest request = new PutObjectRequest()
                {
                    InputStream = stream,
                    Key = keyName,
                    BucketName = bucketName,
                };
                var response = await client.PutObjectAsync(request);
                
                return true;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Check the provided AWS Credentials.");
                    Console.WriteLine(
                        "For service sign up go to http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine(
                        "Error occurred. Message:'{0}' when writing an object"
                        , amazonS3Exception.Message);
                }
            }
            return false;
        }


    }
}
