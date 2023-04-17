import {
    Disposable,
    type ExtensionContext,
    MarkdownString,
    StatusBarAlignment,
    commands,
    window,
    workspace,
} from "vscode";
import {
    LanguageClient,
    TransportKind,
} from "vscode-languageclient/node";

export async function activate(context : ExtensionContext) : Promise<void> {
    const cfg = workspace.getConfiguration("celerity");
    const extensionChannel = window.createOutputChannel("Celerity", { log : true });
    const serverChannel = window.createOutputChannel("Celerity Language Server");
    const status = window.createStatusBarItem("Celerity", StatusBarAlignment.Left);

    let client : LanguageClient | null = null;

    function setStatus(icon : string, tooltip ?: string | undefined) : void {
        status.text = `$(${icon}) Celerity`;

        if (tooltip !== undefined)
            status.tooltip = new MarkdownString(tooltip);
    }

    function pushDisposable(disposable : { dispose : () => unknown }) {
        context.subscriptions.push(disposable);
    }

    function registerCommand(name : string, callback : () => unknown) : void {
        pushDisposable(commands.registerCommand(name, callback));
    }

    pushDisposable(extensionChannel);
    pushDisposable(serverChannel);
    pushDisposable(status);

    registerCommand(
        "celerity.startServer",
        async () => {
            if (client !== null)
                return;

            extensionChannel.info("Starting Celerity language server...");
            setStatus("rocket", "Starting...");

            client = new LanguageClient(
                "celerity",
                {
                    command : cfg.get("executablePath", "celerity"),
                    args : ["serve", "-l", cfg.get<string>("serverLogLevel", "information")],
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

            try {
                await client.start();

                extensionChannel.info("Celerity language server started.");
                setStatus("zap", client.initializeResult?.serverInfo?.version);
            } catch (err) {
                extensionChannel.show(true);

                extensionChannel.error("Celerity language server failed to start:", err);
                setStatus("alert", `\`\`\`\n${err}\n\`\`\``);
            }
        });

    registerCommand("celerity.stopServer", () => {
        if (client === null)
            return;

        extensionChannel.info("Stopping Celerity language server...");
        setStatus("history", "Stopping...");

        void client.dispose();

        client = null;

        extensionChannel.info("Celerity language server stopped.");
        setStatus("circle-slash", "Language server manually stopped.");
    });

    registerCommand("celerity.restartServer", async () => {
        await commands.executeCommand("celerity.stopServer");
        await commands.executeCommand("celerity.startServer");
    });

    status.show();

    pushDisposable(new Disposable(async () => client?.dispose()));

    if (cfg.get<boolean>("autoStartServer", true))
        await commands.executeCommand("celerity.startServer");
    else
        setStatus("circle-slash", "Language server not started due to `celerity.autoStartServer` setting.");
}
