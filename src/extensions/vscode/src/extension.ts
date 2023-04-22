import {
    type CancellationToken,
    Disposable,
    type ExtensionContext,
    MarkdownString,
    ShellExecution,
    StatusBarAlignment,
    Task,
    TaskGroup,
    type TaskProvider,
    type Uri,
    commands,
    tasks,
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

    pushDisposable(new Disposable(async () => client?.dispose()));

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
            } catch (err) {
                extensionChannel.show(true);

                extensionChannel.error("Celerity language server failed to start:", err);
                setStatus("alert", `\`\`\`\n${err}\n\`\`\``);

                return;
            }

            extensionChannel.info("Celerity language server started.");
            setStatus("zap", client.initializeResult?.serverInfo?.version);
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

    if (cfg.get<boolean>("autoDetectTasks", true))
        pushDisposable(tasks.registerTaskProvider("celerity", new CelerityTaskProvider()));

    status.show();

    if (cfg.get<boolean>("autoStartServer", true))
        await commands.executeCommand("celerity.startServer");
    else
        setStatus("circle-slash", "Language server not started due to `celerity.autoStartServer` setting.");
}

class CelerityTaskProvider implements TaskProvider {
    private readonly _tasks : Map<string, Task[]> = new Map<string, Task[]>();

    public constructor() {
        const glob = "**/celerity.json";

        void workspace.findFiles(glob).then(uris => {
            for (const uri of uris)
                this.addTasks(uri);
        });

        const watcher = workspace.createFileSystemWatcher(glob);

        watcher.onDidCreate(uri => this.addTasks(uri));
        watcher.onDidDelete(uri => this._tasks.delete(uri.fsPath));
    }

    public provideTasks(_token : CancellationToken) : Task[] {
        return [... this._tasks.values()].flat();
    }

    // eslint-disable-next-line class-methods-use-this
    public resolveTask(_task : Task, _token : CancellationToken) : undefined {
        return undefined;
    }

    private addTasks(uri : Uri) : void {
        const folder = workspace.getWorkspaceFolder(uri);

        if (folder === undefined)
            return;

        const exe = workspace.getConfiguration("celerity").get<string>("executablePath", "celerity");

        this._tasks.set(
            uri.fsPath,
            ["Check", "Format", "Test"].map(kind => {
                const task = new Task(
                    { type : "celerity" },
                    folder,
                    `${kind} ${workspace.asRelativePath(uri)}`,
                    "Celerity",
                    new ShellExecution(`${exe} ${kind.toLowerCase()}`, kind === "Format" ? ["-f"] : []),
                    "$celerity");

                switch (kind) {
                    case "Check":
                        task.group = TaskGroup.Build;
                        break;
                    case "Test":
                        task.group = TaskGroup.Test;
                        break;
                }

                return task;
            }));
    }
}
