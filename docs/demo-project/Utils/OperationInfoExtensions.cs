using Ofcas.Lk.Api.Client.Core;
using System.Linq;
using System.Windows;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public static class OperationInfoExtensions
    {
        public static bool CheckForAnyRestriction(this IOperationInfo operationInfo, string methodName)
        {
            if (!operationInfo.Restrictions.Any())
                return true;

            var messageBoxResult = MessageBox.Show(operationInfo.ToString(), $"Restrictions for {methodName}", MessageBoxButton.OKCancel);
            return messageBoxResult == MessageBoxResult.OK;
        }
    }
}
