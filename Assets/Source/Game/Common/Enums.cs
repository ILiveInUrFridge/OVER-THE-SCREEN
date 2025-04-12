using System;

namespace Game.Common
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
} 