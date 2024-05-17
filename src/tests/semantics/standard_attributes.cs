// SPDX-License-Identifier: 0BSD

public sealed partial class SemanticTests
{
    [Fact]
    public Task standard_attributes()
    {
        return TestAsync(
            """
            @deprecated "broken"
            @lint "foo:bar"
            @lint "foo:none"
            mod {
                @doc "asdf"
                use X = Y;

                @doc 42
                @deprecated "very broken"
                fn a() { 1; }

                @doc false
                fn b() { 2; }

                @doc "text"
                @doc "duplicate"
                fn c() { 3; }

                @ignore "bad test"
                @flaky true
                @deprecated false
                test stuff {
                    assert false;
                }
            }
            """);
    }
}
