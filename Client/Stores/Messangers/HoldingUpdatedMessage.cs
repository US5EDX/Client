using Client.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Client.Stores.Messangers
{
    public class HoldingUpdatedMessage : ValueChangedMessage<HoldingInfo>
    {
        public HoldingUpdatedMessage(HoldingInfo value) : base(value) { }
    }
}
