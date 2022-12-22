namespace Runtime;


/// <summary>
/// Global configuration for use in any active feature of the runtime.
/// </summary>
public static class Configuration
{
    static Configuration()
    {
        SFlags = new Dictionary<string, bool>();
        SOptions = new Dictionary<string, IEnumerable<string>>();
        
        // add default directory's to search through for DL modules.
        // these are to be used when searching for a file during an
        // import.
        
        RegisterDefaultOption("module-paths",
            "C:\\Users\\deeto\\source\\repos\\DeeL\\playground\\bin\\STD\\DL"
            );

        RegisterDefaultFlag("debug",
#if DEBUG
    true
#else
          false
#endif
            );
    }
    
    /// <summary>
    /// Flags that specify if an arbitrary thing is enabled or disabled.
    /// </summary>
    private static readonly IDictionary<string, bool> SFlags;
    
    /// <summary>
    /// Add a flag and set it to a default value if it's not already initialized.
    /// </summary>
    /// <param name="setting">The setting name</param>
    /// <param name="value">The default value of the setting</param>
    public static void RegisterDefaultFlag(string setting, bool value)
    {
        // If the value has already been set, leave it. That likely means that 
        // it's value has been set elsewhere.

        if (SFlags.ContainsKey(setting))
            return;
        
        Logger.Info(setting, $"This flag was set to `{value}`");

        SFlags[setting] = value;
    }

    public static void SetFlag(string setting, bool value)
    {
        Logger.Info(setting, $"This flag was set to `{value}`");
        SFlags[setting] = value;
    }
    
    /// <summary>
    /// Get the value for a specified flag. If the flag does not exist, the default value
    /// returned is false. (use <see cref="RegisterDefaultFlag"/> to set explicit default values)
    /// </summary>
    /// <param name="setting">The setting to get the value of</param>
    /// <returns>The settings value if it exists, otherwise false</returns>
    public static bool GetFlag(string setting)
    {
        return SFlags.ContainsKey(setting) && SFlags[setting];
    }

    private static readonly IDictionary<string, IEnumerable<string>> SOptions;

    public static void RegisterDefaultOption(string option, params string[] defaults)
    {
        // Like in RegisterDefaultFlag, do not add defaults if the value
        // is already present.

        if (SFlags.ContainsKey(option))
            return;
        
        Logger.Info(option, $"This flag was set to `[{string.Join(',', defaults)}]`");

        SOptions[option] = defaults;
    }

    public static void AppendToOption(string option, params string[] opts)
    {
        Logger.Info(option, $"Values [{string.Join(',', opts)}] were appended");
        
        if (!SOptions.ContainsKey(option))
            SOptions[option] = opts;
        else
        {
            // scuffed
            var copy = SOptions[option].ToList();
            copy.AddRange(opts);
            SOptions[option] = copy.ToArray();
        }
    }

    public static List<string>? GetOption(string setting)
    {
        return SOptions.ContainsKey(setting)
            ? SOptions[setting].ToList()
            : null;
    }
}