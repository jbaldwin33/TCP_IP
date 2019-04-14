using System;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using log4net;

namespace CodingChallengeV2Client
{
    // The following is one outline of one example of how to approach protocol implementation.

    public abstract class RequestResponse
    {
        public static Byte BuildCheckSum(Byte[] input)
        {
            //compute checksum using given algorithm
            //for the 1st byte, check the 1st bit, for the 2nd byte the 2nd bit, ...
            //for the 8th byte the 1st, for the 9th byte the 2nd bit, ...
            byte checksum = 0;
            
            //first bit is the rightmost bit
            int i = 0;
            foreach (byte b in input)
            {
                if (((b >> i) & 1) == 1)
                {
                    checksum++;
                }
                int v = (i < 7) ? i++ : i = 0;
            }
            return checksum;
        }
    }

    //helper class to check buffer before encode/decode
    public class Validation
    {
        public static OutputCode thisCode;

        public static bool CheckParams(string hostname, string port, byte[] buffer, string timeout, byte operation)
        {
            //check other parameters
            string information;
            bool valid = false;

            if (!ushort.TryParse(port, out ushort portResult))
            {
                information = "Port must be a combination of four numbers.";
            }
            //give the user a max timeout instead of saying it should be less than the allowed size of an int (4,000,000,000)
            else if (!int.TryParse(timeout, out int timeoutResult) || timeoutResult < 1000 || timeoutResult > 60000) {
                information = "Timeout must be greater than 1000 and less than 60000 and numbers only.";
            }
            else if (buffer.Length >= GlobalConstants.MAX_REQUEST_SIZE) {
                information = "Maximum request length has been exceeded";
            }
            //using ports 4544 and 4545
            else if ((hostname == "codingchallenge.identityone.net") && ((portResult == 4544) || (portResult == 4545)))
            {
                //update status box
                information = (operation == GlobalConstants.ENCODE_OP )? 
                    "Requesting to Encode " + buffer.Length + " byte(s) of data." :
                    "Requesting to Decode " + buffer.Length + " byte(s) of data.";
                valid = true;
            }
            else
            {
                information = "Check the server name and port.";
            }

            //if input contains error, update status and return; otherwise continue to check other parameters
            if (!valid)
            {
                thisCode = new OutputCode(DateTime.UtcNow.ToString("HH:mm:ss.ffff"), "Error", information);
                return false;
            }
            else
            {
                thisCode = new OutputCode(DateTime.UtcNow.ToString("HH:mm:ss.ffff"), "Information", information);
            }
            return true;
        }
    }

    //class that will parse the data received from the server
    public class Response : RequestResponse
    {
        //response structure
        readonly byte status;
        readonly uint length;
        byte[] data;

        //constructor
        public Response(byte[] responseData)
        {
            //length of data is the total size of response minus the metadata (header, status, length)
            //and minus one byte for the checksum
            length = Convert.ToUInt32(responseData.Length - GlobalConstants.RESPONSE_METADATA_SIZE - 1);
            status = responseData[2];

            //data received from the server
            data = new byte[length];
            System.Array.Copy(responseData, GlobalConstants.RESPONSE_METADATA_SIZE, data, 0, (int)length);
        }

        //getters
        public byte GetStatus()
        {
            return status;
        }
        public byte[] GetData()
        {
            return data;
        }
    }

    //An instance of this class will be used to communicate with the server
    public class SenderReceiver
    {
        // public fields
        public string hostName { get; set; }
        public ushort port { get; set; }
        public byte[] buffer { get; set; }
        public int timeout { get; set; }
        public int numOfEncodes { get; set; }
        public byte[] totalDataReceived { get; set; }

        //private fields
        private byte[] toSend;
        private byte[] metadata;
        private byte[] dataReceived;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //constructors
        public SenderReceiver() { }
        public SenderReceiver(string hostName, ushort port, byte[] buffer, int timeout)
        {
            this.hostName = hostName;
            this.port = port;
            this.buffer = buffer;
            this.timeout = timeout;
            numOfEncodes = 0;
        }

        //public methods
        public void SetProperties(string hostName, ushort port, byte[] buffer, int timeout)
        {
            this.hostName = hostName;
            this.port = port;
            this.buffer = buffer;
            this.timeout = timeout;
        }
        public void ResetEncodes()
        {
            numOfEncodes = 0;
        }
        public void IncrementEncodes()
        {
            numOfEncodes++;
        }
        public void DecrementEncodes()
        {
            numOfEncodes--;
        }

        //create request buffer
        public void CreateRequest(byte op)
        {
            //request structure
            short header = 0x1092;
            byte version = 0x01;
            //length of request is the total size of buffer plus the metadata (header, status, length)
            //plus one byte for the checksum
            uint length = Convert.ToUInt32(buffer.Length + GlobalConstants.REQUEST_METADATA_SIZE + 1);
            byte operation = op; //depends on which button was clicked

            //this buffer consists of the bytes from header to checksum and will be sent to the server
            toSend = new byte[buffer.Length + GlobalConstants.REQUEST_METADATA_SIZE + 1];

            //fill buffer with the correct bytes
            toSend[0] = (byte)(header & 255);
            toSend[1] = (byte)(header >> 8);
            toSend[2] = version;
            toSend[3] = (byte)(length & 255);
            toSend[4] = (byte)(length >> 8);
            toSend[5] = (byte)(length >> 16);
            toSend[6] = (byte)(length >> 24);
            toSend[7] = operation;

            //start copying after metadata
            System.Array.Copy(buffer, 0, toSend, GlobalConstants.REQUEST_METADATA_SIZE, buffer.Length);

            //add the checksum byte to the buffer that will be sent
            toSend[toSend.Length - 1] = RequestResponse.BuildCheckSum(toSend);
        }

