using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Core.Utils;
using Ofcas.Lk.Api.Client.Demo.Events;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Models.Interfaces;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ElevationInstanceViewModel : CoreObjectViewModel, IExtendedDisposable
    {
        private SynchronizationContainerViewModel _synchronizationContainerViewModel;
        private ICoreObjectResult<IElevationInstance> _elevationInstanceResult;
        private ICoreObjectResult<IElevationInstance> ElevationInstanceResult
        {
            get { return _elevationInstanceResult; }
            set { _elevationInstanceResult = value; }
        }
        private IElevationInstance ElevationInstance => ElevationInstanceResult.CoreObject;

        private bool _coreObjectDisposed;
        public bool CoreObjectDisposed
        {
            get { return _coreObjectDisposed; }
            set { _coreObjectDisposed = value; OnPropertyChanged(); }
        }

        private ElevationInstanceModel _elevationInstanceModel;
        public ElevationInstanceModel ElevationInstanceModel
        {
            get { return _elevationInstanceModel; }
            set { _elevationInstanceModel = value; OnPropertyChanged(); }
        }

        public ICommand EditCommand { get; }
        public ICommand OpenDocumentsCommand { get; }
        public ICommand OpenInNewWindowCommand { get; }
        public ICommand OpenSynchronizationContainerCommand { get; }
        public ICommand ForceRefreshCommand { get; }
        public ICommand ExportDrawingCommand { get; }

        public event EventHandler ElevationInstanceRefreshed;

        public ElevationInstanceViewModel(IViewProvider viewProvider, ICoreObjectResult<IElevationInstance> elevationInstance)
            : base(viewProvider, elevationInstance)
        {
            Throw.IfNull(elevationInstance, nameof(elevationInstance));
            ElevationInstanceResult = elevationInstance;
            Refresh(false, true);

            ElevationInstance.Disposed += OnElevationInstanceDisposed;

            EditCommand = new Command<string>(Edit);
            OpenDocumentsCommand = new Command(OpenDocuments);
            OpenInNewWindowCommand = new Command(OpenInNewWindow);
            OpenSynchronizationContainerCommand = new Command(OpenSynchronizationContainer);
            ForceRefreshCommand = new Command(ForceRefresh);
            ExportDrawingCommand = new Command(ExportDrawing);
        }

        private void OnElevationInstanceDisposed(INotifyingDisposable obj)
        {
            CoreObjectDisposed = true;
        }

        private void Edit(string editKey)
        {
            CatchException(() =>
            {
                var title = $"Edit {editKey}";
                object value;
                switch (editKey)
                {
                    case WellKnownEditKey.ElevationInstance.Description:
                        var descriptionInputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel(title,
                            ElevationInstance.Info.Description);
                        if (!ShowDialog(descriptionInputBoxViewModel)) return;

                        value = descriptionInputBoxViewModel.GetValue();
                        break;
                    case WellKnownEditKey.ElevationInstance.LocationInObject:
                        var locationInObjectInputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel(title,
                            ElevationInstance.Info.LocationInObject);
                        if (!ShowDialog(locationInObjectInputBoxViewModel)) return;

                        value = locationInObjectInputBoxViewModel.GetValue();
                        break;
                    default:
                        throw new ArgumentException($"The edit key '{editKey}' is not valid.");
                }

                var operationInfo = ElevationInstance.CanEdit(editKey, value);
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevationInstance.CanEdit))) return;

                ElevationInstance.Edit(editKey, value);
                Refresh();
            });
        }

        private void ExportDrawing()
        {
            CatchException(() => {

                var parameterKeys = new ObservableCollection<IParameter>
                {
                    new Parameter<ElevationDrawingFormat>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.Format,
                        Value = ElevationDrawingFormat.DXF,
                    },
                    new Parameter<View>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.View,
                        Value = View.Interior,
                        RestrictedValues = new List<View>() { View.Interior, View.Exterior }
                    },
                    new Parameter<ElevationDrawingType>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.Type,
                        Value = ElevationDrawingType.Elevation,
                        RestrictedValues = new List<ElevationDrawingType>() { ElevationDrawingType.Elevation }
                    },
                    new Parameter<DxfVersion>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.DxfVersion,
                        Value = DxfVersion.R12,
                    },
                    new Parameter<bool>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.ShowDescription,
                        Value = true,
                    },
                    new Parameter<bool>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.ShowDimensions,
                        Value = true,
                    },
                    new Parameter<double>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.Scale,
                        Value = 1.0
                    }
                };

                var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(new ObservableCollection<IParameter>(parameterKeys));
                if (!ViewProvider.ShowDialog(parametersViewModel, this).GetValueOrDefault())
                    return;

                Dictionary<string, object> parameters = parametersViewModel.GetParameters();
                var drawingResult = ElevationInstance.GetDrawing(parameters);
                var drawingStream = drawingResult.Stream;
                var returnValues = drawingResult.BoundingBox;

                if (drawingStream.Length < 1)
                {
                    ShowMessage("Stream returned empty.");
                    return;
                }

                if (returnValues.Count > 0)
                {
                    var returnValuesCollection = new ObservableCollection<IParameter>();
                    foreach (var value in returnValues)
                    {
                        var returnValue = new Parameter<string>
                        {
                            Key = value.Key,
                            Value = value.Value.ToString(),
                            IsRequired = true
                        };
                        returnValuesCollection.Add(returnValue);
                    }

                    ViewProvider.ShowDialog(new ParametersViewModel(ViewProvider, returnValuesCollection, false), this);
                }

                using (drawingStream)
                {
                    var elevationDrawingFormat =
                        (ElevationDrawingFormat)parameters[WellKnownParameterKey.Elevation.Drawing.Format];

                    var saveFileDialogViewModel = ViewProvider.ViewModelFactory.GetSaveFileDialogViewModel();
                    switch (elevationDrawingFormat)
                    {
                        case ElevationDrawingFormat.DXF:
                            saveFileDialogViewModel.Filter = "DXF Files | *.dxf";
                            saveFileDialogViewModel.DefaultExt = "dxf";
                            break;

                        case ElevationDrawingFormat.OCD:
                            saveFileDialogViewModel.Filter = "OCD Files | *.ocd";
                            saveFileDialogViewModel.DefaultExt = "ocd";
                            break;

                        case ElevationDrawingFormat.PNG:
                            saveFileDialogViewModel.Filter = "PNG Files | *.png";
                            saveFileDialogViewModel.DefaultExt = "png";
                            break;
                    }

                    var saveFileDialogResult = ViewProvider.ShowDialog(saveFileDialogViewModel, this);
                    if (!saveFileDialogResult.GetValueOrDefault(false))
                        return;

                    using (var fileStream = File.OpenWrite(saveFileDialogViewModel.FileName))
                        drawingStream.CopyTo(fileStream);
                }
            });
        }

        private void OpenDocuments()
        {
            CatchException(() =>
            {
                var documentContainer = ElevationInstance.DocumentContainer;
                var viewModel = ViewProvider.ViewModelFactory.GetDocumentContainerViewModel(documentContainer);
                ViewProvider.Show(viewModel);
            });
        }

        private void OpenInNewWindow()
        {
            CatchException(() =>
            {
                var loginScope = ElevationInstance.Parent.Parent.Parent.Parent.LoginScope;
                var coreObjectFactory = loginScope.GetCoreObjectFactory().CoreObjectFactory;

                var elevationInstance = coreObjectFactory.GetElevationInstance(ElevationInstance.Info);
                var elevationInstanceViewModel = ViewProvider.ViewModelFactory.GetElevationInstanceViewModel(elevationInstance);
                Show(elevationInstanceViewModel);
            });
        }

        private void OpenSynchronizationContainer()
        {
            CatchException(() =>
            {
                if (_synchronizationContainerViewModel != null) return;

                _synchronizationContainerViewModel = ViewProvider.ViewModelFactory.GetSynchronizationContainerViewModel(ElevationInstance);
                _synchronizationContainerViewModel.SynchronizedEventReceived +=
                    SynchronizationViewModelOnSynchronizedEventReceived;
                Show(_synchronizationContainerViewModel, delegate { OnSynchronizationContainerClosed(); });
            });
        }

        private void SynchronizationViewModelOnSynchronizedEventReceived(object sender,
            SynchronizedEventReceivedEventArgs eventArgs)
        {
            CatchException(() =>
            {
                var synchronizedEvent = eventArgs.SynchronizedEvent;
                switch (synchronizedEvent.Object)
                {
                    case WellKnownSynchronizedEventObject.ElevationInstance:
                        Refresh(true);
                        break;
                }

                ElevationInstance.SynchronizationContainer.SetHandled(synchronizedEvent);
            });
        }

        private void OnSynchronizationContainerClosed()
        {
            _synchronizationContainerViewModel.SynchronizedEventReceived -= SynchronizationViewModelOnSynchronizedEventReceived;
            _synchronizationContainerViewModel = null;
        }

        private void ForceRefresh()
        {
            Refresh(true, true);
        }

        private void Refresh(bool hardRefresh = false, bool suppressEvent = false)
        {
            if (hardRefresh)
                ElevationInstance.Refresh();

            ElevationInstanceModel = new ElevationInstanceModel()
            {
                CoreObjectId = ElevationInstance.Id.ToString("D"),
                Guid = ElevationInstance.Info.Guid.ToString("D"),
                Description = ElevationInstance.Info.Description,
                LocationInObject = ElevationInstance.Info.LocationInObject,
                AssignedFabricationLot = ElevationInstance.Info.AssignedFabricationLot
            };

            if (!suppressEvent)
                ElevationInstanceRefreshed?.Invoke(this, EventArgs.Empty);
        }

        public bool CanDispose(out string falseReason)
        {
            return CanDispose(_elevationInstanceResult, out falseReason);
        }

        public void Dispose()
        {
            ElevationInstance.Disposed -= OnElevationInstanceDisposed;

            if (_elevationInstanceResult == null) return;

            _elevationInstanceResult.Dispose();
            _elevationInstanceResult = null;
        }
    }
}
