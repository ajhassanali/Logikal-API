using System;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public static class ExportFileFormat
    {
        public static string GetFileExtension(string displayName)
        {
            switch (displayName)
            {
                case SQLite.DisplayName:
                    return SQLite.FileExtension;

                case PDF.DisplayName:
                    return PDF.FileExtension;

                case DXF.DisplayName:
                    return DXF.FileExtension;

                case XLSX.DisplayName:
                    return XLSX.FileExtension;

                case OCD.DisplayName:
                    return OCD.FileExtension;

                default:
                    throw new ArgumentOutOfRangeException(nameof(displayName));
            }
        }

        public static class SQLite
        {
            public const string DisplayName = "SQLite";
            public const string FileExtension = "sqlite3";
        }

        public static class PDF
        {
            public const string DisplayName = "PDF";
            public const string FileExtension = "pdf";
        }

        public static class DXF
        {
            public const string DisplayName = "DXF";
            public const string FileExtension = "dxf";
        }

        public static class OCD
        {
            public const string DisplayName = "OCD";
            public const string FileExtension = "ocd";
        }

        public static class XLSX
        {
            public const string DisplayName = "XLSX";
            public const string FileExtension = "xlsx";
        }
    }
}
