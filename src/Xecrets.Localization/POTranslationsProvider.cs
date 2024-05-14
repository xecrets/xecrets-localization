#region Coypright and License

/*
 * Xecrets Ez - Copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Ez - A cross platform desktop application
 * for encryption, decryption and other file operations based on Xecrets Cli.
 * 
 * The MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the “Software”), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * This code is loosely based on and inspired by sample code from
 * https://github.com/adams85/po which is licensed under the MIT License.
*/

#endregion Coypright and License

using Karambolo.PO;

using System.Reflection;

namespace Xecrets.Localization
{
    /// <summary>
    /// Implementation of <see cref="ITranslationsProvider"/> that loads translations from embedded .po files.
    /// </summary>
    public class POTranslationsProvider : ITranslationsProvider
    {
        private static readonly POParserSettings _parserSettings = new POParserSettings
        {
            SkipComments = true,
            SkipInfoHeaders = true,
            StringDecodingOptions = new POStringDecodingOptions { KeepKeyStringsPlatformIndependent = true }
        };

        private readonly Assembly _resourceAssembly;

        private readonly IReadOnlyDictionary<string, POCatalog> _catalogs;

        /// <summary>
        /// Instantiates a new instance of <see cref="POTranslationsProvider"/>.
        /// </summary>
        /// <param name="resourceAssembly">The assembly to get embedded resource files from.</param>
        /// <remarks>
        /// The resource files must be located in a folder with subfolders for each culture, according to the pattern:
        /// .../[foldername]/[language-country]/[arbitraryname].po, resulting in a resource name like
        /// .../[foldername].[language_country].[arbitraryname].po, for example "Translations.en_US.MyTranslations.po".
        /// </remarks>
        public POTranslationsProvider(Assembly resourceAssembly)
        {
            _resourceAssembly = resourceAssembly;
            _catalogs = LoadFromResources();
        }

        private Dictionary<string, POCatalog> LoadFromResources()
        {
            string[] resources = _resourceAssembly.GetManifestResourceNames();

            return resources
                .Where(r => r.EndsWith(".po"))
                .Select(LoadFile)
                .Where(item => item != null)
                .ToDictionary(item => item!.Value.Culture, item => item!.Value.Catalog);
        }

        private (POCatalog Catalog, string Culture)? LoadFile(string resource)
        {
            string[] parts = resource.Split('.');
            string culture = parts[parts.Length - 3].Replace('_', '-');

            POCatalog? catalog = LoadTranslations(resource);
            return catalog == null ? null : (catalog, culture);
        }

        private POCatalog? LoadTranslations(string resourcePath)
        {
            using Stream stream = _resourceAssembly.GetManifestResourceStream(resourcePath)!;

            POParseResult parseResult = new POParser(_parserSettings).Parse(stream);
            if (!parseResult.Success)
            {
                IEnumerable<Diagnostic> diagnosticMessages = parseResult.Diagnostics
                    .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

                throw new InvalidOperationException($"Translation file '{resourcePath}' has errors: {string.Join(Environment.NewLine, diagnosticMessages)}");
            }

            return parseResult.Catalog;
        }

        /// <summary>
        /// Get a cached dictionary of catalogs.
        /// </summary>
        /// <returns>A read only dictionary.</returns>
        public IReadOnlyDictionary<string, POCatalog> GetCatalogs() => _catalogs;
    }
}
