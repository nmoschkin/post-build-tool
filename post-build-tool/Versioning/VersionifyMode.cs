namespace PostBuildTool.Versioning
{
    [Flags]
    public enum VersionifyMode
    {
        BumpBuild = 0x01,
        BumpRevsion = 0x02,
        BumpMinor = 0x04,
        BumpMajor = 0x08,
        BumpMask = 0x0f,
        BuildHour = 0x10,
        RevisionHour = 0x20,
        BuildMinute = 0x40,
        BuildMask = 0xf0,
        RegularMask = 0xff,
        Custom = 0xf000,
        DoNotIncrement = 0x0100,
        SpecialMask = 0xff00,
    }
}