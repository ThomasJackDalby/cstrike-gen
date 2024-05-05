namespace CSMP.IO
{
    internal class MapTemplate
    {
        [VmfProperty("versioninfo")]
        public VersionInfoTemplate? VersionInfo { get; set; }
        [VmfProperty("visgroups")]
        public VersionInfoTemplate? VisGroups { get; set; }
        [VmfProperty("viewsettings")]
        public ViewSettingsTemplate? ViewSettings { get; set; }
        [VmfProperty("world")]
        public WorldTemplate? World { get; set; }
    }

    public class WorldTemplate
    {
        [VmfProperty("id")]
        public string? ID { get; set; }
        [VmfProperty("mapversion")]
        public string? MapVersion { get; set; }
        [VmfProperty("classname")]
        public string? ClassName { get; set; }
        [VmfProperty("detailmaterial")]
        public string? DetailMaterial { get; set; }
        [VmfProperty("detailvbsp")]
        public string? DetailVBSP { get; set; }
        [VmfProperty("maxpropscreenwidth")]
        public string? MaxPropScreenWidth { get; set; }
        [VmfProperty("skyname")]
        public string? SkyName { get; set; }
    }

    public class ViewSettingsTemplate
    {
    }

    public class VersionInfoTemplate
    {
    }
}
