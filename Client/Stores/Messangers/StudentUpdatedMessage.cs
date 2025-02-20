using Client.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Client.Stores.Messangers
{
    public class StudentUpdatedMessage : ValueChangedMessage<StudentRegistryInfo>
    {
        public StudentUpdatedMessage(StudentRegistryInfo value) : base(value) { }
    }
}
