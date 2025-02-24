import {DI} from "aurelia";
import {IDisposable} from "@common";

export interface IIpcGateway extends IDisposable {
    start(): Promise<void>;

    subscribe(channelName: string, callback: (message: unknown, channel: string) => void): IDisposable;

    send<TResult>(channelName: string, ...params: unknown[]): Promise<TResult>;
}

export const IIpcGateway = DI.createInterface<IIpcGateway>();
