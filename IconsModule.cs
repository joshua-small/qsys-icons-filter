using QsysMods.Core;
namespace Designer.Modules.IconsFilter;
public sealed class IconsModule : ModModule
{
    public IconsModule() => Mods.Register("icons-filter", _ => IconSearch.Install((kind, detail) => Host.Receipt("icons-filter", kind, detail)));
}
