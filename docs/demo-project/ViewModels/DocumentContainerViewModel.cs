using Microsoft.Win32;
using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Events;
using Ofcas.Lk.Api.Client.Demo.Exceptions;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class DocumentContainerViewModel : CoreObjectViewModel, IViewModelWithChildren
    {
        private SynchronizationContainerViewModel _synchronizationContainerViewModel;
        protected IDocumentContainer DocumentContainer { get; private set; }

        public SelectionAdapter<IDocumentInfo> Documents { get; } = new SelectionAdapter<IDocumentInfo>();

        public string ParentName { get; }
        public ICommand CreateDocumentCommand { get; }
        public ICommand DeleteDocumentCommand { get; }
        public ICommand CopyDocumentCommand { get; }
        public ICommand PasteDocumentCommand { get; }
        public ICommand MoveDocumentCommand { get; }
        public ICommand OpenSynchronizationContainerCommand { get; }
        public ICommand ForceRefreshCommand { get; }
        public ICommand OpenDocumentCommand { get; set; }
        public ICommand CreateShortcutCommand { get; }

        public DocumentContainerViewModel(IDocumentContainer documentContainer, IViewProvider viewProvider)
            : base(viewProvider)
        {
            DocumentContainer = documentContainer ?? throw new ArgumentNullException(nameof(documentContainer));

            ParentName = documentContainer.Parent.GetType().Name;

            CreateDocumentCommand = new Command(CreateDocument);
            DeleteDocumentCommand = new Command<IDocumentInfo>(DeleteDocument);
            CopyDocumentCommand = new Command<IDocumentInfo>(CopyDocument);
            PasteDocumentCommand = new Command(PasteDocument, () => ClipboardHelper.Data != null);
            MoveDocumentCommand = new Command(MoveDocument, () => ClipboardHelper.Data != null);
            OpenSynchronizationContainerCommand = new Command(OpenSynchronizationContainer, CanOpenSynchronizationContainer);
            ForceRefreshCommand = new Command(ForceRefresh);
            OpenDocumentCommand = new Command<IDocumentInfo>(OpenDocument);
            CreateShortcutCommand = new Command(CreateShortcut);

            RefreshChildren();
        }

        private void CreateDocument()
        {
            CatchException(() =>
            {
                var fileDialog = new OpenFileDialog();
                var result = fileDialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var fileName = fileDialog.FileName;
                    using (var fileStream = File.OpenRead(fileName))
                    {
                        var parameters = new Dictionary<string, object>
                        {
                            {WellKnownEditKey.Document.Name, Path.GetFileName(fileName)},
                            {WellKnownEditKey.Document.Suffix, Path.GetExtension(fileName)}
                        };

                        var operationInfo = DocumentContainer.CanCreateChild(parameters, fileStream);
                        if (!operationInfo.CheckForAnyRestriction(nameof(IDocumentContainer.CanCreateChild))) return;

                        DocumentContainer.CreateChild(parameters, fileStream);
                    }

                    RefreshChildren(true);
                }
            });
        }

        private void CreateShortcut()
        {
            CatchException(() =>
            {
                var fileDialog = new OpenFileDialog();
                var result = fileDialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var fileName = fileDialog.FileName;
                    var parameters = new Dictionary<string, object>
                        {
                            {WellKnownEditKey.Document.Name, Path.GetFileName(fileName)},
                            {WellKnownEditKey.Document.Suffix, Path.GetExtension(fileName)},
                            {WellKnownEditKey.Document.SourceFileName, fileName}
                        };

                    var operationInfo = DocumentContainer.CanCreateSymbolicLink(parameters);
                    if (!operationInfo.CheckForAnyRestriction(nameof(IDocumentContainer.CanCreateSymbolicLink))) return;

                    DocumentContainer.CreateSymbolicLink(parameters);

                    RefreshChildren(true);
                }
            });
        }

        private void DeleteDocument(IDocumentInfo documentInfo)
        {
            CatchException(() =>
            {
                if (documentInfo == null) return;

                var operationInfo = DocumentContainer.CanDeleteChild(documentInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IDocumentContainer.CanDeleteChild))) return;

                DocumentContainer.DeleteChild(documentInfo);
                RefreshChildren(true);
            });
        }

        private void CopyDocument(IDocumentInfo documentInfo)
        {
            CatchException(() =>
            {
                if (documentInfo == null) return;

                ClipboardHelper.Data = new ClipboardData<DocumentContainerViewModel, IDocumentInfo>
                {
                    Sender = this,
                    Content = documentInfo
                };
            });
        }

        private void PasteDocument()
        {
            CatchException(() => { MoveOrCopy(DocumentContainer.CanCopyFrom, DocumentContainer.CopyFrom); });
        }

        private void MoveDocument()
        {
            CatchException(() => { MoveOrCopy(DocumentContainer.CanMoveFrom, DocumentContainer.MoveFrom); });
        }

        private void MoveOrCopy(Func<IDocumentInfo, IOperationInfo> canExecuteOperation,
            Func<IDocumentInfo, ICoreInfoResult<IDocumentInfo>> operation)
        {
            CatchException(() =>
            {
                if (ClipboardHelper.Data == null)
                    throw new InvalidOperationException("Clipboard is empty.");

                var clipboardData = ClipboardHelper.Data as ClipboardData<DocumentContainerViewModel, IDocumentInfo>;
                if (clipboardData == null)
                    throw new InvalidOperationException(
                        $"Clipboard data is not valid. Clipboard data is of type '{ClipboardHelper.Data.GetType()}'.");

                var operationInfo = canExecuteOperation(clipboardData.Content);
                if (!operationInfo.CheckForAnyRestriction(nameof(canExecuteOperation))) return;

                var newProjectInfo = operation(clipboardData.Content);
                if (newProjectInfo == null)
                    throw new InfoIsNullException();

                clipboardData.Sender.RefreshChildren(true);
                RefreshChildren();
            });
        }

        private bool CanOpenSynchronizationContainer()
        {
            return true;
        }

        private void OpenSynchronizationContainer()
        {
            CatchException(() =>
            {
                if (_synchronizationContainerViewModel != null) return;

                _synchronizationContainerViewModel = ViewProvider.ViewModelFactory.GetSynchronizationContainerViewModel(DocumentContainer);
                _synchronizationContainerViewModel.SynchronizedEventReceived +=
                    SynchronizationViewModelOnSynchronizedEventReceived;
                Show(_synchronizationContainerViewModel, delegate { OnSynchronizationContainerClosed(); });
            });
        }

        private void OnSynchronizationContainerClosed()
        {
            _synchronizationContainerViewModel.SynchronizedEventReceived -= SynchronizationViewModelOnSynchronizedEventReceived;
            _synchronizationContainerViewModel = null;
        }

        private void SynchronizationViewModelOnSynchronizedEventReceived(object sender,
            SynchronizedEventReceivedEventArgs eventArgs)
        {
            CatchException(() =>
            {
                var synchronizedEvent = eventArgs.SynchronizedEvent;
                RefreshChildren(true);
                DocumentContainer.SynchronizationContainer.SetHandled(synchronizedEvent);
            });
        }

        private void ForceRefresh()
        {
            CatchException(() =>
            {
                RefreshChildren(true);
            });
        }

        public void RefreshChildren(bool hardRefresh = false)
        {
            // no need to refresh when view model was disposed already
            if (DocumentContainer == null)
                return;
                
            if (hardRefresh)
                DocumentContainer.RefreshChildren();

            Documents.Load(DocumentContainer.ChildrenInfos);
        }

        public void OpenDocument(IDocumentInfo documentInfo)
        {
            CatchException(() =>
            {
                var operationInfo = DocumentContainer.CanGetChild(documentInfo);
                if (!operationInfo.CheckForAnyRestriction(nameof(IDocumentContainer.CanGetChild))) return;

                var document = DocumentContainer.GetChild(documentInfo);
                var documentViewModel = ViewProvider.ViewModelFactory.GetDocumentViewModel(document);
                documentViewModel.DocumentRefreshed += OnDocumentRefreshed;
                Show(documentViewModel, viewModel =>
                {
                    viewModel.DocumentRefreshed -= OnDocumentRefreshed;
                });
            });
        }

        private void OnDocumentRefreshed(object sender, EventArgs eventArgs)
        {
            RefreshChildren();
        }
    }
}
