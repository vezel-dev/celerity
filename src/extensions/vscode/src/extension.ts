import {
    Disposable,
    type ExtensionContext,
    MarkdownString,
    StatusBarAlignment,
    window,
    workspace,
} from "vscode";
import {
    LanguageClient,
    TransportKind,
} from "vscode-languageclient/node";

export function activate(context : ExtensionContext) : void {
    const { subscriptions } = context;
    const cfg = workspace.getConfiguration("celerity");

    const extensionChannel = window.createOutputChannel("Celerity", { log : true });
    const serverChannel = window.createOutputChannel("Celerity Language Server");
    const status = window.createStatusBarItem("Celerity", StatusBarAlignment.Left);

    subscriptions.push(new Disposable(() => extensionChannel.dispose()));
    subscriptions.push(new Disposable(() => serverChannel.dispose()));
    subscriptions.push(new Disposable(() => status.dispose()));

    status.show();

    function setStatus(icon : string, tooltip ?: string | undefined) : void {
        status.text = `$(${icon}) Celerity`;

        if (tooltip !== undefined)
            status.tooltip = new MarkdownString(tooltip);
    }

    extensionChannel.info("Launching Celerity language server...");

    setStatus("rocket", "Launching...");

    const client = new LanguageClient(
        "celerity",
        {
            command : cfg.get("executablePath", "celerity"),
            args : ["serve", "-l", cfg.get<string>("logLevel", "information")],
            transport : TransportKind.stdio,
        },
        {
            outputChannel : serverChannel,
            documentSelector : [
                {
                    scheme : "file",
                    language : "celerity",
                },
            ],
        });

    client.start().then(
        () => {
            subscriptions.push(client);

            extensionChannel.info("Celerity language server launched.");

            setStatus("zap", client.initializeResult?.serverInfo?.version);
        },
        err => {
            extensionChannel.error("Celerity language server failed to launch:", err);
            extensionChannel.show(true);

            setStatus("alert", `\`\`\`\n${err}\n\`\`\``);
        });
}
