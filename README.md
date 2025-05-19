# .NET Post-Build-Event Tool

## Automated .NET Whole Solution Version Bumping

This tool is used at the end of successful builds to bump version numbers and build numbers for pretty much any .NET project except for the old .NET Framework project styles.

There's really not much to it.  You can create custom versionifiers using the __IVersionifier__ interface and load your DLL via the command line switches or the configuration file.  The configuration file is recommended for advanced customization.

Right now the default function is just to versionify. With a custom versionifier it's possible to change other core aspects of a project, but nothing outside of the top section, unless you override the writer and manipulate the XML yourself, which is also possible.

There is some functionality to scan for NuGet packages and take version numbers from that, but that's it. 

I wrote this tool because I needed exactly this tool (it's hard to bump the version of 35 projects at one time, manually).

So, if someone else finds it useful, more power to them.  If this turns into something bigger, I'll definitely update the documentation.

## Usage

    post-build-tool  [/opt|/options <file>] [/d|/dir <dir>] [/r|/recursive]
                     [/p|/project <project>] [/v|/version <version>]
                     [/ov|/oldversion <version>] [/q|/quiet] [/l|/log <file>]
                     [/lib <dll>] [/cn <name>] [/h|/help] [/m|/mode <mode>] [/ngo]
                     [/ngd <dir>] [/pn <name>] [/ngb <behavior>] [/ngc <constraint>]
                     [/nw|/report]

    /opt              <file>          Use JSON config file. Highly recommended.
    /options

    /d                <dir>           Open and update all .csproj files in the specified directory.
    /dir

    /r                                Recursively scan the directory and sub-directories for .csproj files.
    /recursive

    /p                <project>       Open and update the specified project.
    /project

    /v                <version>       Force the specified version number (as opposed to auto-incrementing.)
    /version

    /ov               <version>       Specify the PreviousVersion variable (as opposed to being automatically calculated.)
    /oldversion

    /q                                Suppress all output.
    /quiet

    /l                <file>          Log output to the specified file.
    /log

    /lib              <dll>           Use an IVersionifier instance from the specified DLL.

    /cn               <name>          Optional class name to be used with /lib.

    /h                                Displays this help screen.
    /help

    /m                <mode>          Set the versioning mode.
    /mode
                         BumpBuild    Bump build by 1 (default)
                       BumpRevsion    Bump revsion by 1
                         BumpMinor    Bump minor version by 1
                         BumpMajor    Bump major version by 1
                         BuildHour    Put the hour of the year in the build number
                      RevisionHour    Put the hour of the year in the revision number
                       BuildMinute    Put the hour of the year in the revision number,
                                      and put the minute of the day in the build number

    /ngo                              Scan NuGet packages to determine most recent last version.

    /ngd              <dir>           Specify the NuGet package directory.

    /pn               <name>          Specify the NuGet package name (used with /ngo.)

    /ngb              <behavior>      Set the behavior if no NuGet packages are found.
                         QuitError    Quit with non-zero exit code (default). This will cause a build failure if run in post-build actions.
                       QuitNoError    Quit with zero exit code.
                    ContinueIgnore    Ignore the NuGet package request and continue without it.

    /ngc              <constraint>    Set the NuGet package constraint.
                           Release    Must be release packages (default).
                             Debug    Must be debug packages.
                          DontCare    No preference.

    /nw                               Report only. Do not write any changes.
    /report


