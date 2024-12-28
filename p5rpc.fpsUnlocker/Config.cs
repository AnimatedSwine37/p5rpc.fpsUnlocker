using System.ComponentModel;
using p5rpc.fpsUnlocker.Template.Configuration;

namespace p5rpc.fpsUnlocker.Configuration;

public class Config : Configurable<Config>
{
    [DisplayName("FPS Limit")]
    [Description("The maximum FPS")]
    [DefaultValue(99999)]
    public float MaxFps { get; set; } = 99999;
    
    [DisplayName("Tie Game Speed to FPS")]
    [Description("If enabled, the game's speed is tied to the FPS. \nDo not enable this if you are sensitive to flashing lights as things animate very fast at high FPS!!!")]
    [DefaultValue(false)]
    public bool TieGameSpeedToFps { get; set; } = false;
    
    [DisplayName("Debug Mode")]
    [Description("Logs additional information to the console that is useful for debugging.")]
    [DefaultValue(false)]
    public bool DebugEnabled { get; set; } = false;
}

/// <summary>
/// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
/// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
/// </summary>
public class ConfiguratorMixin : ConfiguratorMixinBase
{
    // 
}