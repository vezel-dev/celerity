// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Service.Logging;

public abstract class LanguageServiceLogger
{
    public abstract void Log(LogLevel logLevel, string eventName, string message, Exception? exception);
}
