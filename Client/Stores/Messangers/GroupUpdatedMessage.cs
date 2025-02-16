using Client.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Client.Stores.Messangers
{
    public class GroupUpdatedMessage : ValueChangedMessage<GroupWithSpecialtyInfo>
    {
        public GroupUpdatedMessage(GroupWithSpecialtyInfo value) : base(value) { }
    }
}
