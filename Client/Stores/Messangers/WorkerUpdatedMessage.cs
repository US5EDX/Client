using Client.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Client.Stores.Messangers
{
    public class WorkerUpdatedMessage : ValueChangedMessage<UserFullInfo>
    {
        public WorkerUpdatedMessage(UserFullInfo value) : base(value) { }
    }
}
