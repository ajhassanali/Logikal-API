using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public abstract class CoreObjectViewModel : ParentViewModelBase
    {
        public ICoreObjectResult<ICoreObject> CoreObjectResult { get; protected set; }
        public ICoreObject CoreObject;

        public ICommand ShowCoreObjectDetailsCommand { get; }

        public CoreObjectViewModel(IViewProvider viewProvider)
            : base(viewProvider)
        {
            CoreObjectResult = null;
            ShowCoreObjectDetailsCommand = new Command(ShowCoreObjectDetails, CanShowCoreObjectDetails);
        }

        public CoreObjectViewModel(IViewProvider viewProvider, ICoreObjectResult<ICoreObject> coreObject)
            : base(viewProvider)
        {
            CoreObjectResult = coreObject;
            CoreObject = coreObject.CoreObject;
            ShowCoreObjectDetailsCommand = new Command(ShowCoreObjectDetails, CanShowCoreObjectDetails);
        }

        public CoreObjectViewModel(IViewProvider viewProvider, ICoreObject coreObject)
            : base(viewProvider)
        {
            CoreObject = coreObject;
            ShowCoreObjectDetailsCommand = new Command(ShowCoreObjectDetails, CanShowCoreObjectDetails);
        }

        public void ShowCoreObjectDetails()
        {
            var coreObjectDetailsViewModel = ViewProvider.ViewModelFactory.GetCoreObjectDetailsViewModel(CoreObject);
            ViewProvider.Show(coreObjectDetailsViewModel);
        }

        public bool CanShowCoreObjectDetails()
        {
            return CoreObject != null;
        }
    }
}
