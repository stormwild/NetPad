﻿import {WithDisposables} from "@common";
import {ViewerHost} from "./viewer-host";

export enum ViewableObjectType {
    Text = "Text",
    Media = "Media"
}

export interface IViewableObjectCommands
{
    open: (viewerHost: ViewerHost) => Promise<void>;
    close: (viewerHost: ViewerHost) => Promise<void>;
    activate: (viewerHost: ViewerHost) => Promise<void>;
    save: () => Promise<void>;
    openContainingFolder: () => Promise<void>;
}

export abstract class ViewableObject extends WithDisposables {
    protected constructor(
        public readonly id: string,
        public readonly type: ViewableObjectType,
        protected readonly commands: IViewableObjectCommands
    ) {
        super();
    }

    abstract get name(): string;

    abstract get isDirty(): boolean;

    public override toString() {
        return `${this.type} [${this.id}] ${this.name}`;
    }

    public open(viewerHost: ViewerHost): Promise<void> {
        return this.commands.open(viewerHost);
    }

    public close(viewerHost: ViewerHost): Promise<void> {
        return this.commands.close(viewerHost);
    }

    public activate(viewerHost: ViewerHost): Promise<void> {
        return this.commands.activate(viewerHost);
    }

    public save(): Promise<void> {
        return this.commands.save();
    }

    public openContainingFolder(): Promise<void> {
        return this.commands.openContainingFolder();
    }
}
