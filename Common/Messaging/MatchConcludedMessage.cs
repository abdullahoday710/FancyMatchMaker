using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Messaging
{
    public class MatchConcludedMessage
    {
        public long? WinnerPlayerID { get; set; }
        public required List<long> ParticipatingPlayerIDs { get; set; }
        public required string MatchUUID { get; set; }

    }
}
