using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    public partial class FacultiesPageViewModel : ObservableRecipient, IFrameViewModel
    {
        public FacultiesPageViewModel()
        {

        }

        public async Task LoadContentAsync()
        {
            await Task.Delay(200);
            throw new Exception("Hello");
        }
    }
}
