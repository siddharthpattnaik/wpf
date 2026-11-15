// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MS.Internal.Text.TextInterface;

internal static class ModuleInitializer
{
    /// <summary>
    /// DirectWriteForwarder has a module constructor that implements
    /// the setting of the default DPI awareness for the process.
    /// We need to load DirectWriteForwarder the instant PresentationCore
    /// loads in order to ensure that this is set before any DPI sensitive
    /// operations are carried out.  To do this, we simply call LoadDwrite
    /// as the module constructor for DirectWriteForwarder would do this anyway.
    /// </summary>
#pragma warning disable CA2255
    [ModuleInitializer]
    public static void Initialize()
    {
        IsProcessDpiAware();

        // UPF: DirectWrite is never loaded - NativeLibrary.Load(name, assembly, path) bypasses the UPF
        // DllImportResolver, so this would load the real dwrite.dll on Windows (no-fallback rule) and throw
        // on Linux/macOS. Text is Upf.Text's (Phase 3.1); the DWrite factory pointer stays null and
        // Factory.Initialize fails in Upf.Text's DWriteTypeConverter before it could be called.
        // DWriteLoader.LoadDWrite();
        // AppDomain.CurrentDomain.ProcessExit += static (object sender, EventArgs e) => DWriteLoader.UnloadDWrite();

        MS.Internal.NativeWPFDLLLoader.LoadDwrite();
    }
#pragma warning restore CA2255

    private static void IsProcessDpiAware()
    {
        bool disableDpiAware = false;

        // By default, Application is DPIAware.
        Assembly assemblyApp = Assembly.GetEntryAssembly();

        // Check if the Application has explicitly set DisableDpiAwareness attribute.
        if (assemblyApp != null && Attribute.IsDefined(assemblyApp, typeof(System.Windows.Media.DisableDpiAwarenessAttribute)))
        {
            disableDpiAware = true;
        }

        if (!disableDpiAware)
        {
            // DpiAware composition is enabled for this application.
            SetProcessDPIAware_Internal();
        }

        // Only when DisableDpiAwareness attribute is set in Application assembly,
        // It will ignore the SetProcessDPIAware API call.
    }

    [DllImport("user32.dll", EntryPoint = "SetProcessDPIAware")]
    private static extern void SetProcessDPIAware_Internal();
}
