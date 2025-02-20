using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    public partial class AllStudentChoicesViewModel : ObservableRecipient, IFrameViewModel
    {
        public async Task LoadContentAsync()
        {
            await Task.Delay(2000);

            throw new NotImplementedException();
        }
    }
}
