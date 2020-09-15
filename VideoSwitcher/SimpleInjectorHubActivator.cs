using Microsoft.AspNetCore.SignalR;

using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace VideoSwitcher
{
    public sealed class SimpleInjectorHubActivator<T>
    : IHubActivator<T> where T : Hub
    {
        private readonly Container container;
        private Scope scope;

        public SimpleInjectorHubActivator(Container container) =>
            this.container = container;

        public T Create()
        {
            scope = AsyncScopedLifestyle.BeginScope(container);
            return container.GetInstance<T>();
        }

        public void Release(T hub) => scope.Dispose();
    }
}
