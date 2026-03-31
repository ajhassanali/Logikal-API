using System;
using System.IO;
using Ofcas.Lk.Api.Client.Demo.Mvvm;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class SaveFileDialogViewModel : ViewModelBase
    {
        public string DefaultExt { get; set; }
        public string Filter { get; set; }
        public string FileName { get; set; }

        public SaveFileDialogViewModel()
        {
        }

        public string GetExtension(bool withDot)
        {
            var extension = Path.GetExtension(FileName);
            if (string.IsNullOrEmpty(extension))
                return string.Empty;

            return withDot ? extension : extension.Remove(0, 1);
        }

        /// <summary>
        ///     Adds a file extension filter to the SaveFileDialog.
        /// </summary>
        /// <param name="fileType">The type of file to save.</param>
        /// <param name="fileExtension">The extension of the file without dot.</param>
        public void AddFilter(string fileExtension, bool isDefault = false, string fileType = "")
        {
            if (string.IsNullOrWhiteSpace(fileExtension))
                throw new ArgumentException("File extension must not be empty.", nameof(fileExtension));

            if (string.IsNullOrWhiteSpace(fileType))
                fileType = $"{fileExtension.ToUpperInvariant()} Files";

            var additionalFilter = $"{fileType}|*.{fileExtension.ToLowerInvariant()}";
            if (string.IsNullOrEmpty(Filter))
                Filter = additionalFilter;
            else
                Filter += $"|{additionalFilter}";

            if (isDefault)
                DefaultExt = $".{fileExtension.ToLowerInvariant()}";
        }
    }
}
