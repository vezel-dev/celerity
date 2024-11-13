// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Runtime.Compiler.Native;

internal static partial class RuntimeInterop
{
    private const string Library = "Vezel.Celerity.Runtime.Native";

    static RuntimeInterop()
    {
        NativeLibrary.SetDllImportResolver(
            typeof(ThisAssembly).Assembly,
            static (name, asm, paths) =>
            {
                // First try the normal search algorithm that takes into account the application's configuration and
                // static dependency information.
                if (NativeLibrary.TryLoad(name, asm, paths, out var handle))
                    return handle;

                // If someone is trying to load some unknown library through our assembly, at this point, there is
                // nothing more that we can do.
                if (name != Library)
                    return 0;

                // It is now likely that someone is trying to use Celerity without static dependency information, so the
                // runtime has no idea how to find Vezel.Celerity.Runtime.Native. In this case, it is likely to either
                // sit right next to Vezel.Celerity.Runtime.dll, or in runtimes/<rid>/native.

                var directory = AppContext.BaseDirectory;
                var fileName = OperatingSystem.IsWindows()
                    ? $"{name}.dll"
                    : OperatingSystem.IsMacOS()
                        ? $"lib{name}.dylib"
                        : $"lib{name}.so";

                bool TryLoad(out nint handle, params ReadOnlySpan<string> paths)
                {
                    return NativeLibrary.TryLoad(Path.Combine([directory, .. paths, fileName]), out handle);
                }

                return TryLoad(out handle)
                    ? handle
                    : TryLoad(out handle, "runtimes", RuntimeInformation.RuntimeIdentifier, "native")
                        ? handle
                        : 0;
            });
    }

    [LibraryImport(Library, EntryPoint = "celerity_initialize")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void Initialize();
}
