#region

using System;
using System.IO;
using System.Net;

#endregion

namespace Azure.Updater
{
    internal class WebData
    {
        public static event BytesDownloadedEventHandler bytesDownloaded;

        public static bool downloadFromWeb(string URL, string file, string targetFolder)
        {
            try
            {
                byte[] downloadedData;

                downloadedData = new byte[0];

                //open a data stream from the supplied URL
                WebRequest webReq = WebRequest.Create(string.Format("{0}{1}", URL, file));
                WebResponse webResponse = webReq.GetResponse();
                Stream dataStream = webResponse.GetResponseStream();

                //Download the data in chuncks
                byte[] dataBuffer = new byte[1024];

                //Get the total size of the download
                int dataLength = (int)webResponse.ContentLength;

                //lets declare our downloaded bytes event args
                ByteArgs byteArgs = new ByteArgs();

                byteArgs.downloaded = 0;
                byteArgs.total = dataLength;

                //we need to test for a null as if an event is not consumed we will get an exception
                if (bytesDownloaded != null)
                {
                    bytesDownloaded(byteArgs);
                }

                //Download the data
                MemoryStream memoryStream = new MemoryStream();
                while (true)
                {
                    //Let's try and read the data
                    int bytesFromStream = dataStream.Read(dataBuffer, 0, dataBuffer.Length);

                    if (bytesFromStream == 0)
                    {
                        byteArgs.downloaded = dataLength;
                        byteArgs.total = dataLength;
                        if (bytesDownloaded != null)
                        {
                            bytesDownloaded(byteArgs);
                        }

                        //Download complete
                        break;
                    }
                    //Write the downloaded data
                    memoryStream.Write(dataBuffer, 0, bytesFromStream);

                    byteArgs.downloaded = bytesFromStream;
                    byteArgs.total = dataLength;
                    if (bytesDownloaded != null)
                    {
                        bytesDownloaded(byteArgs);
                    }
                }

                //Convert the downloaded stream to a byte array
                downloadedData = memoryStream.ToArray();

                //Release resources
                dataStream.Close();
                memoryStream.Close();

                //Write bytes to the specified file
                FileStream newFile = new FileStream(string.Format("{0}{1}", targetFolder, file), FileMode.Create);
                newFile.Write(downloadedData, 0, downloadedData.Length);
                newFile.Close();

                return true;
            }
            catch (Exception)
            {
                //We may not be connected to the internet
                //Or the URL may be incorrect
                return false;
            }
        }
    }
}