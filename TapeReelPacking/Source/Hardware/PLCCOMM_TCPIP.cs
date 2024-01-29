using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace TapeReelPacking.Source.Hardware
{
    using TapeReelPacking.Source.LogMessage;
    using System.Threading;

    class PLCCOMM_TCPIP
    {
        public CommInterface commSequence;
        string m_IpAddress;
        public int m_Port;
        const int BUFLEN = 255;
        Thread CommSequenceThread;

    }
}
