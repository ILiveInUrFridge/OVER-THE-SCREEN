using System;

namespace OTS.Common
{
    /// <summary>
    ///     Represents different moods Purrine can have
    /// </summary>
    public enum Mood
    {
        NEUTRAL,
        HAPPY,
        SAD,
        ANGRY,
        TIRED,
        SICK,
        DEPRESSED,
        ANXIOUS,
        BROKEN
    }
    
    /// <summary>
    ///     Extensions for the Mood enum
    /// </summary>
    public static class MoodExtensions
    {
        public static string GetName(this Mood mood) => mood switch
        {
            Mood.NEUTRAL => "Neutral",
            Mood.HAPPY => "Happy",
            Mood.SAD => "Sad",
            Mood.ANGRY => "Angry",
            Mood.TIRED => "Tired",
            Mood.SICK => "Sick",
            Mood.DEPRESSED => "Depressed",
            Mood.ANXIOUS => "Anxious",
            Mood.BROKEN => "Broken",
            _ => throw new ArgumentOutOfRangeException(nameof(mood))
        };
    }

    /// <summary>
    ///     Represents different GPU models available in the game
    /// </summary>
    public enum GPU
    {
        MEOWTX_1000, // Default GPU
        MEOWTX_2000,
        MEOWTX_3000,
        MEOWTX_4000,
        MEOWTX_5000,
        MEOWTX_V100,
        MEOWTX_A100,
        MEOWTX_H100
    }
    
    /// <summary>
    ///     Extensions for the GPU enum
    /// </summary>
    public static class GPUExtensions
    {
        public static string GetName(this GPU gpu) => gpu switch
        {
            GPU.MEOWTX_1000 => "MeowTX 1000",
            GPU.MEOWTX_2000 => "MeowTX 2000",
            GPU.MEOWTX_3000 => "MeowTX 3000",
            GPU.MEOWTX_4000 => "MeowTX 4000",
            GPU.MEOWTX_5000 => "MeowTX 5000",
            GPU.MEOWTX_V100 => "MeowTX V100",
            GPU.MEOWTX_A100 => "MeowTX A100",
            GPU.MEOWTX_H100 => "MeowTX H100",
            _ => throw new ArgumentOutOfRangeException(nameof(gpu))
        };

        public static int GetPrice(this GPU gpu) => gpu switch
        {
            GPU.MEOWTX_1000 => 1000,
            GPU.MEOWTX_2000 => 2000,
            GPU.MEOWTX_3000 => 3000,
            GPU.MEOWTX_4000 => 4000,
            GPU.MEOWTX_5000 => 5000,
            GPU.MEOWTX_V100 => 6000,
            GPU.MEOWTX_A100 => 7000,
            GPU.MEOWTX_H100 => 8000,
            _ => throw new ArgumentOutOfRangeException(nameof(gpu))
        };
    }

    /// <summary>
    ///     Represents different times of day within a single game day
    /// </summary>
    public enum TimeOfDay
    {
        Morning,    // Start of each day
        Afternoon,  // Mid-day
        Night       // End of day (before progressing to next day)
    }

    /// <summary>
    ///     Extensions for the TimeOfDay enum
    /// </summary>
    public static class TimeOfDayExtensions
    {
        public static string GetName(this TimeOfDay timeOfDay) => timeOfDay switch
        {
            TimeOfDay.Morning => "Morning",
            TimeOfDay.Afternoon => "Afternoon", 
            TimeOfDay.Night => "Night",
            _ => throw new ArgumentOutOfRangeException(nameof(timeOfDay))
        };

        public static TimeOfDay GetNext(this TimeOfDay timeOfDay) => timeOfDay switch
        {
            TimeOfDay.Morning => TimeOfDay.Afternoon,
            TimeOfDay.Afternoon => TimeOfDay.Night,
            TimeOfDay.Night => TimeOfDay.Morning,
            _ => throw new ArgumentOutOfRangeException(nameof(timeOfDay))
        };

    }


    /// <summary>
    ///     Represents different environment layer types
    /// </summary>
    public enum EnvironmentLayerType
    {
        // Core layers (always present)
        Background,
        NoiseOverlay,

        // Post-processing layers (modular)
        Vignette,
        Filter,
        CharacterLighting,

        // Meta layers for custom ordering
        MetaBeforeBackground,
        BackgroundPostProcess,
        MetaAfterBackground,
        MetaAbovePostProcess,
        PostProcessingAboveEverything
    }

    /// <summary>
    ///     Extensions for the EnvironmentLayerType enum
    /// </summary>
    public static class EnvironmentLayerTypeExtensions
    {
        public static string GetName(this EnvironmentLayerType layerType) => layerType switch
        {
            EnvironmentLayerType.Background => "Background",
            EnvironmentLayerType.NoiseOverlay => "Noise Overlay",
            EnvironmentLayerType.Vignette => "Vignette",
            EnvironmentLayerType.Filter => "Filter",
            EnvironmentLayerType.CharacterLighting => "Character Lighting",
            EnvironmentLayerType.MetaBeforeBackground => "Meta Before Background",
            EnvironmentLayerType.BackgroundPostProcess => "Background Post Process",
            EnvironmentLayerType.MetaAfterBackground => "Meta After Background",
            EnvironmentLayerType.MetaAbovePostProcess => "Meta Above Post Process",
            EnvironmentLayerType.PostProcessingAboveEverything => "Post Processing Above Everything",
            _ => throw new ArgumentOutOfRangeException(nameof(layerType))
        };

    }
} 