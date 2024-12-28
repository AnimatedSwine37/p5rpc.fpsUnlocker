using Reloaded.Mod.Interfaces;
using p5rpc.fpsUnlocker.Template;
using p5rpc.fpsUnlocker.Configuration;
using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using static p5rpc.fpsUnlocker.Utils;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace p5rpc.fpsUnlocker;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public unsafe class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    private IAsmHook _fpsCapHook;
    private IAsmHook _gameSpeedHook;

    private float* _maxFps;
    
    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        if (!Utils.Initialise(_logger, _configuration, _modLoader))
        {
            return;
        }

        if (_hooks == null)
        {
            LogError("Failed to get Reloaded Hooks, nothing will work!");
            return;
        }
        
        _maxFps = (float*) Marshal.AllocHGlobal(sizeof(float));
        *_maxFps = _configuration.MaxFps;
        
        SigScan("F3 0F 10 41 ?? 48 8D 71 ?? 48 89 78 ??", "FPS Cap", address =>
        {
            string[] function =
            {
                "use64",
                $"lea rsi, [qword {(nuint)_maxFps}]",
                "movss xmm0, [rsi]"
            };
            
            _fpsCapHook = _hooks.CreateAsmHook(function, address, AsmHookBehaviour.DoNotExecuteOriginal).Activate();
        });
        
        SigScan("F3 44 0F 5E D0 E8 ?? ?? ?? ?? 48 8B 8C 24 ?? ?? ?? ??", "Tie Game Speed to FPS", address =>
        {
            string[] function =
            {
                "use64",
                "divss xmm10, xmm0" // Original code
            };

            _gameSpeedHook = _hooks.CreateAsmHook(function, address, AsmHookBehaviour.DoNotExecuteOriginal).Activate();

            if (!_configuration.TieGameSpeedToFps)
            {
                _gameSpeedHook.Disable();
            }
        });
    }

    #region Standard Overrides

    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");

        *_maxFps = _configuration.MaxFps;

        if (_configuration.TieGameSpeedToFps)
        {
            _gameSpeedHook.Enable();
        }
        else
        {
            _gameSpeedHook.Disable();
        }
    }

    #endregion

    #region For Exports, Serialization etc.

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod()
    {
    }
#pragma warning restore CS8618

    #endregion
}