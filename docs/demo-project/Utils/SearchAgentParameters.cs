using Ofcas.Lk.Api.Shared;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public class SearchAgentParameters
    {
        public class Types
        {
            public static class Unknown
            {
                public const string DisplayName = "Unknown";
                public const SearchType Type = SearchType.Unknown;
            }

            public static class Project
            {
                public const string DisplayName = "Project";
                public const SearchType Type = SearchType.Project;
            }
        }

        public static class ProjectDirectory
        {
            public const string DisplayName = "Directory";
        }
    }
}
