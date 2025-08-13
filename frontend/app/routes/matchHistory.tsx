import { useEffect, useState } from "react";
import { gameService } from "~/api/axiosInstances";

export default function MatchHistoryListPage() {
    const [matches, setMatches] = useState<any[]>([]);

    let getMatchHistory = async () => {
        var allMatches = await gameService.get("/MatchHistory/GetMatchHistory")
        console.log(allMatches.data);
        setMatches(allMatches.data)
    }
    useEffect(() => {
        getMatchHistory();
    }, [])

    return (
        <div className="min-h-screen bg-gray-100 p-6">
            <h1 className="text-2xl font-bold mb-6 text-center">Match History</h1>

            <div className="max-w-2xl mx-auto space-y-4">
                {matches.length === 0 ? (
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
                                    <span className="font-semibold">Player 1:</span> {}
                                </p>
                                <p>
                                    <span className="font-semibold">Player 2:</span> {}
                                </p>
                            </div>
                            <p className="mt-2">
                                <span className="font-semibold text-green-600">Winner:</span> {}
                            </p>
                        </div>
                    ))
                )}
            </div>
        </div>
    );
}