import { useEffect, useState } from "react";
import { GetUserProfile, SignOut } from "~/api/userState";
import { useNavigate } from "react-router";

export default function Dashboard() {
  const navigate = useNavigate();

  const [profile, setProfile] = useState<{ userName?: string }>({});
  const [searchingForMatch, setSearchingForMatch] = useState(false);
  const [elapsedSeconds, setElapsedSeconds] = useState(0);

  useEffect(() => {
    GetUserProfile().then((profile) => {
      setProfile(profile);
    });
  }, []);

  // Timer effect: runs only when searchingForMatch is true
  useEffect(() => {
    if (!searchingForMatch) {
      setElapsedSeconds(0);
      return;
    }

    const interval = setInterval(() => {
      setElapsedSeconds((seconds) => seconds + 1);
    }, 1000);

    return () => clearInterval(interval); // cleanup on stop searching or unmount
  }, [searchingForMatch]);

  let onLogOut = async () => {
    await SignOut();
    navigate("/login", { replace: true });
  };

  let onFindMatch = async () => {
    setSearchingForMatch(true);
  };

  let onCancelSearch = async () => {
    setSearchingForMatch(false);
  };

  // Format seconds to mm:ss
  function formatTime(seconds: number) {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${String(mins).padStart(2, "0")}:${String(secs).padStart(2, "0")}`;
  }

  let RenderQueueControlButtons = () => {
    if (searchingForMatch) {
      return (
        <div className="flex flex-col items-center gap-4">
          <button
            onClick={() => {
              onCancelSearch();
            }}
            className="bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-3 px-6 rounded-md transition"
          >
            Cancel search
          </button>
          <div className="text-gray-700 font-medium text-lg">
            Searching for match... {formatTime(elapsedSeconds)}
          </div>
        </div>
      );
    } else {
      return (
        <button
          onClick={() => {
            onFindMatch();
          }}
          className="bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-3 px-6 rounded-md transition"
        >
          Find match
        </button>
      );
    }
  };

  return (
    <div className="min-h-screen flex flex-col">
      {/* Top Bar */}
      <header className="bg-blue-400 text-white px-6 py-4 flex justify-between items-center">
        <div className="font-semibold text-lg">{profile.userName}</div>
        <button
          onClick={() => {
            onLogOut();
          }}
          className="bg-red-600 hover:bg-red-700 transition px-4 py-2 rounded-md font-semibold"
        >
          Logout
        </button>
      </header>

      {/* Main Content */}
      <main className="flex-grow flex items-center justify-center bg-gray-100 p-4">
        <div className="bg-white shadow-md rounded-lg p-8 w-full max-w-sm flex justify-center">
          {RenderQueueControlButtons()}
        </div>
      </main>
    </div>
  );
}