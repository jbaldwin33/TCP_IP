using System;

namespace CodingChallengeV2Client
{
    //enum to store error codes
    public enum Codes
    {
        Zero = 0x00, One = 0x01, Two = 0x02, Three = 0x03, Four = 0x04, Five = 0x05, Six = 0x06, Seven = 0x07, Eight = 0x08, Nine = 0x09
    };

    //helper class to format output in the status box
    public class OutputCode
    {
        string timestamp;
        string typeOfOutput;
        string info;

        public OutputCode(string timestamp, string typeOfOutput, string info)
        {
            this.timestamp = timestamp;
            this.typeOfOutput = typeOfOutput;
            this.info = info;
        }

        public static OutputCode StatusOutput(Codes code)
        {
            string timestamp = DateTime.UtcNow.ToString("HH:mm:ss.ffff");
            string type = "";
            string info = "";

            switch (code)
            {
                case Codes.Zero:
                    type = "Information";
                    info = "Request completed successfully";
                    break;
                case Codes.One:
                    type = "Error";
                    info = "Invalid header was received";
                    break;
                case Codes.Two:
                    type = "Error";
                    info = "Unsupported protocol version was received";
                    break;
                case Codes.Three:
                    type = "Error";
                    info = "Unsupported protocol operation was received";
                    break;
                case Codes.Four:
                    type = "Error";
                    info = "Timed out waiting for more data / Incomplete data length received";
                    break;
                case Codes.Five:
                    type = "Error";
                    info = "Maximum request length has been exceeded";
                    break;
                case Codes.Six:
                    type = "Error";
                    info = "Invalid checksum was received";
                    break;
                case Codes.Seven:
                    type = "Error";
                    info = "Encode operation failed";
                    break;
                case Codes.Eight:
                    type = "Error";
                    info = "Decode operation failed";
                    break;
                case Codes.Nine:
                    type = "Error";
                    info = "Server responded but maximum response length after operation exceeds maximum allowed response length";
                    break;
                default:
                    type = "Error";
                    info = "Unknown error occurred";
                    break;
            }
            return new OutputCode(timestamp, type, info);
        }
        override public string ToString()
        {
            return timestamp + "\t" + typeOfOutput + "\t" + info;
        }
    }
}
