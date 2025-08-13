import * as signalR from "@microsoft/signalr";
import { gameservice_url } from "~/api/axiosInstances";
import { GetAuthToken } from "~/api/userState";

type StancePlayedCallback = (data : any) => void;
type GameConcludedCallback = (data : any) => void;

const listenersForStancePlayed = new Set<StancePlayedCallback>();
export const onStancePlayed = (callback: StancePlayedCallback): (() => void) => {
    listenersForStancePlayed.add(callback);
    return () => listenersForStancePlayed.delete(callback);
};

const listenersForGameConcludedCallback = new Set<GameConcludedCallback>();
export const onGameConcluded = (callback: GameConcludedCallback): (() => void) => {
    listenersForGameConcludedCallback.add(callback);
    return () => listenersForGameConcludedCallback.delete(callback);
};


let connection;

export const connectToGameServiceHub = async () => {
    const token = await GetAuthToken();

    if (token != null) {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(gameservice_url + "/gameServiceHub", {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        connection.on("StancePlayed", (data) => {
            listenersForStancePlayed.forEach((cb) => cb(data));
        });

        connection.on("GameConcluded", (data) => {
            listenersForGameConcludedCallback.forEach((cb) => cb(data));
        })

        await connection.start();
        console.log("Connected to game service hub");
    }
};