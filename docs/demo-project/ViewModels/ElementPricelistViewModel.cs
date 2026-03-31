using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Models.Interfaces;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ElementPricelistViewModel : CoreObjectViewModel, IExtendedDisposable
    {
        private ICoreObjectResult<IElementPricelist> ElementPriceListResult;
        private IElementPricelist ElementPricelist => ElementPriceListResult.CoreObject; 

        private ElementPricelistModel _elementPricelistModel;

        public ElementPricelistModel ElementPricelistModel
        {
            get { return _elementPricelistModel; }
            set { _elementPricelistModel = value; OnPropertyChanged(); }
        }

        public ICommand ExportThumbnailCommand { get; }

        public event EventHandler ElementPricelistRefreshed;

        public ElementPricelistViewModel(IViewProvider viewProvider, ICoreObjectResult<IElementPricelist> elementPricelist)
            : base(viewProvider, elementPricelist)
        {
            Throw.IfNull(elementPricelist, nameof(elementPricelist));
            ElementPriceListResult = elementPricelist;

            Refresh(false, true);

            ExportThumbnailCommand = new Command(ExportThumbnail);
        }

        private void ExportThumbnail()
        {
            CatchException(() =>
            {
                var saveFileDialogViewModel = ViewProvider.ViewModelFactory.GetSaveFileDialogViewModel("Image Files | *.png; *.jpg; *.emf", ".png");

                if (!ShowDialog(saveFileDialogViewModel)) return;

                var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(new ObservableCollection<IParameter>
                {
                    new Parameter<string>
                    {
                        Key = WellKnownParameterKey.ElementPricelist.Thumbnail.Format,
                        IsRequired = true,
                        Value = saveFileDialogViewModel.GetExtension(false).ToUpper()
                    },
                    new Parameter<double>
                    {
                        Key = WellKnownParameterKey.ElementPricelist.Thumbnail.Height,
                        IsRequired = true
                    },
                    new Parameter<double>
                    {
                        Key = WellKnownParameterKey.ElementPricelist.Thumbnail.Width,
                        IsRequired = true
                    },
                    new Parameter<bool>
                    {
                        Key = WellKnownParameterKey.ElementPricelist.Thumbnail.SameRatio,
                        IsRequired = true
                    }
                });

                if (!ShowDialog(parametersViewModel)) return;

                var parameters = parametersViewModel.GetParameters();
                using (var thumbnailStream = ElementPricelist.GetThumbnail(parameters).Stream)
                {
                    using (var fileStream = new FileStream(saveFileDialogViewModel.FileName, FileMode.Create))
                    {
                        thumbnailStream.CopyTo(fileStream);
                    }
                }
            });
        }

        private void Refresh(bool hardRefresh = false, bool suppressEvent = false)
        {
            if (hardRefresh)
                ElementPricelist.Refresh();

            ElementPricelistModel = new ElementPricelistModel
            {
                CoreObjectId = ElementPricelist.Id,
                Guid = ElementPricelist.Info.Guid,
                Name = ElementPricelist.Info.Name,
                System = ElementPricelist.Info.System,
                Path = ElementPricelist.Info.Path,
                Thumbnail = InitThumbnail()
            };

            if (!suppressEvent)
                ElementPricelistRefreshed?.Invoke(this, EventArgs.Empty);
        }

        private ImageSource InitThumbnail()
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    [WellKnownParameterKey.ElementPricelist.Thumbnail.Format] = "PNG",
                    [WellKnownParameterKey.ElementPricelist.Thumbnail.Height] = 300,
                    [WellKnownParameterKey.ElementPricelist.Thumbnail.Width] = 300,
                    [WellKnownParameterKey.ElementPricelist.Thumbnail.SameRatio] = true
                };

                var thumbnailStream = ElementPricelist.GetThumbnail(parameters).Stream;
                var bitmapImage = new BitmapImage();
                if (thumbnailStream.Length == 0)
                    return bitmapImage;

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = thumbnailStream;
                bitmapImage.EndInit();
                return bitmapImage;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }

            return null;
        }

        public bool CanDispose(out string falseReason)
        {
            return CanDispose(ElementPriceListResult, out falseReason);
        }

        public void Dispose()
        {
            ElementPriceListResult.Dispose();
            ElementPriceListResult = null;
        }
    }
}
