// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Service.Logging;

public abstract class LanguageServiceLoggerProvider
{
    public abstract LanguageServiceLogger CreateLogger(string name);
}
