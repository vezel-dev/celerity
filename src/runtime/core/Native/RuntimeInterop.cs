namespace Vezel.Celerity.Runtime.Compiler.Native;

internal static partial class RuntimeInterop
{
    private const string Library = "Vezel.Celerity.Runtime.Native";

    static RuntimeInterop()
    {
        Initialize();
    }

    [LibraryImport(Library, EntryPoint = "celerity_initialize")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void Initialize();
}
