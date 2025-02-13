using Client.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Client.Stores.Messangers
{
    public class WorkerUpdatedMessage : ValueChangedMessage<WorkerFullInfo>
    {
        public WorkerUpdatedMessage(WorkerFullInfo value) : base(value) { }
    }
}
