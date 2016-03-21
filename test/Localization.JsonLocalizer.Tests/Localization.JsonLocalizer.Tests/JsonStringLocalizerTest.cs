using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Localization.JsonLocalizer.StringLocalizer
{
    public class JsonStringLocalizerTest
    {
        [Fact]
        public void CreateWithNullBaseName()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new JsonStringLocalizer(null, "", null));
            Assert.Equal(ex.ParamName, "baseName");
        }
    }
}
