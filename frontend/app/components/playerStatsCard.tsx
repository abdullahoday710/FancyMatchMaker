import { useEffect, useState } from "react";
import { matchService } from "~/api/axiosInstances";

export default function PlayerStatsCard() {

    const [matchMakingProfile, setMatchMakingProfile] = useState<any>({});
    const [isLoading, setIsLoading] = useState(false);

    let getMatchMakingProfileData = async () =>
    {
        var resp = await matchService.get("/MatchMaking/MyProfile");
        setMatchMakingProfile(resp.data);

        console.log(resp.data);
    }

    useEffect(() => {
        getMatchMakingProfileData();
    }, [])

    return (
        <div className="flex justify-center items-center min-h-screen bg-gray-100">
            <div className="bg-white rounded-2xl shadow-xl p-8 w-80">
                <h2 className="text-2xl font-bold text-center mb-6">Player Stats</h2>
                <div className="space-y-4 text-center">
                    <div>
                        <p className="text-gray-500 text-sm">Matches Played</p>
                        <p className="text-xl font-semibold">{matchMakingProfile.matchesPlayed}</p>
                    </div>
                    <div>
                        <p className="text-gray-500 text-sm">Matches Won</p>
                        <p className="text-xl font-semibold text-green-600">{matchMakingProfile.matchesWon}</p>
                    </div>
                    <div>
                        <p className="text-gray-500 text-sm">Matches Lost</p>
                        <p className="text-xl font-semibold text-red-600">{matchMakingProfile.matchesLost}</p>
                    </div>
                    <div>
                        <p className="text-gray-500 text-sm">Matches Tied</p>
                        <p className="text-xl font-semibold text-yellow-600">{matchMakingProfile.matchesTied}</p>
                    </div>
                </div>
            </div>
        </div>
    );
}