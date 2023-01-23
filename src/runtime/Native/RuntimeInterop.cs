namespace Vezel.Celerity.Runtime.Native;

internal static partial class RuntimeInterop
{
    private const string Library = "Vezel.Celerity.Runtime.Native";

    private const string Prefix = "celerity_";

    static RuntimeInterop()
    {
        Initialize();
    }

    [LibraryImport(Library, EntryPoint = $"{Prefix}initialize")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void Initialize();
}
