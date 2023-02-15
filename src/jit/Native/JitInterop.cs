namespace Vezel.Celerity.Jit.Native;

internal static partial class JitInterop
{
    private const string Library = "Vezel.Celerity.Native";

    static JitInterop()
    {
        Initialize();
    }

    [LibraryImport(Library, EntryPoint = $"celerity_initialize")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void Initialize();
}
