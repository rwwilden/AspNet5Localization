using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Localization.JsonLocalizer.StringLocalizer
{
    public class LocalizerUtilTests
    {
        [Fact]
        public void TrimPrefixWithNullName()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => LocalizerUtil.TrimPrefix(null, ""));
            Assert.Equal(ex.ParamName, "name");
        }

        [Fact]
        public void TrimPrefixWithNullPrefix()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => LocalizerUtil.TrimPrefix("", null));
            Assert.Equal(ex.ParamName, "prefix");
        }

        [Theory]
        [InlineData("ABC", "")]
        public void TrimPrefixWithEmptyPrefix(string name, string prefix)
        {
            Assert.Equal(name, LocalizerUtil.TrimPrefix(name, prefix));
        }

        [Theory]
        [InlineData("ABC", "X")]
        [InlineData("ABC", "BC")]
        [InlineData("ABC", "ABCD")]
        public void TrimPrefixWithNonMatchingPrefix(string name, string prefix)
        {
            Assert.Equal(name, LocalizerUtil.TrimPrefix(name, prefix));
        }

        [Theory]
        [InlineData("ABC", "a")]
        [InlineData("ABC", "abc")]
        public void TrimPrefixWithDifferentCasing(string name, string prefix)
        {
            Assert.Equal(name, LocalizerUtil.TrimPrefix(name, prefix));
        }

        [Theory]
        [InlineData("ABC", "A")]
        [InlineData("ABC", "AB")]
        [InlineData("ABC", "ABC")]
        public void TrimPrefix(string name, string prefix)
        {
            Assert.Equal(name.Substring(prefix.Length), LocalizerUtil.TrimPrefix(name, prefix));
        }

        [Fact]
        public void ExpandPathsWithNullName()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => LocalizerUtil.ExpandPaths(null, ""));
            Assert.Equal(ex.ParamName, "name");
        }

        [Fact]
        public void ExpandPathsWithNullBaseName()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => LocalizerUtil.ExpandPaths("", null));
            Assert.Equal(ex.ParamName, "baseName");
        }

        [Theory]
        [InlineData("", "")]
        public void ExpandPathsWithEmptyNames(string name, string baseName)
        {
            Assert.Empty(LocalizerUtil.ExpandPaths(name, baseName));
        }

        [Theory]
        [InlineData("A", "")]
        public void ExpandPathsWithOneComponentAndEmptyPrefix(string name, string baseName)
        {
            Assert.Collection(LocalizerUtil.ExpandPaths(name, baseName),
                path => Assert.Equal("A", path));
        }

        [Theory]
        [InlineData("A.B", "")]
        public void ExpandPathsWithTwoComponentsAndEmptyPrefix(string name, string baseName)
        {
            Assert.Collection(LocalizerUtil.ExpandPaths(name, baseName).OrderBy(p => p),
                path => Assert.Equal("A.B", path),
                path => Assert.Equal("A" + Path.DirectorySeparatorChar + "B", path));
        }

        [Theory]
        [InlineData("A.B.C", "")]
        public void ExpandPathsWithThreeComponentsAndEmptyPrefix(string name, string baseName)
        {
            Assert.Collection(LocalizerUtil.ExpandPaths(name, baseName).OrderBy(p => p),
                path => Assert.Equal("A.B.C", path),
                path => Assert.Equal("A" + Path.DirectorySeparatorChar + "B.C", path),
                path => Assert.Equal("A" + Path.DirectorySeparatorChar + "B" + Path.DirectorySeparatorChar + "C", path));
        }
        
        [Theory]
        [InlineData("A", "B")]
        public void ExpandPathsWithNonMatchingPrefix(string name, string baseName)
        {
            Assert.Collection(LocalizerUtil.ExpandPaths(name, baseName),
                path => Assert.Equal("A", path));
        }
        
        [Theory]
        [InlineData("A", "A")]
        public void ExpandPathsWithOneComponent(string name, string baseName)
        {
            Assert.Collection(LocalizerUtil.ExpandPaths(name, baseName),
                path => Assert.Equal("A", path));
        }
        
        [Theory]
        [InlineData("A.B", "A")]
        public void ExpandPathsWithTwoComponents(string name, string baseName)
        {
            Assert.Collection(LocalizerUtil.ExpandPaths(name, baseName).OrderBy(p => p),
                path => Assert.Equal("A.B", path),
                path => Assert.Equal("A" + Path.DirectorySeparatorChar + "B", path),
                path => Assert.Equal("B", path));
        }
        
        [Theory]
        [InlineData("A.B.C", "A")]
        public void ExpandPathsWithThreeComponents(string name, string baseName)
        {
            Assert.Collection(LocalizerUtil.ExpandPaths(name, baseName).OrderBy(p => p),
                path => Assert.Equal("A.B.C", path),
                path => Assert.Equal("A" + Path.DirectorySeparatorChar + "B.C", path),
                path => Assert.Equal("A" + Path.DirectorySeparatorChar + "B" + Path.DirectorySeparatorChar + "C", path),
                path => Assert.Equal("B.C", path),
                path => Assert.Equal("B" + Path.DirectorySeparatorChar + "C", path));
        }
    }
}
