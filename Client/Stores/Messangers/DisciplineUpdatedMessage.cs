using Client.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Client.Stores.Messangers
{
    public class DisciplineUpdatedMessage : ValueChangedMessage<DisciplineFullInfo>
    {
        public DisciplineUpdatedMessage(DisciplineFullInfo value) : base(value) { }
    }
}
