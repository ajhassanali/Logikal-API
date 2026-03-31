using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Shared;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class CalculatedElementPricelistViewModel : ViewModelBase
    {
        public ImageSource Preview { get; set; }

        public SelectionAdapter<ICalculatedElementPricelistPrice> Prices { get; } =
            new SelectionAdapter<ICalculatedElementPricelistPrice>();

        public CalculatedElementPricelistViewModel(ICalculatedElementPricelistResult calculatedElementPricelist)
        {
            Prices.ItemsSource =
                new ObservableCollection<ICalculatedElementPricelistPrice>(calculatedElementPricelist.Prices);

            Preview = InitPreview(calculatedElementPricelist);
        }

        private ImageSource InitPreview(ICalculatedElementPricelistResult calculatedElementPricelist)
        {
            try
            {
                var previewStream = calculatedElementPricelist.Stream;
                var bitmapImage = new BitmapImage();
                if (previewStream.Length == 0)
                    return bitmapImage;

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = previewStream;
                bitmapImage.EndInit();
                return bitmapImage;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }

            return null;
        }
    }
}
