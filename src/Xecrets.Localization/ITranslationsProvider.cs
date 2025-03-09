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

namespace Xecrets.Localization;

/// <summary>
/// Provides translations from embedded .po files.
/// </summary>
public interface ITranslationsProvider
{
    /// <summary>
    /// Gets the catalogs of translations.
    /// </summary>
    /// <returns></returns>
    IReadOnlyDictionary<string, POCatalog> GetCatalogs();
}
