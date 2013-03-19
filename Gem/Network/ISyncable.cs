using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.Network
{
    public interface ISyncable
    {
        UInt32 EntityID { get; }
        UInt32 SyncID { get; }
        void writeFullSync(WriteOnlyDatagram datagram);
        void readFullSync(ReadOnlyDatagram datagram);
    }
}
