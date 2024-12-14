using Wpf.Ui;

namespace Controller.Services
{
    /// <summary>
    /// Service that provides pages for navigation.
    /// </summary>
    /// <remarks>
    /// Creates new instance and attaches the <see cref="IServiceProvider"/>.
    /// </remarks>
    public class PageService(IServiceProvider serviceProvider) : IPageService
    {
        /// <summary>
        /// Service which provides the instances of pages.
        /// </summary>
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        /// <inheritdoc />
        public T? GetPage<T>()
            where T : class
        {
            if (!typeof(FrameworkElement).IsAssignableFrom(typeof(T)))
                throw new InvalidOperationException("The page should be a WPF control.");

            return (T?)_serviceProvider.GetService(typeof(T));
        }

        /// <inheritdoc />
        public FrameworkElement? GetPage(Type pageType)
        {
            if (!typeof(FrameworkElement).IsAssignableFrom(pageType))
                throw new InvalidOperationException("The page should be a WPF control.");

            return _serviceProvider.GetService(pageType) as FrameworkElement;
        }
    }
}
