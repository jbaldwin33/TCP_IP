using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingChallengeV2Client
{
    public static class GlobalConstants
    {
        //maximum number of bytes that the user can request to account for 9 bytes of metadata
        public const uint MAX_REQUEST_SIZE = 1048567;

        //default data size
        public const uint DEFAULT_DATA_SIZE = 250000;

        //encode and decode operations
        public const byte ENCODE_OP = 0x01;
        public const byte DECODE_OP = 0x02;

        //size of request metadata
        public const int REQUEST_METADATA_SIZE = 8;
        //size of response metadata
        public const int RESPONSE_METADATA_SIZE = 7;
    }
}
