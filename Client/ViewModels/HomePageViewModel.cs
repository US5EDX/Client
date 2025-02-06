using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    public partial class HomePageViewModel : ObservableRecipient, IFrameViewModel
    {
        public HomePageViewModel()
        {

        }
        public async Task LoadContentAsync()
        {
            await Task.Delay(3000);

        }
    }
}
