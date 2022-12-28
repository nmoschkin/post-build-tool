# .NET Post-Build-Event Tool

This tool is used at the end of successful builds to bump version numbers and build numbers in .NET Sdk-style projects.

There's really not much to it.  You can create custom versionifiers using the __IVersionifier__ interface and load your DLL via the command line switches or the configuration file.  The configuration file is recommended for advanced customization.

Right now the default function is just to versionify. With a custom versionifier it's possible to change other core aspects of a project, but nothing outside of the top section, unless you override the writer and manipulate the XML yourself, which is also possible.

There is some functionality to scan for NuGet packages and take version numbers from that, but that's it. 

I wrote this tool because I needed exactly this tool (it's hard to bump the version of 35 projects at one time, manually).

So, if someone else finds it useful, more power to them.  If this turns into something bigger, I'll definitely update the documentation.
