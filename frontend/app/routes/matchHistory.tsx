import { useEffect, useState } from "react";
import { authService, gameService } from "~/api/axiosInstances";
import { GetUserProfile } from "~/api/userState";
import TopBar from "~/components/topBar";

export default function MatchHistoryListPage() {
    const [matches, setMatches] = useState<any[]>([]);
    const [playerNames, setPlayerNames] = useState<any>(null);
    const [profile, setProfile] = useState<any>(null);

    function getUniquePlayerIds(arr: any) {
        const allIds = arr.flatMap(match => match.participatingPlayers.concat(match.winnerID ? [match.winnerID] : []));
        return [...new Set(allIds)];
    }

    let getMatchHistory = async () => {
        var allMatches = await gameService.get("/MatchHistory/GetMatchHistory")
        setMatches(allMatches.data)
        var playerIDs = getUniquePlayerIds(allMatches.data);
        console.log(playerIDs)
        var playerNames = await authService.post("/GetProfileNames", { userIDs: playerIDs })
        console.log(playerNames.data)
        setPlayerNames(playerNames.data);
    }
    useEffect(() => {
        GetUserProfile().then((profile) => { setProfile(profile) })
        getMatchHistory();
    }, [])

    let renderMatchWinner = (match: any) => {
        return (
            match.winnerID == profile.userID ? <span>(You)</span> : <span>{playerNames[match.winnerID]}</span>
        )
    }

    let renderOpponentName = (match: any) => {
        return (
            <>
                {match.participatingPlayers[0] !== profile.userID && (
                    <span>{playerNames[match.participatingPlayers[0]]}</span>
                )}
                {match.participatingPlayers[1] !== profile.userID && (
                    <span>{playerNames[match.participatingPlayers[1]]}</span>
                )}
            </>
        );
    };

    return (
        <div className="min-h-screen flex flex-col">
            <TopBar />
            <div className="min-h-screen bg-gray-100 p-6">

                <h1 className="text-2xl font-bold mb-6 text-center">Match History</h1>

                <div className="max-w-2xl mx-auto space-y-4">
                    {matches.length === 0 || playerNames == null ? (
                        <p className="text-gray-500 text-center">No matches found.</p>
                    ) : (
                        matches.map((match) => (
                            <div
                                key={match.matchUUID}
                                className="bg-white rounded-xl shadow-md p-4 hover:shadow-lg transition"
                            >
                                <p className="text-sm text-gray-500">
                                    <span className="font-semibold">Match UUID:</span> {match.matchUUID}
                                </p>
                                <div className="mt-2 grid grid-cols-2 gap-2">
                                    <p>
                                        <span className="font-semibold">You against :</span> {renderOpponentName(match)}
                                    </p>
                                </div>
                                <p className="mt-2">
                                    <span className="font-semibold text-green-600">Winner:</span> {renderMatchWinner(match)}
                                </p>
                            </div>
                        ))
                    )}
                </div>
            </div>
        </div>
    );
}