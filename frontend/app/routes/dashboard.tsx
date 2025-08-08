import { useEffect, useState } from "react"
import { GetUserProfile, SignOut } from "~/api/userState";
import { useNavigate } from "react-router";

export default function Dashboard()
{
    const navigate = useNavigate();
    const [profile, setProfile] = useState({});

    useEffect(() => {
        GetUserProfile().then((profile) => {setProfile(profile); console.log(profile)});
    }, [])

    let onLogOut = async () =>
    {
        await SignOut();
        navigate("/login", {replace: true});
    }

    let onFindMatch = async () =>
    {

    }

return (
    <div className="min-h-screen flex flex-col">
        {/* Top Bar */}
        <header className="bg-blue-400 text-white px-6 py-4 flex justify-between items-center">
            <div className="font-semibold text-lg">{profile.userName}</div>
            <button
                onClick={() => {onLogOut()}}
                className="bg-red-600 hover:bg-red-700 transition px-4 py-2 rounded-md font-semibold"
            >
                Logout
            </button>
        </header>

        {/* Main Content */}
        <main className="flex-grow flex items-center justify-center bg-gray-100 p-4">
            <div className="bg-white shadow-md rounded-lg p-8 w-full max-w-sm flex justify-center">
                <button
                    onClick={() => {onFindMatch()}}
                    className="bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-3 px-6 rounded-md transition"
                >
                    Find match
                </button>
            </div>
        </main>
    </div>
    )
}