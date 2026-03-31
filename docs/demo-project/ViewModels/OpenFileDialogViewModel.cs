using System.IO;
using Ofcas.Lk.Api.Client.Demo.Mvvm;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class OpenFileDialogViewModel : ViewModelBase
    {
        public string Filter { get; set; }
        public string FileName { get; set; }

        public string GetExtension(bool withDot)
        {
            var extension = Path.GetExtension(FileName);
            if (string.IsNullOrEmpty(extension))
                return string.Empty;

            return withDot ? extension : extension.Remove(0, 1);
        }
    }
}
