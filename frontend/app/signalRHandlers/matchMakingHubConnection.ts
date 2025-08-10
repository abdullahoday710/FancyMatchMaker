// matchmakingHubClient.js
import * as signalR from "@microsoft/signalr";
import { matchmaking_url } from "~/api/axiosInstances";
import { GetAuthToken } from "~/api/userState";

type MatchFoundCallback = (matchID: string) => void;
type MatchStartedCallback = () => void;
type PlayerAcceptedMatchCallback = () => void;

const listenersForMatchFound = new Set<MatchFoundCallback>();

export const onMatchFound = (callback: MatchFoundCallback): (() => void) => {
    listenersForMatchFound.add(callback);
    return () => listenersForMatchFound.delete(callback);
};

const listenersForMatchStarted = new Set<MatchStartedCallback>();
export const onMatchStarted = (callback: MatchStartedCallback): (() => void) => {
    listenersForMatchStarted.add(callback);
    return () => listenersForMatchStarted.delete(callback);
};

const listenersForSomeoneAcceptedMatch = new Set<PlayerAcceptedMatchCallback>();
export const onSomeoneAcceptedMatch = (callback: PlayerAcceptedMatchCallback): (() => void) => {
    listenersForSomeoneAcceptedMatch.add(callback);
    return () => listenersForSomeoneAcceptedMatch.delete(callback);
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
            listenersForMatchFound.forEach((cb) => cb(data));
        });

        connection.on("SomeoneAcceptedMatch", () => {
            console.log("someone accepted ?")
            listenersForSomeoneAcceptedMatch.forEach((cb) => cb());
        })

        
        connection.on("MatchStarted", () => {
            listenersForMatchStarted.forEach((cb) => cb());
        })

        await connection.start();
        console.log("Connected to matchmaking hub");
    }
};