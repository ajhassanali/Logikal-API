using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ElementPricelistContainerViewModel : CoreObjectViewModel
    {
        private IElementPricelistContainer _elementPricelistContainer;

        public SelectionAdapter<IElementPricelistInfo> ElementPricelists { get; } =
            new SelectionAdapter<IElementPricelistInfo>();

        public ICommand OpenChildCommand { get; }

        public ElementPricelistContainerViewModel(IViewProvider viewProvider,
            IElementPricelistContainer elementPricelistContainer)
            : base(viewProvider, elementPricelistContainer)
        {
            Throw.IfNull(elementPricelistContainer, nameof(elementPricelistContainer));
            _elementPricelistContainer = elementPricelistContainer;

            RefreshChildren();

            OpenChildCommand = new Command<IElementPricelistInfo>(OpenChild);
        }

        private void OpenChild(IElementPricelistInfo elementPricelistInfo)
        {
            CatchException(() =>
            {
                if (elementPricelistInfo == null) return;

                var operationInfo = _elementPricelistContainer.CanGetChild(elementPricelistInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IElementPricelistContainer.CanGetChild))) return;

                var elementPricelist = _elementPricelistContainer.GetChild(elementPricelistInfo);
                var elementPricelistViewModel = ViewProvider.ViewModelFactory.GetElementPricelistViewModel(elementPricelist);
                Show(elementPricelistViewModel);
            });
        }

        private void RefreshChildren(bool hardRefresh = false)
        {
            if (hardRefresh)
                _elementPricelistContainer.RefreshChildren();

            ElementPricelists.ItemsSource =
                new ObservableCollection<IElementPricelistInfo>(_elementPricelistContainer.ChildrenInfos);
        }
    }
}
