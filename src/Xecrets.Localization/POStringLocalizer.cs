#region Copyright and License

/*
 * Xecrets Ez - Copyright © 2022-2025 Svante Seleborg, All Rights Reserved.
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

#endregion Copyright and License

using Karambolo.PO;

using Microsoft.Extensions.Localization;

using System.Globalization;

namespace Xecrets.Localization;

/// <summary>
/// Localizes strings using a <see cref="ITranslationsProvider"/>.
/// </summary>
/// <param name="translationsProvider"></param>
/// <param name="location"></param>
/// <param name="culture"></param>
public sealed class POStringLocalizer(ITranslationsProvider translationsProvider, string location, CultureInfo? culture = null) : IStringLocalizer
{
    private readonly CultureInfo _culture = culture ?? CultureInfo.CurrentUICulture;

    /// <summary>
    /// Translates a string by name, falling back to the name if no translation is found.
    /// </summary>
    /// <param name="name">The string to translate, typically the english original.</param>
    /// <returns>A translated string.</returns>
    public LocalizedString this[string name]
    {
        get
        {
            bool resourceNotFound = !TryGetTranslation(name, out var searchedLocation, out var value);
            return new LocalizedString(name, value!, resourceNotFound, searchedLocation);
        }
    }

    /// <summary>
    /// Translates a string by name, falling back to the name if no translation is found.
    /// Also formats the string with the provided arguments.
    /// </summary>
    /// <param name="name">The string to translate, typically the english original.</param>
    /// <param name="arguments">Arguments passed to string.Format()</param>
    /// <returns>A translated string.</returns>
    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            bool resourceNotFound = !TryGetTranslation(name, out string searchedLocation, out string value);
            return new LocalizedString(name, string.Format(value!, arguments), resourceNotFound, searchedLocation);
        }
    }

    /// <summary>
    /// Try to get a translation for a given name.
    /// </summary>
    /// <param name="name">The name of the string to get.</param>
    /// <param name="searchedLocation">Where we're looking for the translation.</param>
    /// <param name="value">The translated value, or falls back to name.</param>
    /// <returns>true if a translation was found.</returns>
    public bool TryGetTranslation(string name, out string searchedLocation, out string value)
    {
        bool wasFound = TryInternal(location, name, out searchedLocation, out value);
        value = value.Replace(@"\n", Environment.NewLine);
        return wasFound;
    }

    private bool TryInternal(string location, string name, out string searchedLocation, out string value)
    {
        searchedLocation = location;

        IReadOnlyDictionary<string, POCatalog> catalogs = translationsProvider.GetCatalogs();
        POCatalog? catalog = GetCatalog(catalogs);
        if (catalog == null)
        {
            value = name;
            return false;
        }

        POKey key = new(name.Replace("\r\n", "\n"));
        string? translation = catalog.GetTranslation(key);
        if (translation == null)
        {
            catalogs.TryGetValue("en-US", out POCatalog? englishCatalog);
            translation = englishCatalog?.GetTranslation(key) ?? key.Id;
        }

        value = translation;
        return value != key.Id;
    }

    private POCatalog? GetCatalog(IReadOnlyDictionary<string, POCatalog> catalogs)
    {
        CultureInfo culture = _culture;
        while (true)
        {
            if (catalogs.TryGetValue(culture.Name, out POCatalog? catalog))
            {
                return catalog;
            }

            CultureInfo parentCulture = culture.Parent;
            if (culture == parentCulture)
            {
                catalogs.TryGetValue("en-US", out POCatalog? englishCatalog);
                return englishCatalog;
            }

            culture = parentCulture;
        }
    }

    /// <summary>
    /// Get all strings for the current culture.
    /// </summary>
    /// <param name="includeParentCultures"></param>
    /// <returns></returns>
    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        IReadOnlyDictionary<string, POCatalog> catalogs = translationsProvider.GetCatalogs();
        CultureInfo culture = _culture;
        do
        {
            if (catalogs.TryGetValue(culture.Name, out var catalog))
            {
                foreach (IPOEntry? entry in catalog)
                {
                    if (entry.Count > 0)
                    {
                        yield return new LocalizedString(entry.Key.Id, entry[0], resourceNotFound: false, location);
                    }
                }
            }

            CultureInfo parentCulture = culture.Parent;
            if (culture == parentCulture)
            {
                break;
            }

            culture = parentCulture;
        }
        while (includeParentCultures);
    }
}
