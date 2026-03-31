using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Mvvm;
using Ofcas.Lk.Api.Client.Demo.Utils;
using Ofcas.Lk.Api.Shared;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace Ofcas.Lk.Api.Client.Demo.ViewModels
{
    public class DocumentViewModel : CoreObjectViewModel, IExtendedDisposable
    {
        public event EventHandler DocumentRefreshed;

        private ICoreObjectResult<IDocument> DocumentResult;
        private IDocument Document => DocumentResult.CoreObject;
        private DocumentModel _documentModel;

        public DocumentModel DocumentModel
        {
            get => _documentModel;
            set { _documentModel = value; OnPropertyChanged(); }
        }

        public ICommand EditCommand { get; }
        public ICommand ReadDocumentCommand { get; }
        public ICommand WriteDocumentCommand { get; }

        public DocumentViewModel(IViewProvider viewProvider, ICoreObjectResult<IDocument> document)
            : base(viewProvider, document)
        {
            Throw.IfNull(document, nameof(document));
            DocumentResult = document;

            EditCommand = new Command<string>(Edit);
            ReadDocumentCommand = new Command<bool>(ReadDocument);
            WriteDocumentCommand = new Command<bool>(WriteDocument);

            Refresh();
        }

        private void Edit(string editKey)
        {
            CatchException(() =>
            {
                var title = $"Edit {editKey}";
                object value;

                switch (editKey)
                {
                    case WellKnownEditKey.Document.Name:
                        var nameInputBoxViewModel = ViewProvider.ViewModelFactory.GetInputBoxViewModel(title, Document.Info.DisplayName);
                        if (!ShowDialog(nameInputBoxViewModel))
                            return;
                        value = nameInputBoxViewModel.GetValue();
                        break;

                    default:
                        throw new ArgumentException($"The edit key '{editKey}' is not valid.");
                }
                var operationInfo = Document.CanEdit(editKey, value);
                if (!operationInfo.CheckForAnyRestriction(nameof(IDocument.CanEdit)))
                    return;

                Document.Edit(editKey, value);

                Refresh(true);
            });
        }

        private void ReadDocument(bool writeAccess)
        {
            CatchException(() =>
            {
                var tempFile = GetContentAsFile(Document, writeAccess);
                if (string.IsNullOrEmpty(tempFile)) return;

                var fileProcess = Process.Start(tempFile);
                if (fileProcess != null)
                    fileProcess.WaitForExit();

                File.Delete(tempFile);
            });
        }

        private void WriteDocument(bool writeAccess)
        {
            CatchException(() =>
            {
                var tempFile = GetContentAsFile(Document, writeAccess);
                if (string.IsNullOrEmpty(tempFile)) return;

                var fileProcess = Process.Start(tempFile);
                if (fileProcess != null)
                {
                    fileProcess.WaitForExit();

                    using (var fileStream = new FileStream(tempFile, FileMode.Open))
                    {
                        var operationInfo = Document.CanSetContent(fileStream);
                        if (!operationInfo.CheckForAnyRestriction(nameof(IDocument.CanSetContent))) return;

                        Document.SetContent(fileStream);
                    }
                }
                File.Delete(tempFile);

                Refresh(true);
            });
        }

        private string GetContentAsFile(IDocument document, bool writeAccess)
        {
            var tempFile = Path.ChangeExtension(Path.GetTempFileName(), document.Info.Suffix);
            var operationInfo = document.CanGetContent(writeAccess);
            if (!operationInfo.CheckForAnyRestriction(nameof(IDocument.CanGetContent))) return default(string);

            using (var content = document.GetContent(writeAccess).Stream)
            {
                using (var fileStream = File.Create(tempFile))
                    content.CopyTo(fileStream);
            }
            return tempFile;
        }

        public bool CanDispose(out string falseReason)
        {
            return CanDispose(DocumentResult, out falseReason);
        }

        public void Dispose()
        {
            if (DocumentResult == null) return;

            DocumentResult.Dispose();
            DocumentResult = null;
        }

        private void Refresh(bool hardRefresh = false, bool suppressEvent = false)
        {
            if (hardRefresh)
                Document.Refresh();

            DocumentModel = new DocumentModel
            {
                CoreObjectId = Document.Id,
                Guid = Document.Info.Guid,
                DisplayName = Document.Info.DisplayName,
                Suffix = Document.Info.Suffix,
                Uri = Document.Info.Uri
            };

            if (!suppressEvent)
                DocumentRefreshed?.Invoke(this, EventArgs.Empty);
        }
    }
}
