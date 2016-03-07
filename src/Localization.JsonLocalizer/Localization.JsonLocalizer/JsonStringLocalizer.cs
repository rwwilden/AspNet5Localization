using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Localization.JsonLocalizer.StringLocalizer
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly ConcurrentDictionary<string, Lazy<JObject>> _resourceObjectCache =
            new ConcurrentDictionary<string, Lazy<JObject>>();

        private readonly string _baseName;
        private readonly string _applicationName;
        private readonly ILogger _logger;
        private readonly IEnumerable<string> _resourceFileLocations;

        public JsonStringLocalizer(string baseName, string applicationName, ILogger logger)
        {
            this._baseName = baseName;
            this._applicationName = applicationName;
            this._logger = logger;

            // Get a list of possible resource file locations.
            _resourceFileLocations = LocalizerUtil.ExpandPaths(baseName, applicationName).ToList();
            foreach (var resFileLocation in _resourceFileLocations)
            {
                logger.LogVerbose($"Resource file location base path: {resFileLocation}");
            }
        }

        public LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var value = GetLocalizedString(name);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var format = GetLocalizedString(name);
                var value = string.Format(format ?? name, arguments);
                return new LocalizedString(name, value, resourceNotFound: format == null);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeAncestorCultures)
        {
            throw new NotImplementedException();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
        private string GetLocalizedString(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            
            // Attempt to find resource file.
            var currentCulture = CultureInfo.CurrentCulture;
            var resourceObject = GetResourceObject(currentCulture);
            if (resourceObject == null)
            {
                _logger.LogInformation($"No resource file found or error occurred for base name {_baseName} and culture {currentCulture}.");
                return null;
            }

            // Attempt to get resource with the given name from the resource object.
            JToken value;
            if (resourceObject.TryGetValue(name, out value))
            {
                var localizedString = value.ToString();
                return localizedString;
            }

            _logger.LogInformation($"Could not find key '{name}' in resource file for base name {_baseName} and culture {currentCulture}.");
            return null;
        }

        private JObject GetResourceObject(CultureInfo currentCulture)
        {
            var cultureSuffixes = GetCultureSuffixes(currentCulture);

            // Check all locations starting with the most specific suffix.
            foreach (var cultureSuffix in cultureSuffixes)
            {
                var lazyJObjectGetter = new Lazy<JObject>(() =>
                {
                    // First attempt to find a resource file location that exists.
                    string resourcePath = null;
                    foreach (var resourceFileLocation in _resourceFileLocations)
                    {
                        resourcePath = resourceFileLocation + cultureSuffix + "json";
                        if (File.Exists(resourcePath))
                        {
                            _logger.LogInformation($"Resource file location {resourcePath} found.");
                            break;
                        }
                        else
                        {
                            _logger.LogVerbose($"Resource file location {resourcePath} does not exist.");
                            resourcePath = null;
                        }
                    }
                    if (resourcePath == null)
                    {
                        _logger.LogVerbose($"No resource file found for suffix {cultureSuffix}");
                        return null;
                    }

                    // Found a resource file path: attempt to parse it into a JObject.
                    try
                    {
                        var resourceFileStream =
                            new FileStream(resourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
                        using (resourceFileStream)
                        {
                            var resourceReader =
                                new JsonTextReader(new StreamReader(resourceFileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true));
                            using (resourceReader)
                            {
                                return JObject.Load(resourceReader);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Error occurred attempting to read JSON resource file {resourcePath}: {e}");
                        return null;
                    }

                }, LazyThreadSafetyMode.ExecutionAndPublication);

                lazyJObjectGetter = _resourceObjectCache.GetOrAdd(cultureSuffix, lazyJObjectGetter);
                var resourceObject = lazyJObjectGetter.Value;
                if (resourceObject != null)
                {
                    return resourceObject;
                }
            }
            return null;
        }

        private string[] GetCultureSuffixes(CultureInfo currentCulture)
        {
            // Get culture suffixes (e.g.: { "nl-NL.", "nl.", "" }).
            string[] cultureSuffixes;
            if (currentCulture == null)
            {
                cultureSuffixes = new[] { "" };
            }
            else
            {
                if (currentCulture.IsNeutralCulture)
                {
                    cultureSuffixes = new[] { currentCulture.Name + ".", "" };
                }
                else
                {
                    cultureSuffixes = new[] { currentCulture.Name + ".", currentCulture.Parent.Name + ".", "" };
                }
            }
            
            var cultureSuffixesLogString = string.Join(", ", cultureSuffixes);
            _logger.LogVerbose($"Using culture suffixes {cultureSuffixesLogString}");
            return cultureSuffixes;
        }
    }
}
