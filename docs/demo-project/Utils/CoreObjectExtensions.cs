using Ofcas.Lk.Api.Client.Core;

namespace Ofcas.Lk.Api.Client.Demo.Utils
{
    public static class CoreObjectExtensions
    {
        public static T GetParent<T>(this ICoreObjectWithParent coreObjectWithParent)
            where T : ICoreObjectWithChildren
        {
            if (coreObjectWithParent == null)
                return default(T);

            var parent = coreObjectWithParent.Parent;
            while (parent != null)
            {
                if (parent is T)
                    return (T)parent;

                parent = (parent as ICoreObjectWithParent)?.Parent;
            }

            return default(T);
        }
    }
}
