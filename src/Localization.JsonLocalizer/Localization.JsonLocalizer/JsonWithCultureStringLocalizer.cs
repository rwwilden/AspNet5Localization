using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Localization.JsonLocalizer.StringLocalizer
{
    public class JsonWithCultureStringLocalizer : JsonStringLocalizer
    {
        private CultureInfo _culture;
        
        public JsonWithCultureStringLocalizer(string baseName, string applicationName, CultureInfo culture, ILogger logger)
            : base(baseName, applicationName, logger)
        {
            if (baseName == null)
            {
                throw new ArgumentNullException(nameof(baseName));
            }
            if (applicationName == null)
            {
                throw new ArgumentNullException(nameof(applicationName));
            }
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            
            this._culture = culture;
        }
        
        public override LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                
                var value = GetLocalizedString(name, _culture);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }
        
        public override LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                
                var format = GetLocalizedString(name, _culture);
                var value = string.Format(format ?? name, arguments);
                return new LocalizedString(name, value, resourceNotFound: format == null);
            }
        }
        
        public override IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            GetAllStrings(includeParentCultures, _culture); 
    }
}
