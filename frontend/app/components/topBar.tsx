import { GetUserProfile, SignOut } from "~/api/userState";
import { useNavigate } from "react-router";
import { useEffect, useState } from "react";

export default function TopBar() {
    const navigate = useNavigate();
    const [profile, setProfile] = useState<any>({});

    useEffect(() => {
        GetUserProfile().then(profile => {
            setProfile(profile);
        });
    }, []);

    const onLogOut = async () => {
        await SignOut();
        navigate("/login", { replace: true });
    };

    return (
        <header className="bg-blue-400 text-white px-6 py-4 flex justify-between items-center">
            {/* Username */}
            <div className="font-semibold text-lg">{profile.userName}</div>

            {/* Buttons */}
            <div className="flex gap-2">
                <button
                    onClick={() => navigate("/matchHistory", { replace: true })}
                    className="bg-blue-600 hover:bg-blue-700 transition px-4 py-2 rounded-md font-semibold"
                >
                    Match history
                </button>

                <button
                    onClick={() => navigate("/dashboard", { replace: true })}
                    className="bg-blue-600 hover:bg-blue-700 transition px-4 py-2 rounded-md font-semibold"
                >
                    Dashboard
                </button>

                <button
                    onClick={onLogOut}
                    className="bg-red-600 hover:bg-red-700 transition px-4 py-2 rounded-md font-semibold"
                >
                    Logout
                </button>
            </div>
        </header>
    );
}