import { useEffect, useState, useRef } from "react";
import { GetUserProfile, SignOut } from "~/api/userState";
import { useNavigate } from "react-router";
import { onMatchFound, onMatchStarted, onSomeoneAcceptedMatch } from "~/signalRHandlers/matchMakingHubConnection";
import { matchService } from "~/api/axiosInstances";
import MatchFoundModalHandle from "~/components/matchFoundModal";
import MatchFoundModal from "~/components/matchFoundModal";
import CircularCountdown from "~/components/circularCountdown";
import AcceptedPlayersPreview from "~/components/acceptedPlayersPreview";

export default function Dashboard() {
  const navigate = useNavigate();

  const [profile, setProfile] = useState<{ userName?: string }>({});
  const [searchingForMatch, setSearchingForMatch] = useState(false);
  const [elapsedSeconds, setElapsedSeconds] = useState(0);
  const [currentMatchID, setCurrentMatchID] = useState("");
  const [totalAcceptedPlayers, setTotalAcceptedPlayers] = useState(0);

  const modalRef = useRef<MatchFoundModalHandle>(null);
  const audioRef = useRef<HTMLAudioElement | null>(null);

  let handleOnMatchFound = async (matchID: string) => {
    modalRef.current?.show();

    if (audioRef.current) {
      audioRef.current.currentTime = 0; // rewind to start
      audioRef.current.play();
    }

    setCurrentMatchID(matchID)
  }

  let cleanUp = async () => {
    modalRef.current?.hide();
    setSearchingForMatch(false);
    setElapsedSeconds(0);
    setTotalAcceptedPlayers(0);
  }

  let handleMatchStarted = async () => {
    navigate("/game");
    cleanUp();
  }

  let handleMatchWasntAccepted = async () => {
    cleanUp();
  }

  let handleSomeoneAcceptedMatch = async () => {
    console.log("SOMEONE ACCEPTED MATCH")
    setTotalAcceptedPlayers(totalAcceptedPlayers + 1);
  }

  useEffect(() => {
    GetUserProfile().then((profile) => {
      setProfile(profile);
    });
    // Preload the audio once when component mounts
    audioRef.current = new Audio("/sounds/match_found.mp3");
    audioRef.current.load();

    const unsubscribeMatchFound = onMatchFound((matchID: string) => {
      handleOnMatchFound(matchID)
    });

    const unsubscribeMatchStarted = onMatchStarted(() => {
      handleMatchStarted();
    });

    const unsubscribeSomeoneAcceptedMatch = onSomeoneAcceptedMatch(() => {
      handleSomeoneAcceptedMatch();
    })

    return () => {
      unsubscribeMatchFound();
      unsubscribeMatchStarted();
      unsubscribeSomeoneAcceptedMatch();
    }
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

  let onMatchAccepted = async () => {

    console.log(currentMatchID.matchId);
    await matchService.post("/MatchMaking/AcceptMatch", { "matchId": currentMatchID.matchId })
  }

  let onFindMatch = async () => {
    setSearchingForMatch(true);

    await matchService.post("/MatchMaking/JoinQueue");
  };

  let onCancelSearch = async () => {
    setSearchingForMatch(false);

    await matchService.post("/MatchMaking/LeaveQueue");
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

        <MatchFoundModal ref={modalRef}>
          <h2 className="text-xl font-bold mb-4">Match Found!</h2>
          <CircularCountdown
            duration={15}
            size={120}
            color="#44cdefff"
            onComplete={() => handleMatchWasntAccepted()}
          />

          <AcceptedPlayersPreview totalPlayers={2} acceptedCount={totalAcceptedPlayers} />
          <button
            onClick={onMatchAccepted}
            className="bg-indigo-600 hover:bg-red-700 transition px-4 py-2 rounded-md font-semibold mt-4"
          >
            Accept
          </button>
        </MatchFoundModal>
      </main>
    </div>
  );
}