using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels.Interfaces
{
    public interface IPageViewModel : INotifyPropertyChanged
    {
        bool IsActive { get; set; }
    }
}
