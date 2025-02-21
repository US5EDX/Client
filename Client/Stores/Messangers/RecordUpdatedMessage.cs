using Client.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Stores.Messangers
{
    public class RecordUpdatedMessage : ValueChangedMessage<RecordInfo>
    {
        public RecordUpdatedMessage(RecordInfo value) : base(value) { }
    }
}
