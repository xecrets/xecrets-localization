# Xecrets Localization

Provide translations from embedded .po files using definitions and fallbacks from .resx resources.

This package is used to handle translations in the Xecrets Ez desktop application for Windows, Linux
and macOS, using the Avalonia UI framework. See https://www.axantum.com/ .

It bridges the gap between .NET's localization and GNU gettext's. Traditionally gettext parses
source files for strings to translate, extracts them to .pot files, and then this template is used
to create .po files for each language.

Using the POTools from https://github.com/adams85/aspnetskeleton2/tree/master/tools/POTools the
first step is to extract strings from .resx files to .pot files. The second step is to create .po
files for each language. The third step is to embed the .po files in the assembly.

The .po files should be located in a separate directory for each culture, named after the culture
for example "en-US" or "sv-SE". Include the files in the project and set the "Build Action" to
"Embedded Resource". This will cause the resource to be named based on the path and the names, with
path separators replaced by ".", and dashes replaced by underscores. This is expected and required
by the `Xecrets.Localization` package.

Once the .po files are embedded in the assembly, the `Xecrets.Localization` package can be used to
provide translations for the strings in the .resx files.

For a console application, register the implementations in `Program.cs` similar to this:

```csharp
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ITranslationsProvider, POTranslationsProvider>((_) => new POTranslationsProvider(Assembly.GetExecutingAssembly()))
builder.Services.AddSingleton<IStringLocalizerFactory, POStringLocalizerFactory>()
builder.Services.AddTransient<IStringLocalizer>((_) => New<IStringLocalizerFactory>().Create(string.Empty, "Embedded .po resources"))

using IHost host = builder.Build();
```

For an Avalonia UI application, register the implementations in `Program.cs` similar to this:

```csharp
IServiceCollection serviceCollection = new ServiceCollection()
    .AddSingleton<ITranslationsProvider, POTranslationsProvider>((_) => new POTranslationsProvider(Assembly.GetExecutingAssembly()))
    .AddSingleton<IStringLocalizerFactory, POStringLocalizerFactory>()
    .AddTransient<IStringLocalizer>((_) => New<IStringLocalizerFactory>().Create(string.Empty, "Embedded .po resources"))

ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
```

Now you can use the `IStringLocalizer` interface to get the translations, similar to this:
    
```csharp
IStringLocalizer T => serviceProvider.GetRequiredService<IStringLocalizer>();

string translated = T[Resources.HellWorld];
```

This nicely ties together the .resx resources and the .po translations, and provides a neat way to
translate strings in a .NET application using the GNU gettext tools.

Since the Resource lookup actually gets the original untranslated string from the .resx file, it is
easy to use the same string in multiple places in the code, and still get the correct translation.
This is not possible with the traditional gettext approach. It also makes it easy and natural to
provide the fallback.

You can still use refactoring tools etc to find references to a string in the code.

Note - pluralization is not supported in this version. It may be supported in a future version.
Currently we roll our own, using a convention to have the singular and plural forms in the same
string, with a separator, and then split the string in the code, for example ``"No files|One
file|{0} files"``." This does not handle all possible cases, but it is sufficient for our needs at
this time.