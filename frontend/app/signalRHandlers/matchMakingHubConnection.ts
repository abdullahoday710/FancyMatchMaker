import * as signalR from "@microsoft/signalr";
import { matchmaking_url } from "~/api/axiosInstances";
import { GetAuthToken } from "~/api/userState";

export const connectToMatchMakingHub = async () =>
{
    const token = await GetAuthToken();

    if (token != null)
    {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl(matchmaking_url + "/matchmakingHub", {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

            connection.on("MatchFound", (data) => {
    console.log("Match found!", data);
    // You can update UI or trigger other logic here
});

        connection.start()
            .then(() => console.log("Connected to matchmaking hub"))
            .catch(err => console.error(err));
    }
}

