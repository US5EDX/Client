using Client.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Client.Stores.Messangers
{
    public class GroupUpdatedMessage : ValueChangedMessage<GroupFullInfo>
    {
        public GroupUpdatedMessage(GroupFullInfo value) : base(value) { }
    }
}
