import type * as vscode from "vscode";

export function activate(_context : vscode.ExtensionContext) : void {
    console.log("Celerity extension activated.");
}

export function deactivate() : void {
    console.log("Celerity extension deactivated.");
}
