using Verse;

namespace DVeLsCommonLibrary.Settings
{
    public class DVeLsCommonLibraryMod : Mod
    {
        public static DVeLsCommonLibraryModSettings settings;

        public DVeLsCommonLibraryMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<DVeLsCommonLibraryModSettings>();
        }

        public override string SettingsCategory()
        {
            return "DVeLsCommonLibrary";
        }
    }
}
