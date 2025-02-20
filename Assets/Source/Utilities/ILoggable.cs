using UnityEngine;
using System.Diagnostics;

/// <summary>
///     Interface to provide logging capabilities to components.
///     Implement this interface to add logging functionality to any class.
/// </summary>
public interface ILoggable
{
    /// <summary>
    ///     Gets the component name for logging.
    ///     By default, returns the actual type name of the implementing class.
    /// </summary>
    string LogComponent => GetType().Name;
}

/// <summary>
///     Extension methods to provide logging functionality similar to traits
/// </summary>
public static class LoggableExtensions
{
    /// <summary>
    ///     Logs debug messages with component context only when in Unity Editor.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public static void Log(this ILoggable self, string message)
    {
        UnityEngine.Debug.Log($"[{self.LogComponent}] {message}");
    }

    /// <summary>
    ///     Logs warning messages with component context only when in Unity Editor.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public static void LogWarning(this ILoggable self, string message)
    {
        UnityEngine.Debug.LogWarning($"[{self.LogComponent}] {message}");
    }

    /// <summary>
    ///     Logs error messages with component context only when in Unity Editor.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public static void LogError(this ILoggable self, string message)
    {
        UnityEngine.Debug.LogError($"[{self.LogComponent}] {message}");
    }
}