        //connect to the server asynchronously
        public async Task MyConnect()
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    await Task.Run(() => tcpClient.Connect(hostName, port));

                    //stream for reading and writing
                    NetworkStream stream = tcpClient.GetStream();

                    //write data to stream so server can read it
                    stream.Write(toSend, 0, toSend.Length);

                    //receive response from server
                    //first read the metadata to confirm whether or not an error occurred
                    metadata = new byte[GlobalConstants.RESPONSE_METADATA_SIZE];

                    //keep track of the number of bytes read; use as index in byte array to avoid
                    //overwriting the data in the buffer
                    int totalRead = 0;

                    //number of bytes read from a single Read() call
                    int read = 0;
                    stream.ReadTimeout = timeout;
                    read = stream.Read(metadata, 0, metadata.Length);

                    //figure out the size of the received response
                    uint respLength1 = (Convert.ToUInt32(metadata[3]));
                    uint respLength2 = (Convert.ToUInt32(metadata[4]) << 8);
                    uint respLength3 = (Convert.ToUInt32(metadata[5]) << 16);
                    uint respLength4 = (Convert.ToUInt32(metadata[6]) << 24);
                    uint respLength = respLength1 | respLength2 | respLength3 | respLength4;

                    //get size of the encoded data which is total length minus the metadata
                    int sizeOfEncodedData = (int)respLength - metadata.Length;

                    //as long as no error occurred and the size of the data doesn't exceed the max
                    //allowed size of a 32-bit integer, continue to extract the encoded data
                    if ((metadata[2] == Convert.ToByte(Codes.Zero)) && sizeOfEncodedData < GlobalConstants.MAX_REQUEST_SIZE)
                    {
                        //catch if size is <= 0 
                        try
                        {
                            dataReceived = new byte[sizeOfEncodedData];

                            //continue to read data until you read the desired amount
                            do
                            {
                                read = stream.Read(dataReceived, totalRead, dataReceived.Length - totalRead);
                                if (read > 0)
                                {
                                    totalRead += read;
                                }
                            } while (totalRead < dataReceived.Length);

                            //combine the metadata and the encoded data into one array that will be
                            //parsed by the Response class
                            //the size of totalDataReceived will be the size of a Response which is 7 bytes plus
                            //the size of the data
                            totalDataReceived = new byte[metadata.Length + dataReceived.Length];
                            System.Array.Copy(metadata, 0, totalDataReceived, 0, metadata.Length);
                            System.Array.Copy(dataReceived, 0, totalDataReceived, metadata.Length, dataReceived.Length);
                        }
                        catch (OverflowException e)
                        {                            
                            //corrupt or unusable metadata
                            log.Info("OverflowException: {0}", e);
                            totalDataReceived = new byte[buffer.Length + metadata.Length];

                            //set status code to reflect timeout
                            totalDataReceived[2] = Convert.ToByte(Codes.Seven);
                            //use data from original buffer
                            System.Array.Copy(buffer, 0, totalDataReceived, metadata.Length, buffer.Length);
                        }
                    }
                    //if an error occurred or the size is too big, set the buffer back to the original data
                    else if ((metadata[2] != Convert.ToByte(Codes.Zero)) || sizeOfEncodedData >= GlobalConstants.MAX_REQUEST_SIZE)
                    {
                        //the size of totalDataReceived will be the size of the original data sent as
                        //a Request which was 8 bytes plus the size of the data minus 1 byte to match 
                        //the size of a Response
                        totalDataReceived = new byte[toSend.Length - 1];
                        //use data from original buffer
                        System.Array.Copy(metadata, 0, totalDataReceived, 0, metadata.Length);
                        System.Array.Copy(buffer, 0, totalDataReceived, metadata.Length, buffer.Length);
                        //if failure was caused because the size of the response was too big
                        //change error code to 0x09
                        if (totalDataReceived[2] == Convert.ToByte(Codes.Zero))
                        {
                            totalDataReceived[2] = Convert.ToByte(Codes.Nine);
                        }
                    }
                }

                catch (IOException e)
                {
                    //catch data read timeout
                    log.Info("IOException: {0}", e);
                    totalDataReceived = new byte[buffer.Length + metadata.Length];

                    //set status code to reflect timeout
                    totalDataReceived[2] = Convert.ToByte(Codes.Four);
                    //use data from original buffer
                    System.Array.Copy(buffer, 0, totalDataReceived, metadata.Length, buffer.Length);
                }
                catch (SocketException e)
                {
                    //catch data read timeout
                    log.Info("SocketException: {0}", e);
                    totalDataReceived = new byte[buffer.Length + metadata.Length];

                    //set status code to reflect timeout
                    totalDataReceived[2] = Convert.ToByte(Codes.Four);
                    //use data from original buffer
                    System.Array.Copy(buffer, 0, totalDataReceived, metadata.Length, buffer.Length);
                }
            }
        }
    }
}
