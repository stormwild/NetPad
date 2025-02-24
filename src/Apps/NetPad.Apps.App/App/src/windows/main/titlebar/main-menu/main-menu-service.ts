import {DI, IContainer} from "aurelia";
import {IMenuItem} from "./imenu-item";
import {System} from "@common";
import {ISettingService, IWindowService} from "@domain";
import {IShortcutManager} from "@application";
import {ITextEditorService} from "@application/editor/text-editor-service";
import {AppUpdateDialog} from "@application/dialogs/app-update-dialog/app-update-dialog";
import {IDialogService} from "@aurelia/dialog";
import {DialogBase} from "@application/dialogs/dialog-base";

export const IMainMenuService = DI.createInterface<MainMenuService>();

export interface IMainMenuService {
    items: ReadonlyArray<IMenuItem>;
    addItem(item: IMenuItem);
    removeItem(item: IMenuItem);
}

export class MainMenuService implements IMainMenuService {
    private readonly _items: IMenuItem[] = [];

    constructor(
        @ISettingService private readonly settingsService: ISettingService,
        @IShortcutManager private readonly shortcutManager: IShortcutManager,
        @ITextEditorService private readonly textEditorService: ITextEditorService,
        @IWindowService private readonly windowService: IWindowService,
        @IContainer private readonly container: IContainer,
    ) {
        this._items = [
            {
                text: "File",
                menuItems: [
                    {
                        text: "New",
                        icon: "add-script-icon",
                        shortcut: this.shortcutManager.getShortcutByName("New"),
                    },
                    {
                        isDivider: true
                    },
                    {
                        text: "Save",
                        icon: "save-icon",
                        shortcut: this.shortcutManager.getShortcutByName("Save"),
                    },
                    {
                        text: "Save All",
                        icon: "save-icon",
                        shortcut: this.shortcutManager.getShortcutByName("Save All"),
                    },
                    {
                        text: "Properties",
                        icon: "properties-icon",
                        shortcut: this.shortcutManager.getShortcutByName("Script Properties"),
                    },
                    {
                        text: "Close",
                        icon: "close-icon",
                        shortcut: this.shortcutManager.getShortcutByName("Close"),
                    },
                    {
                        isDivider: true
                    },
                    {
                        text: "Settings",
                        icon: "settings-icon",
                        shortcut: this.shortcutManager.getShortcutByName("Settings"),
                    },
                    {
                        text: "Exit",
                        click: async () => window.close()
                    }
                ]
            },
            {
                text: "Edit",
                menuItems: [
                    {
                        text: "Undo",
                        icon: "undo-icon",
                        click: async () => this.textEditorService.active?.monaco
                            .trigger(null, "undo", null),
                        helpText: "Ctrl + Z"
                    },
                    {
                        text: "Redo",
                        icon: "redo-icon",
                        click: async () => this.textEditorService.active?.monaco
                            .trigger(null, "redo", null),
                        helpText: "Ctrl + Shift + Z"
                    },
                    {
                        isDivider: true
                    },
                    {
                        text: "Cut",
                        icon: "cut-icon",
                        click: async () => this.textEditorService.active?.monaco
                            .trigger(null, "editor.action.clipboardCutAction", null)
                    },
                    {
                        text: "Copy",
                        icon: "copy-icon",
                        click: async () => this.textEditorService.active?.monaco
                            .trigger(null, "editor.action.clipboardCopyAction", null)
                    },
                    {
                        text: "Delete",
                        icon: "backspace-icon",
                        click: async () => this.textEditorService.active?.monaco
                            .trigger(null, "deleteRight", null)
                    },
                    {
                        text: "Select All",
                        click: async () => this.textEditorService.active?.monaco
                            .trigger(null, "editor.action.selectAll", null)
                    },
                    {
                        isDivider: true
                    },
                    {
                        text: "Find",
                        icon: "search-icon",
                        click: async () => this.textEditorService.active?.monaco
                            .trigger(null, "actions.findWithSelection", null),
                        helpText: "Ctrl + F"
                    },
                    {
                        text: "Replace",
                        click: async () => this.textEditorService.active?.monaco
                            .trigger(null, "editor.action.startFindReplaceAction", null),
                        helpText: "Ctrl + H"
                    },
                    {
                        isDivider: true
                    },
                    {
                        text: "Transform to Upper Case",
                        click: async () => this.textEditorService.active?.monaco
                            .trigger(null, "editor.action.transformToUppercase", null)
                    },
                    {
                        text: "Transform to Lower Case",
                        click: async () => this.textEditorService.active?.monaco
                            .trigger(null, "editor.action.transformToLowercase", null)
                    },
                    {
                        text: "Transform to Title Case",
                        click: async () => this.textEditorService.active?.monaco
                            .trigger(null, "editor.action.transformToTitlecase", null)
                    },
                    {
                        text: "Transform to Kebab Case",
                        click: async () => this.textEditorService.active?.monaco
                            .trigger(null, "editor.action.transformToKebabcase", null)
                    },
                    {
                        text: "Transform to Snake Case",
                        click: async () => this.textEditorService.active?.monaco
                            .trigger(null, "editor.action.transformToSnakecase", null)
                    },
                    {
                        isDivider: true
                    },
                    {
                        text: "Toggle Line Comment",
                        click: async () => this.textEditorService.active?.monaco
                            .trigger(null, "editor.action.commentLine", null),
                        helpText: "Ctrl + /"
                    },
                    {
                        text: "Toggle Block Comment",
                        click: async () => this.textEditorService.active?.monaco
                            .trigger(null, "editor.action.blockComment", null),
                        helpText: "Ctrl + Shift + A"
                    },
                ]
            },
            {
                text: "View",
                menuItems: [
                    {
                        text: "Reload",
                        click: async () => window.location.reload(),
                        helpText: "Ctrl + R"
                    },
                    {
                        text: "Open Developer Tools",
                        click: async() => this.windowService.openDeveloperTools(),
                        helpText: "Ctrl + Shift + I",
                    },
                    {
                        isDivider: true
                    },
                    {
                        text: "Zoom In",
                        icon: "zoom-in-icon",
                        helpText: "Ctrl + +",
                    },
                    {
                        text: "Zoom Out",
                        icon: "zoom-out-icon",
                        helpText: "Ctrl + -",
                    },
                    {
                        text: "Reset Zoom",
                        helpText: "Ctrl + 0",
                    },
                ]
            },
            {
                text: "Help",
                menuItems: [
                    {
                        text: "About",
                        icon: "star-icon",
                        click: async () => await this.settingsService.openSettingsWindow("about")
                    },
                    {
                        text: "Check for Updates",
                        icon: "app-update-icon",
                        click: async () => await DialogBase.toggle(this.container.get(IDialogService), AppUpdateDialog)
                    },
                    {
                        text: "GitHub",
                        icon: "github-icon",
                        click: async () => System.openUrlInBrowser("https://github.com/tareqimbasher/NetPad")
                    },
                    {
                        text: "Search Issues",
                        icon: "github-icon",
                        click: async () => System.openUrlInBrowser("https://github.com/tareqimbasher/NetPad/issues")
                    },
                ]
            }
        ];
    }

    public get items(): ReadonlyArray<IMenuItem> {
        return this._items;
    }

    public addItem(item: IMenuItem) {
        this._items.push(item);
    }

    public removeItem(item: IMenuItem) {
        const ix = this._items.indexOf(item);
        if (ix >= 0) this._items.splice(ix, 1);
    }
}
