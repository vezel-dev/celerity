// SPDX-License-Identifier: 0BSD

public sealed partial class SemanticTests
{
    [Fact]
    public Task loop_control()
    {
        return TestAsync(
            """
            mod {
                fn loops() {
                    break;

                    next;

                    while true {
                        break;

                        break as 1;

                        next;
                    }

                    while break {
                        2;
                    }

                    while next {
                        3;
                    };

                    for _ in [] {
                        break;

                        break as 4;

                        next;
                    }

                    for _ in break {
                        5;
                    };

                    for _ in next {
                        6;
                    };

                    while true {
                        fn() -> {
                            break;

                            break as 7;

                            next;
                        };
                    };
                }
            }
            """);
    }
}
