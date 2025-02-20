using Client.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Client.Stores.Messangers
{
    public class StudentsAddedMessage : ValueChangedMessage<IEnumerable<StudentRegistryInfo>>
    {
        public StudentsAddedMessage(IEnumerable<StudentRegistryInfo> value) : base(value) { }
    }
}
