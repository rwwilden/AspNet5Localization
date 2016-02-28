using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;

namespace Localization.JsonLocalizer.StringLocalizer
{
    public class JsonStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ConcurrentDictionary<string, JsonStringLocalizer> _localizerCache =
            new ConcurrentDictionary<string, JsonStringLocalizer>();
        
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly ILogger<JsonStringLocalizerFactory> _logger;
        private string _resourcesRelativePath;

        public JsonStringLocalizerFactory(IApplicationEnvironment applicationEnvironment,
                                          IOptions<JsonLocalizationOptions> localizationOptions,
                                          ILogger<JsonStringLocalizerFactory> logger)
        {
            if (applicationEnvironment == null)
            {
                throw new ArgumentNullException(nameof(applicationEnvironment));
            }
            if (localizationOptions == null)
            {
                throw new ArgumentNullException(nameof(localizationOptions));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this._applicationEnvironment = applicationEnvironment;
            this._logger = logger;
            
            _resourcesRelativePath = localizationOptions.Value.ResourcesPath ?? string.Empty;
            if (!string.IsNullOrEmpty(_resourcesRelativePath))
            {
                _resourcesRelativePath = _resourcesRelativePath
                    .Replace(Path.AltDirectorySeparatorChar, '.')
                    .Replace(Path.DirectorySeparatorChar, '.') + ".";
            }
            
            logger.LogVerbose($"Created {nameof(JsonStringLocalizerFactory)} with:{Environment.NewLine}" +
                $"    (application name: {applicationEnvironment.ApplicationName}{Environment.NewLine}" +
                $"    (resources relative path: {_resourcesRelativePath})");
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            if (resourceSource == null)
            {
                throw new ArgumentNullException(nameof(resourceSource));
            }
            
            _logger.LogVerbose($"Getting localizer for type {resourceSource}.");
            
            var typeInfo = resourceSource.GetTypeInfo();
            var assembly = typeInfo.Assembly;

            // Re-root the base name if a resources path is set
            var baseName = string.IsNullOrEmpty(_resourcesRelativePath)
                ? typeInfo.FullName
                : _applicationEnvironment.ApplicationName + "." + _resourcesRelativePath
                    + LocalizerUtil.TrimPrefix(typeInfo.FullName, _applicationEnvironment.ApplicationName + ".");
            _logger.LogVerbose($"Localizer basename: {baseName}");

            return _localizerCache.GetOrAdd(baseName, _ => new JsonStringLocalizer(baseName, _applicationEnvironment.ApplicationName, _logger));
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            if (baseName == null)
            {
                throw new ArgumentNullException(nameof(baseName));
            }
            
            _logger.LogVerbose($"Getting localizer for baseName {baseName} and location {location}.");
            throw new NotImplementedException();
        }
    }
}
