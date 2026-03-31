using Microsoft.Win32;
using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Events;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Models.Interfaces;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Client.Ui;
using Ofcas.Lk.Api.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ofcas.Lk.Api.Shared.Utils;
using Enum = Ofcas.Lk.Api.Client.Demo.Utils.Enum;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class ElevationViewModel : CoreObjectViewModel, IExtendedDisposable, IApplicationView
    {
        private SynchronizationContainerViewModel _synchronizationContainerViewModel;
        private ElevationModel _elevationModel;
        private bool _useUiMode = true;

        private ICoreObjectResult<IElevationUi> _elevationUiResult;
        protected ICoreObjectResult<IElevationUi> ElevationUiResult
        {
            get { return _elevationUiResult; }
            set { _elevationUiResult = value; }
        }
        protected IElevationUi ElevationUi => ElevationUiResult.CoreObject;

        public bool UseUiMode
        {
            get => _useUiMode;
            set { _useUiMode = value; OnPropertyChanged(); }
        }

        private bool _coreObjectDisposed;
        public bool CoreObjectDisposed
        {
            get { return _coreObjectDisposed; }
            set { _coreObjectDisposed = value; OnPropertyChanged(); }
        }

        public ElevationModel ElevationModel
        {
            get { return _elevationModel; }
            set { _elevationModel = value; OnPropertyChanged(); }
        }

        public SelectionAdapter<IElevationInstanceInfo> ElevationInstances { get; } =
            new SelectionAdapter<IElevationInstanceInfo>();

        public ICommand EditCommand { get; }
        public ICommand ShowAsyncCommand { get; }
        public ICommand ShowAsyncWithStreamCommand { get; }
        public ICommand ShowAsyncWithCategoriesCommand { get; }
        public ICommand ExportGdzCommand { get; }
        public ICommand ExportPartsListCommand { get; }
        public ICommand ExportExplosionDrawingCommand { get; }
        public ICommand ExportElevationDrawingCommand { get; }
        public ICommand ExportSectionDrawingCommand { get; }
        public ICommand ExportSectionLineDrawingCommand { get; }
        public ICommand ExportCrossSectionDrawingsCommand { get; }
        public ICommand ExportElevationWithSectionLinesDrawingCommand { get; }
        public ICommand ExportThumbnailCommand { get; }
        public ICommand Export3DCommand { get; }
        public ICommand OpenChildCommand { get; }
        public ICommand CreateChildCommand { get; }
        public ICommand CreateChildrenCommand { get; }
        public ICommand DeleteChildCommand { get; }
        public ICommand CopyElevationInstanceCommand { get; }
        public ICommand OpenDocumentsCommand { get; }
        public ICommand OpenSynchronizationContainerCommand { get; }
        public ICommand ForceRefreshCommand { get; }
        public ICommand OpenInNewWindowCommand { get; }
        public ICommand ShowCalculationPriceCommand { get; }
        public ICommand ShowQuotationPriceCommand { get; }
        public ICommand GetInformationCommand { get; }
        public ICommand InsertMachiningsCommand { get; }
        public ICommand ShowElevationHistoryCommand { get; }
        public ICommand ShowQuotationTextsCommand { get; }
        public ICommand ShowElevationWarningsCommand { get; }
        public ICommand ShowCommentsCommand { get; }

        public event EventHandler ElevationRefreshed;

        public ElevationViewModel(IViewProvider viewProvider, ICoreObjectResult<IElevationUi> elevationUi)
            : base(viewProvider, elevationUi)
        {
            Throw.IfNull(elevationUi, nameof(elevationUi));
            ElevationUiResult = elevationUi;

            ElevationUi.Disposed += OnElevationUiDisposed;

            Refresh(false, true);
            RefreshChildren();

            EditCommand = new Command<string>(Edit);
            ShowAsyncCommand = new AsyncCommand<bool>(ShowAsync);
            ShowAsyncWithStreamCommand = new AsyncCommand(ShowAsyncWithStream);
            ExportGdzCommand = new Command(ExportGdz);
            ExportPartsListCommand = new Command(ExportPartsList);
            ExportExplosionDrawingCommand = new Command(ExportExplosionDrawing);
            ExportElevationDrawingCommand = new Command(ExportElevationDrawing);
            ExportSectionDrawingCommand = new Command(ExportSectionDrawing);
            ExportElevationWithSectionLinesDrawingCommand = new Command(ExportElevationWithSectionLinesDrawing);
            ExportSectionLineDrawingCommand = new Command(ExportSectionLineDrawing);
            ExportCrossSectionDrawingsCommand = new Command(ExportCrossSectionDrawings);
            ExportThumbnailCommand = new Command(ExportThumbnail);
            Export3DCommand = new Command(Export3D);
            OpenChildCommand = new Command<IElevationInstanceInfo>(OpenChild, CanOpenChild);
            CreateChildCommand = new Command(CreateChild);
            CreateChildrenCommand = new Command(CreateChildren);
            DeleteChildCommand = new Command<IElevationInstanceInfo>(DeleteChild, CanDeleteChild);
            CopyElevationInstanceCommand = new Command<ICoreInfo>(CopyElevationInstance, CanCopyElevationInstance);
            OpenDocumentsCommand = new Command(OpenDocuments);
            OpenSynchronizationContainerCommand = new Command(OpenSynchronizationContainer);
            ForceRefreshCommand = new Command(ForceRefresh);
            OpenInNewWindowCommand = new Command(OpenInNewWindow);
            InsertMachiningsCommand = new Command(ImportMachinings);
            ShowCalculationPriceCommand = new Command(ShowCalculationPrice);
            ShowQuotationPriceCommand = new Command(ShowQuotationPrice);
            GetInformationCommand = new Command(GetInformation);
            ShowElevationHistoryCommand = new Command(ShowElevationHistory);
            ShowQuotationTextsCommand = new Command(ShowQuotationTexts);
            ShowElevationWarningsCommand = new Command(ShowElevationWarnings);
            ShowCommentsCommand = new Command(ShowComments);
        }

        private void OnElevationUiDisposed(INotifyingDisposable obj)
        {
            CoreObjectDisposed = true;
        }

        private void Edit(string editKey)
        {
            CatchException(() =>
            {
                var operationInfo = default(IOperationInfo);
                var title = $"Edit {editKey}";
                object value;
                switch (editKey)
                {
                    case WellKnownEditKey.Elevation.Name:
                        var nameInputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel(title, ElevationUi.Info.Name);
                        if (!ShowDialog(nameInputBoxViewModel)) return;

                        value = nameInputBoxViewModel.GetValue();
                        break;

                    case WellKnownEditKey.Elevation.UserDescription:
                        var userDescriptionInputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel(title, ElevationUi.Info.UserDescription);
                        if (!ShowDialog(userDescriptionInputBoxViewModel)) return;

                        value = userDescriptionInputBoxViewModel.GetValue();
                        break;

                    case WellKnownEditKey.Elevation.ModelDescription:
                        var modelDescriptionInputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel(title, ElevationUi.Info.ModelDescription);
                        if (!ShowDialog(modelDescriptionInputBoxViewModel)) return;

                        value = modelDescriptionInputBoxViewModel.GetValue();
                        break;

                    case WellKnownEditKey.Elevation.Amount:
                        var doubleInputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel(title, ElevationUi.Info.Amount);
                        if (!ShowDialog(doubleInputBoxViewModel)) return;

                        value = doubleInputBoxViewModel.GetValue();
                        break;

                    case WellKnownEditKey.Elevation.ProcessingStatus:
                        operationInfo = ElevationUi.Parent.Parent.Parent.LoginScope.CanGetElevationProcessingStatuses();
                        if (!operationInfo.CheckForAnyRestriction(nameof(ILoginScope.CanGetElevationProcessingStatuses))) return;

                        var processingStatuses = ElevationUi.Parent.Parent.Parent.LoginScope.GetElevationProcessingStatuses().ElevationProcessingStatuses;
                        var processingStatusParameter = new Parameter<IElevationProcessingStatus>
                        {
                            IsRequired = true,
                            Key = editKey,
                            Value = processingStatuses.FirstOrDefault(x => x.Id == ElevationUi.Info.ProcessingStatus.Id),
                            RestrictedValues = processingStatuses.ToList()
                        };

                        var processingStatusesParametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(
                            new ObservableCollection<IParameter>
                            {
                                processingStatusParameter
                            }, false);

                        if (!ShowDialog(processingStatusesParametersViewModel)) return;

                        value = processingStatusParameter.Value;
                        break;

                    case WellKnownEditKey.Elevation.Alternative:
                        value = !ElevationModel.IsAlternative;
                        break;

                    case WellKnownEditKey.Elevation.ElementType:
                        var elementTypes = ElevationUi.Parent.Parent.Parent.LoginScope.GetElementTypes().ElementTypes;
                        var elementTypeParameter = new Parameter<IElementType>
                        {
                            IsRequired = true,
                            Key = editKey,
                            Value = elementTypes.FirstOrDefault(x => x.Id == ElevationUi.Info.ElementType.Id),
                            RestrictedValues = elementTypes.ToList()
                        };

                        var elementTypesParametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(
                            new ObservableCollection<IParameter>
                            {
                                elementTypeParameter
                            }, false);

                        if (!ShowDialog(elementTypesParametersViewModel)) return;

                        value = elementTypeParameter.Value.Id;
                        break;

                    case WellKnownEditKey.Elevation.ElementPricelistGuid:
                        var elementPricelistElevationInfo = ElevationUi.Info.AsElementPricelistElevation();
                        var elementPricelistGuidInputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel(title,
                            elementPricelistElevationInfo.ElementPricelistGuid);
                        if (!ShowDialog(elementPricelistGuidInputBoxViewModel)) return;

                        value = elementPricelistGuidInputBoxViewModel.GetValue();
                        break;

                    default:
                        throw new ArgumentException($"The edit key '{editKey}' is not valid.");
                }

                if (UseUiMode)
                    operationInfo = ElevationUi.CanEdit(editKey, value);
                else
                    operationInfo = ((IElevation)ElevationUi).CanEdit(editKey, value);
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevation.CanEdit))) return;

                if (UseUiMode)
                    ElevationUi.Edit(editKey, value);
                else
                    ((IElevation)ElevationUi).Edit(editKey, value);

                RefreshChildren();
                Refresh(true);
            });
        }

        private async Task ShowAsync(bool useDefault)
        {
            await CatchException(async () =>
            {
                ISynchronizedOperation synchronizedOperation;
                if (!useDefault)
                {
                    var editMode = GetEditMode();
                    if (editMode == null) return;

                    var operationInfo = ElevationUi.CanShow(editMode);
                    if (!operationInfo.CheckForAnyRestriction(nameof(IElevationUi.CanShow))) return;

                    synchronizedOperation = ElevationUi.BeginShow(editMode).SynchronizedOperation;
                }
                else
                {
                    var operationInfo = ElevationUi.CanShow();
                    if (!operationInfo.CheckForAnyRestriction(nameof(IElevationUi.CanShow))) return;

                    synchronizedOperation = ElevationUi.BeginShow().SynchronizedOperation;
                }

                ICoreInfoResult<IElevationInfo> coreInfoResult = null;
                await Task.Run(() => { coreInfoResult = ElevationUi.EndShow(synchronizedOperation); })
                    .ConfigureAwait(true);

                AfterShow(coreInfoResult);
            }).ConfigureAwait(true);
        }

        private async Task ShowAsyncWithStream()
        {
            _ = await CatchException(async () =>
            {
                Stream content = Stream.Null;

                var openFileDialog = ViewProvider.ViewModelFactory.GetOpenFileDialogViewModel();
                if (!ShowDialog(openFileDialog))
                    return;
                content = File.OpenRead(openFileDialog.FileName);
                ISynchronizedOperation synchronizedOperation;

                using (content)
                {
                    IOperationInfo operationInfo = ElevationUi.CanShow(content);
                    operationInfo.CheckForAnyRestriction(nameof(IElevationUi.CanShow));
                    content.Position = 0;

                    synchronizedOperation = ElevationUi.BeginShow(content).SynchronizedOperation;
                }

                ICoreInfoResult<IElevationInfo> coreInfoResult = null;
                await Task.Run(() => { coreInfoResult = ElevationUi.EndShow(synchronizedOperation); })
                    .ConfigureAwait(true);

                AfterShow(coreInfoResult);
            }).ConfigureAwait(true);
        }

        private IElevationEditMode GetEditMode()
        {
            var editModeModels = new List<ElevationEditModeModel>();

            foreach (var editMode in ElevationUi.GetEditModes().ElevationEditModes)
                editModeModels.Add(new ElevationEditModeModel(editMode));

            var selectElevationEditModeViewModel = ViewProvider.ViewModelFactory.GetSelectElevationEditModeViewModel(editModeModels);
            if (ViewProvider.ShowDialog(selectElevationEditModeViewModel, this).GetValueOrDefault(false))
                return selectElevationEditModeViewModel.ElevationEditMode.Value.EditMode;
            return null;
        }

        private void AfterShow(IResult result)
        {
            if (result.OperationCode == OperationCode.Rejected)
            {
                ShowMessage("Dialog rejected.");
                return;
            }

            Refresh();
            RefreshChildren(true);
        }

        private void ExportGdz()
        {
            CatchException(() =>
            {
                var parameterViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(
                    new ObservableCollection<IParameter>
                    {
                        new Parameter<string>
                        {
                            Key = WellKnownParameterKey.Elevation.Gdz.Format,
                            RestrictedValues = new List<string> { "WAVEFRONT_OBJ", "XML", "XML_WAVEFRONT" }
                        },
                    });

                if (!ViewProvider.ShowDialog(parameterViewModel, this).GetValueOrDefault())
                    return;

                Dictionary<string, object> parameters = parameterViewModel.GetParameters();
                if (!ElevationUi.CanGetGdz(parameters).CheckForAnyRestriction(nameof(IElevation.CanGetGdz)))
                    return;

                using (var gdzStream = ElevationUi.GetGdz(parameters).Stream)
                {
                    var saveFileDialogViewModel = ViewProvider.ViewModelFactory.GetSaveFileDialogViewModel("ZIP Files | *.zip", "zip");
                    if (!ShowDialog(saveFileDialogViewModel)) return;

                    using (var zipFileStream = new FileStream(saveFileDialogViewModel.FileName, FileMode.Create))
                    {
                        gdzStream.CopyTo(zipFileStream);
                    }
                }
            });

            Refresh(true);
        }

        private void ExportPartsList()
        {
            CatchException(() =>
            {
                var saveFileDialogViewModel = ViewProvider.ViewModelFactory.GetSaveFileDialogViewModel("SQLite3 Files | *.db; *.sqlite3", "db");
                var dialogResult = ViewProvider.ShowDialog(saveFileDialogViewModel, this);
                if (!dialogResult.GetValueOrDefault(false)) return;

                if (!ElevationUi.CanGetPartsList().CheckForAnyRestriction(nameof(IElevation.CanGetPartsList)))
                    return;

                using (var pieceListStream = ElevationUi.GetPartsList().Stream)
                using (var fileStream = File.Create(saveFileDialogViewModel.FileName))
                    pieceListStream.CopyTo(fileStream);
            });
        }

        private void ExportExplosionDrawing()
        {
            CatchException(() =>
            {
                var parameterKeys = new ObservableCollection<IParameter>
                {
                    new Parameter<ElevationDrawingFormat>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.Format,
                        Value = ElevationDrawingFormat.DXF
                    },
                    new Parameter<View>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.View,
                        Value = View.Interior
                    },
                    new Parameter<ElevationDrawingType>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.Type,
                        Value = ElevationDrawingType.Explosion,
                        RestrictedValues = new List<ElevationDrawingType> { ElevationDrawingType.Explosion }
                    },
                    new Parameter<DxfVersion>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.DxfVersion,
                        Value = DxfVersion.R12,
                    }
                };

                ExportDrawing(parameterKeys);
            });
        }

        private void ExportElevationDrawing()
        {
            CatchException(() =>
            {
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

                ExportDrawing(parameterKeys);
            });
        }

        private void ExportSectionDrawing()
        {
            CatchException(() =>
            {
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
                        Value = ElevationDrawingType.Section,
                        RestrictedValues = new List<ElevationDrawingType> { ElevationDrawingType.Section }
                    },
                    new Parameter<DxfVersion>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.DxfVersion,
                        Value = DxfVersion.R12,
                    },
                    new Parameter<double>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.StartX,
                        Value = 0,
                    },
                    new Parameter<double>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.StartY,
                        Value = 0,
                    },
                    new Parameter<double>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.EndX,
                        Value = 0,
                    },
                    new Parameter<double>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.EndY,
                        Value = 0,
                    },
                    new Parameter<bool>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.WithActualGlassSizeDisplay,
                        Value = false,
                    },
                };

                ExportDrawing(parameterKeys);
            });
        }

        private void ExportSectionLineDrawing()
        {
            CatchException(() =>
            {
                var parameterKeys = new ObservableCollection<IParameter>
                {
                    new Parameter<ElevationDrawingFormat>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.Format,
                        Value = ElevationDrawingFormat.DXF
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
                        Value = ElevationDrawingType.SectionLine,
                        RestrictedValues = new List<ElevationDrawingType> { ElevationDrawingType.SectionLine }
                    },
                    new Parameter<DxfVersion>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.DxfVersion,
                        Value = DxfVersion.R12,
                    },
                    new Parameter<double>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.StartX,
                        Value = 0,
                    },
                    new Parameter<double>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.StartY,
                        Value = 0,
                    },
                    new Parameter<double>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.EndX,
                        Value = 0,
                    },
                    new Parameter<double>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.EndY,
                        Value = 0,
                    },
                    new Parameter<bool>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.WithActualGlassSizeDisplay,
                        Value = false,
                    }
                };

                ExportDrawing(parameterKeys);
            });
        }

        private async void ExportCrossSectionDrawings()
        {
            await CatchException(async () =>
            {
                var operationInfo = ElevationUi.CanShowCrossSection();
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevationUi.CanShowCrossSection))) return;

                var asyncOperationResult = ElevationUi.BeginShowCrossSection();
                if (asyncOperationResult.OperationCode != OperationCode.Accepted)
                    return;

                ICrossSectionResult crossSectionResult = null;
                await Task.Run(() => { crossSectionResult = ElevationUi.EndShowCrossSection(asyncOperationResult.SynchronizedOperation); })
                    .ConfigureAwait(true);

                if (crossSectionResult == null)
                    return;

                bool withBoundingBoxOffset = false;

                // exporting all cross sections
                foreach (var crossSectionParameters in crossSectionResult.Sections)
                {
                    var parameterKeys = new ObservableCollection<IParameter>
                    {
                        new Parameter<ElevationDrawingType>
                        {
                            Key = WellKnownParameterKey.Elevation.Drawing.Type,
                            Value = ElevationDrawingType.Section
                        },
                        new Parameter<ElevationDrawingFormat>
                        {
                            Key = WellKnownParameterKey.Elevation.Drawing.Format,
                            Value = ElevationDrawingFormat.DXF,
                        },
                        new Parameter<DxfVersion>
                        {
                            Key = WellKnownParameterKey.Elevation.Drawing.DxfVersion,
                            Value = DxfVersion.R12,
                        },
                        new Parameter<bool>
                        {
                            Key = WellKnownParameterKey.Elevation.Drawing.WithBoundingBoxOffset,
                            Value = withBoundingBoxOffset,
                        },
                    };

                    foreach (var item in crossSectionParameters)
                    {
                        parameterKeys.Add(new Parameter<object> { Key = item.Key, Value = item.Value });
                    }

                    foreach (var item in crossSectionResult.Parameters)
                    {
                        parameterKeys.Add(new Parameter<object> { Key = item.Key, Value = item.Value });
                    }

                    var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(parameterKeys);
                    if (!ViewProvider.ShowDialog(parametersViewModel, this).GetValueOrDefault())
                        return;

                    Dictionary<string, object> parameters = parametersViewModel.GetParameters();

                    // remember last selected withBoundingBoxOffset option
                    parameters.TryGetValue(WellKnownParameterKey.Elevation.Drawing.WithBoundingBoxOffset,
                        out withBoundingBoxOffset);

                    operationInfo = ElevationUi.CanGetDrawing(parameters);
                    if (!operationInfo.CheckForAnyRestriction(nameof(IElevation.CanGetDrawing))) return;

                    var drawingResult = ElevationUi.GetDrawing(parameters);
                    Stream drawingStream = drawingResult.Stream;
                    var values = drawingResult.BoundingBox;

                    if (values.Count > 0)
                    {
                        var returnValues = new ObservableCollection<IParameter>();
                        foreach (var value in values)
                        {
                            var returnValue = new Parameter<string>
                            {
                                Key = value.Key,
                                Value = value.Value.ToString(),
                                IsRequired = true
                            };
                            returnValues.Add(returnValue);
                        }

                        ViewProvider.ShowDialog(new ParametersViewModel(ViewProvider, returnValues, false), this);
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
                        if (!saveFileDialogResult.GetValueOrDefault(false)) return;

                        using (var fileStream = File.OpenWrite(saveFileDialogViewModel.FileName))
                            drawingStream.CopyTo(fileStream);
                    }
                }
            }).ConfigureAwait(true);
        }

        private void ExportElevationWithSectionLinesDrawing()
        {
            CatchException(() =>
            {
                var parameterKeys = new ObservableCollection<IParameter>
                {
                    new Parameter<ElevationDrawingFormat>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.Format,
                        Value = ElevationDrawingFormat.DXF
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
                        Value = ElevationDrawingType.ElevationWithSectionLines,
                        RestrictedValues = new List<ElevationDrawingType> { ElevationDrawingType.ElevationWithSectionLines }
                    },
                    new Parameter<DxfVersion>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.DxfVersion,
                        Value = DxfVersion.R12
                    },
                    new Parameter<bool>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.WithActualGlassSizeDisplay,
                        Value = false
                    },
                    new Parameter<bool>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.WithElevation,
                        Value = false
                    },
                    new Parameter<bool>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.WithVerticalCuts,
                        Value = false
                    },
                    new Parameter<bool>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.WithHorizontalCuts,
                        Value = false
                    },
                    new Parameter<IList<IDictionary<string, object>>>
                    {
                        Key = WellKnownParameterKey.Elevation.Drawing.SectionLines,
                        Value = null,
                        InputHelper = () =>
                        {
                            var asyncOperationResult = ElevationUi.BeginShowCrossSection();
                            ICrossSectionResult crossSectionResult = null;

                            Task.Run(() => { crossSectionResult = ElevationUi.EndShowCrossSection(asyncOperationResult.SynchronizedOperation); })
                                .ConfigureAwait(true);
                            
                            ShowMessage(StringUtils.ToDetailedString(crossSectionResult.Sections));
                            return crossSectionResult.Sections;
                        }
                    }
                };

                ExportDrawing(parameterKeys);
            });
        }

        private void ExportDrawing(ObservableCollection<IParameter> parameterKeys)
        {
            CatchException(() =>
            {
                var parametersViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(parameterKeys);
                if (!ViewProvider.ShowDialog(parametersViewModel, this).GetValueOrDefault())
                    return;

                Dictionary<string, object> parameters = parametersViewModel.GetParameters();

                var operationInfo = ElevationUi.CanGetDrawing(parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevation.CanGetDrawing))) return;

                using (var drawingStream = ElevationUi.GetDrawing(parameters).Stream)
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
                    if (!saveFileDialogResult.GetValueOrDefault(false)) return;

                    using (var fileStream = File.OpenWrite(saveFileDialogViewModel.FileName))
                        drawingStream.CopyTo(fileStream);
                }
            });
        }

        private void ExportThumbnail()
        {
            CatchException(() =>
            {
                var saveFileDialogViewModel = ViewProvider.ViewModelFactory.GetSaveFileDialogViewModel(
                    "Portable Graphic |*.png|Metafile Graphic |*.emf|JPEG Graphic |*.jpg", "png");

                var dialogResult = ViewProvider.ShowDialog(saveFileDialogViewModel, this);

                if (!dialogResult.GetValueOrDefault(false)) return;

                var graphicFormat = saveFileDialogViewModel.GetExtension(false);

                ObservableCollection<IParameter> parameters = new ObservableCollection<IParameter>
                {
                    new Parameter<string>
                    {
                        Key = WellKnownParameterKey.Elevation.Thumbnail.Format,
                        Value = graphicFormat,
                        RestrictedValues = new List<string> { "png", "emf", "jpg" },
                    },
                    new Parameter<View>
                    {
                        Key = WellKnownParameterKey.Elevation.Thumbnail.View,
                        Value = View.Interior,
                        RestrictedValues = Enum.GetValues<View>().ToList(),
                    },
                    new Parameter<double>
                    {
                        Key = WellKnownParameterKey.Elevation.Thumbnail.Width,
                        Value = 300,
                    },
                    new Parameter<double>
                    {
                        Key = WellKnownParameterKey.Elevation.Thumbnail.Height,
                        Value = 300,
                    },
                    new Parameter<bool>
                    {
                        Key = WellKnownParameterKey.Elevation.Thumbnail.WithDimensions,
                        Value = true,
                        RestrictedValues = new List<bool> { true, false },
                    },
                    new Parameter<bool>
                    {
                        Key = WellKnownParameterKey.Elevation.Thumbnail.WithDescription,
                        Value = true,
                        RestrictedValues = new List<bool> { true, false },
                    },
                };

                var parameterViewModel = new ParametersViewModel(ViewProvider, parameters);
                if (!ViewProvider.ShowDialog(parameterViewModel, this).GetValueOrDefault())
                    return;

                IOperationInfo operationInfo = ElevationUi.CanGetThumbnail(parameterViewModel.GetParameters());
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevation.CanGetThumbnail))) return;

                using (var streamThumbnail = ElevationUi.GetThumbnail(parameterViewModel.GetParameters()).Stream)
                using (var fileStream = File.OpenWrite(saveFileDialogViewModel.FileName))
                    streamThumbnail.CopyTo(fileStream);
            });
        }

        private void Export3D()
        {
            CatchException(() =>
            {
                var saveFileDialogViewModel = ViewProvider.ViewModelFactory.GetSaveFileDialogViewModel("XML Files | *.xml", "xml");
                var dialogResult = ViewProvider.ShowDialog(saveFileDialogViewModel, this);
                if (!dialogResult.GetValueOrDefault(false)) return;

                var parameters = new Dictionary<string, object>();
                IOperationInfo operationInfo = ElevationUi.CanGet3D(parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevation.CanGet3D))) return;

                using (var stream3DContent = ElevationUi.Get3D(parameters).Stream)
                using (var fileStream = File.OpenWrite(saveFileDialogViewModel.FileName))
                    stream3DContent.CopyTo(fileStream);
            });
        }

        private bool CanOpenChild(IElevationInstanceInfo elevationInstanceInfo)
        {
            return elevationInstanceInfo != null;
        }

        private void OpenChild(IElevationInstanceInfo elevationInstanceInfo)
        {
            CatchException(() =>
            {
                var operationInfo = ElevationUi.CanGetChild(elevationInstanceInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevation.CanGetChild))) return;

                var elevationInstance = ElevationUi.GetChild(elevationInstanceInfo);
                var elevationInstanceViewModel = ViewProvider.ViewModelFactory.GetElevationInstanceViewModel(elevationInstance);
                elevationInstanceViewModel.ElevationInstanceRefreshed += OnElevationInstanceRefreshed;
                ViewProvider.Show(elevationInstanceViewModel);
            });
        }

        private void CreateChild()
        {
            CatchException(() =>
            {
                IOperationInfo operationInfo;
                if (_useUiMode)
                    operationInfo = ElevationUi.CanCreateChild();
                else
                    operationInfo = ((IElevation)ElevationUi).CanCreateChild();
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevation.CanCreateChild))) return;

                if (_useUiMode)
                    ElevationUi.CreateChild();
                else
                    ((IElevation)ElevationUi).CreateChild();

                RefreshChildren();
                Refresh(true);
            });
        }

        public void CreateChildren()
        {
            CatchException(() =>
            {
                IOperationInfo operationInfo;
                if (_useUiMode)
                    operationInfo = ElevationUi.CanCreateChild();
                else
                    operationInfo = ((IElevation)ElevationUi).CanCreateChild();
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevation.CanCreateChild))) return;

                var inputbox = ViewProvider.ViewModelFactory.GetInputBoxViewModel("Enter instance Amount", 1);
                var result = ViewProvider.ShowDialog(inputbox, this);
                if (!result.GetValueOrDefault(false)) return;

                if (_useUiMode)
                    ElevationUi.CreateChildren(inputbox.GetValue());
                else
                    ((IElevation)ElevationUi).CreateChildren(inputbox.GetValue());

                RefreshChildren();
                Refresh(true);
            });
        }

        private bool CanDeleteChild(IElevationInstanceInfo elevationInstanceInfo)
        {
            return elevationInstanceInfo != null;
        }

        private void DeleteChild(IElevationInstanceInfo elevationInstanceInfo)
        {
            CatchException(() =>
            {
                IOperationInfo operationInfo;
                if (_useUiMode)
                    operationInfo = ElevationUi.CanDeleteChild(elevationInstanceInfo);
                else
                    operationInfo = ((IElevation)ElevationUi).CanDeleteChild(elevationInstanceInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevation.CanDeleteChild))) return;


                if (_useUiMode)
                    ElevationUi.DeleteChild(elevationInstanceInfo);
                else
                    ((IElevation)ElevationUi).DeleteChild(elevationInstanceInfo);

                RefreshChildren();
                Refresh(true);
            });
        }

        private bool CanCopyElevationInstance(ICoreInfo coreInfo)
        {
            return coreInfo != null;
        }

        public void CopyElevationInstance(ICoreInfo coreInfo)
        {
            CatchException(() =>
            {
                var data = new ClipboardData<ElevationViewModel, ICoreInfo>
                {
                    Sender = this,
                    Content = coreInfo
                };

                ClipboardHelper.Data = data;
            });
        }

        private void OpenDocuments()
        {
            CatchException(() =>
            {
                var documentContainer = ElevationUi.DocumentContainer;
                var viewModel = ViewProvider.ViewModelFactory.GetDocumentContainerViewModel(documentContainer);
                ViewProvider.Show(viewModel);
            });
        }

        private void OpenSynchronizationContainer()
        {
            CatchException(() =>
            {
                if (_synchronizationContainerViewModel != null) return;

                _synchronizationContainerViewModel = ViewProvider.ViewModelFactory.GetSynchronizationContainerViewModel(ElevationUi);
                _synchronizationContainerViewModel.SynchronizedEventReceived += SynchronizationViewModelOnSynchronizedEventReceived;
                ViewProvider.Show(_synchronizationContainerViewModel, delegate { OnSynchronizationContainerClosed(); });
            });
        }

        private void ForceRefresh()
        {
            CatchException(() =>
            {
                Refresh(true);
                RefreshChildren(true);
            });
        }

        public void OpenInNewWindow()
        {
            CatchException(() =>
            {
                var loginScope = ElevationUi.Parent.Parent.Parent.LoginScope;
                var coreObjectFactory = loginScope.GetCoreObjectFactory().CoreObjectFactory;

                var elevation = coreObjectFactory.GetElevation(ElevationUi.Info);
                var elevationViewModel = ViewProvider.ViewModelFactory.GetElevationViewModel((ICoreObjectResult<IElevationUi>)elevation);
                Show(elevationViewModel);
            });
        }

        private ImageSource InitView()
        {
            var parameters = new Dictionary<string, object>
            {
                [WellKnownParameterKey.Elevation.Thumbnail.Format] = "PNG",
                [WellKnownParameterKey.Elevation.Thumbnail.View] = View.Interior,
                [WellKnownParameterKey.Elevation.Thumbnail.Width] = 300,
                [WellKnownParameterKey.Elevation.Thumbnail.Height] = 300,
                [WellKnownParameterKey.Elevation.Thumbnail.WithDimensions] = true,
                [WellKnownParameterKey.Elevation.Thumbnail.WithDescription] = true
            };

            try
            {
                var img = new BitmapImage();
                var imgStream = ElevationUi.GetThumbnail(parameters).Stream;
                if (imgStream.Length == 0)
                    return img;
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = imgStream;
                img.EndInit();

                return img;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return null;
            }
        }

        private void SynchronizationViewModelOnSynchronizedEventReceived(object sender, SynchronizedEventReceivedEventArgs eventArgs)
        {
            CatchException(() =>
            {
                var synchronizedEvent = eventArgs.SynchronizedEvent;
                if (synchronizedEvent.Object == WellKnownSynchronizedEventObject.Elevation)
                    Refresh(true);

                ElevationUi.SynchronizationContainer.SetHandled(synchronizedEvent);
            });
        }

        private void OnSynchronizationContainerClosed()
        {
            _synchronizationContainerViewModel.SynchronizedEventReceived -= SynchronizationViewModelOnSynchronizedEventReceived;
            _synchronizationContainerViewModel = null;
        }

        private void OnElevationInstanceRefreshed(object sender, EventArgs eventArgs)
        {
            RefreshChildren();
        }

        private void Refresh(bool hardRefresh = false, bool suppressEvent = false)
        {
            if (hardRefresh)
                ElevationUi.Refresh();

            var elevationModel = new ElevationModel(this)
            {
                CoreObjectId = ElevationUi.Id,
                Guid = ElevationUi.Info.Guid,
                VersionGuid = ElevationUi.Info.VersionGuid,
                Name = ElevationUi.Info.Name,
                ProcessingStatus = ElevationUi.Info.ProcessingStatus,
                CreatedDate = ElevationUi.Info.CreatedDateTime,
                ChangedDate = ElevationUi.Info.LastChangedDateTime,
                Thumbnail = InitView(),
                State = ElevationUi.Info.State,
                ElementType = ElevationUi.Info.ElementType,
                IsAlternative = ElevationUi.Info.IsAlternative,
                UserDescription = ElevationUi.Info.UserDescription,
                Type = ElevationUi.Parent.Parent.Parent.Info.Type.Id,
                Amount = ElevationUi.Info.Amount,
                IsStandardElevation = ElevationUi.Info.IsStandardElevation(),
                IsSubSectionElevation = ElevationUi.Info.IsSubSectionElevation(),
                IsTextElevation = ElevationUi.Info.IsTextElevation(),
                IsElementPricelistElevation = ElevationUi.Info.IsElementPricelistElevation(),
                IsMaterialElevation = ElevationUi.Info.IsMaterialElevation(),
                IsBaseElementElevation = ElevationUi.Info.IsBaseElementElevation(),
                IsSegmentElevation = ElevationUi.Info.IsSegmentElevation(),
                IsInsertionElevation = ElevationUi.Info.IsInsertionElevation(),
                IsLayerElevation = ElevationUi.Info.IsLayerElevation(),
                AutomaticDescription = ElevationUi.Info.AutomaticDescription,
                ModelDescription = ElevationUi.Info.ModelDescription,
                SystemDescription = ElevationUi.Info.SystemDescription,
                CreatedByUser = ElevationUi.Info.CreatedByUser,
                ChangedByUser = ElevationUi.Info.ChangedByUser,
                Comment = ElevationUi.Info.Comment,
                WarningLevel = ElevationUi.Info.WarningLevel,
                ElevationWarningsCount = ElevationUi.Info.ElevationWarnings.Count,
                IsIncludedInEPD = ElevationUi.Info.IsIncludedInEPD,
            };

            if (ElevationUi.Info.IsStandardElevation())
            {
                var standardElevationInfo = ElevationUi.Info.AsStandardElevation();
                elevationModel.Width = standardElevationInfo.Width;
                elevationModel.Height = standardElevationInfo.Height;
                elevationModel.UValue = standardElevationInfo.UValue;

                var surfaceBase = standardElevationInfo.SurfaceBase;
                elevationModel.SurfaceBaseName = surfaceBase.Name;
                elevationModel.SurfaceBaseColors = $"{surfaceBase.ColorInside} / {surfaceBase.ColorOutside}";
                var surfaceFrame = standardElevationInfo.SurfaceFrame;
                elevationModel.SurfaceFrameName = surfaceFrame.Name;
                elevationModel.SurfaceFrameColors = $"{surfaceFrame.ColorInside} / {surfaceFrame.ColorOutside}";
            }

            if (elevationModel.IsTextElevation)
            {
                var textElevationInfo = ElevationUi.Info.AsTextElevation();
                elevationModel.Unit = textElevationInfo.Unit;
                elevationModel.RtfContent = textElevationInfo.RtfContent;
            }

            if (elevationModel.IsSubSectionElevation)
            {
                var subSectionElevationInfo = ElevationUi.Info.AsSubSectionElevation();
                elevationModel.MainGuid = subSectionElevationInfo.MainElevationGuid;
            }

            if (elevationModel.IsElementPricelistElevation)
            {
                var elementPricelistElevation = ElevationUi.Info.AsElementPricelistElevation();
                elevationModel.ElementPricelistGuid = elementPricelistElevation.ElementPricelistGuid;
            }

            if (elevationModel.IsMaterialElevation)
            {
                var materialElevationInfo = ElevationUi.Info.AsMaterialElevation();
                elevationModel.RtfContent = materialElevationInfo.RtfContent;
            }

            ElevationModel = elevationModel;

            if (!suppressEvent)
                ElevationRefreshed?.Invoke(this, EventArgs.Empty);
        }

        private void RefreshChildren(bool hardRefresh = false)
        {
            if (hardRefresh)
                ElevationUi.RefreshChildren();

            ElevationInstances.Load(ElevationUi.ChildrenInfos);
        }

        public void OnLoaded()
        {
            ElevationUi.SetApplicationHandle(ViewProvider.GetHandle(this));
        }

        public bool CanDispose(out string falseReason)
        {
            if (_synchronizationContainerViewModel == null)
                return CanDispose(_elevationUiResult, out falseReason);

            falseReason = $"Synchronization [{ElevationUi.Id}]";
            return false;
        }

        public void Dispose()
        {
            if (_elevationUiResult == null) return;

            _elevationUiResult.Dispose();
            _elevationUiResult = null;
        }

        private void ImportMachinings()
        {
            CatchException(() =>
            {
                var operationInfo = ElevationUi.CanSetMachinings();
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevation.CanSetMachinings))) return;

                var fileDialog = new OpenFileDialog();
                var result = fileDialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var fileName = fileDialog.FileName;
                    using (var fileStream = File.OpenRead(fileName))
                    {
                        var elevationInfo = ElevationUi.SetMachinings(fileStream);
                    }
                }
            });
        }

        public void ShowElevationHistory()
        {
            CatchException(() =>
            {
                var operationInfo = ElevationUi.CanGetInfoHistory();
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevation.CanGetInfoHistory))) return;

                var viewModel = ViewProvider.ViewModelFactory.GetElevationHistoryViewModel(ElevationUi);
                Show(viewModel);
            });
        }

        private void ShowCalculationPrice()
        {
            CatchException(() =>
            {
                var operationInfo = ElevationUi.CanGetCalculationPrice();
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevation.CanGetCalculationPrice))) return;

                var calculationPrice = ElevationUi.GetCalculationPrice().Value;

                MessageBox.Show(calculationPrice);
            });
        }

        private void ShowQuotationPrice()
        {
            CatchException(() =>
            {
                var operationInfo = ElevationUi.CanGetQuotationPrice();
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevation.CanGetQuotationPrice))) return;

                var quotationPrice = ElevationUi.GetQuotationPrice().Value;

                MessageBox.Show(quotationPrice);
            });
        }

        private void GetInformation()
        {
            CatchException(() =>
            {
                var availableParameters = new ObservableCollection<IParameter>
                {
                    new Parameter<string>
                    {
                        Key = WellKnownParameterKey.Elevation.Information.Format,
                        RestrictedValues = new List<string> {"XML"},
                        Value = "XML",
                    },
                    new Parameter<bool>
                    {
                        Key = WellKnownParameterKey.Elevation.Information.UseMetric
                    },
                    new Parameter<LevelOfDetail>
                    {
                        Key = WellKnownParameterKey.Elevation.Information.LevelOfDetail,
                        Value = LevelOfDetail.Overview
                    }
                };

                var parameterViewModel = ViewProvider.ViewModelFactory.GetParametersViewModel(availableParameters);
                if (!ViewProvider.ShowDialog(parameterViewModel, this).GetValueOrDefault())
                    return;

                Dictionary<string, object> parameters = parameterViewModel.GetParameters();
                var operationInfo = ElevationUi.CanGetInformation(parameters);
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevation.CanGetInformation))) return;

                using (Stream stream = ElevationUi.GetInformation(parameters).Stream)
                {
                    var saveDialogViewModel = ViewProvider.ViewModelFactory.GetSaveFileDialogViewModel("All Files (*.*)|*.*");
                    if (!ViewProvider.ShowDialog(saveDialogViewModel, this).GetValueOrDefault())
                        return;

                    using (var fileStream = new FileStream(saveDialogViewModel.FileName, FileMode.Create))
                    {
                        stream.CopyTo(fileStream);
                        fileStream.Flush(true);
                    }
                }
            });
        }

        private void ShowQuotationTexts()
        {
            CatchException(() =>
            {
                var operationInfo = ElevationUi.CanGetQuotationTexts();
                if (!operationInfo.CheckForAnyRestriction(nameof(IElevation.CanGetQuotationTexts))) return;

                var quotationTexts = ElevationUi.GetQuotationTexts().QuotationTexts;

                var elevationQuotationTextsViewModel =
                    ViewProvider.ViewModelFactory.GetElevationQuotationTextsViewModel(ElevationModel.Name,
                        quotationTexts);

                ViewProvider.Show(elevationQuotationTextsViewModel);
            });
        }

        private void ShowComments()
        {
            CatchException(() =>
            {
                var comments = ElevationUi.GetComments().ElevationComments;

                var elevationCommentsViewModel =
                    ViewProvider.ViewModelFactory.GetElevationCommentsViewModel(ElevationModel.Name, comments);

                ViewProvider.Show(elevationCommentsViewModel);
            });
        }

        private void ShowElevationWarnings()
        {
            CatchException(() =>
            {
                var elevationWarningsViewModel =
                    ViewProvider.ViewModelFactory.GetElevationWarningsViewModel(ElevationUi);
                ViewProvider.Show(elevationWarningsViewModel);
            });
        }
    }
}
