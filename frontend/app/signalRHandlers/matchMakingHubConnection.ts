// matchmakingHubClient.js
import * as signalR from "@microsoft/signalr";
import { matchmaking_url } from "~/api/axiosInstances";
import { GetAuthToken } from "~/api/userState";

type MatchFoundCallback = (matchID: string) => void;

const listeners = new Set<MatchFoundCallback>();

export const onMatchFound = (callback: MatchFoundCallback): (() => void) => {
    listeners.add(callback);
    return () => listeners.delete(callback);
};

let connection;

export const connectToMatchMakingHub = async () => {
    const token = await GetAuthToken();

    if (token != null) {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(matchmaking_url + "/matchmakingHub", {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        connection.on("MatchFound", (data) => {
            listeners.forEach((cb) => cb(data));
        });

        await connection.start();
        console.log("Connected to matchmaking hub");
    }
};