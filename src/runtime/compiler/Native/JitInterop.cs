namespace Vezel.Celerity.Runtime.Compiler.Native;

internal static partial class JitInterop
{
    private const string Library = "Vezel.Celerity.Runtime.Native";

    static JitInterop()
    {
        Initialize();
    }

    [LibraryImport(Library, EntryPoint = $"celerity_initialize")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void Initialize();
}
