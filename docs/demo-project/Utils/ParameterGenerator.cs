using Ofcas.Lk.Api.Client.Core;
using Ofcas.Lk.Api.Client.Demo.Models;
using Ofcas.Lk.Api.Client.Demo.Models.Interfaces;
using Ofcas.Lk.Api.Shared;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public static class ParameterGenerator
    {
        public static ObservableCollection<IParameter> CreateCommonParametersForElevationCreation(IProject project)
        {
            if (!project.Parent.LoginScope.CanGetElementTypes().CheckForAnyRestriction(nameof(ILoginScope.CanGetElementTypes))) return default(ObservableCollection<IParameter>);

            var elementTypes = project.Parent.LoginScope.GetElementTypes().ElementTypes;

            if (!project.CanGetNextPositionNumber().CheckForAnyRestriction(nameof(IProject.CanGetNextPositionNumber))) return default(ObservableCollection<IParameter>);

            return new ObservableCollection<IParameter>
            {
                new Parameter<string>
                {
                    Key = WellKnownEditKey.Elevation.Name,
                    Value = project.GetNextPositionNumber().Value
                },
                new Parameter<IElementType>
                {
                    Key = WellKnownEditKey.Elevation.ElementType,
                    Value = elementTypes.FirstOrDefault(),
                    RestrictedValues = elementTypes.ToList()
                },
                new Parameter<string>
                {
                    Key = WellKnownEditKey.Elevation.Amount,
                    Value = "1"
                },
                new Parameter<string>
                {
                    Key = WellKnownEditKey.Elevation.UserDescription,
                    Value = string.Empty
                },
                new Parameter<string>
                {
                    Key = WellKnownEditKey.Elevation.ModelDescription,
                    Value = string.Empty
                },
                new Parameter<bool>
                {
                    Key = WellKnownEditKey.Elevation.Alternative,
                    Value = false
                },
            };
        }
    }
}
