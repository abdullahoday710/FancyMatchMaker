import { useState, useEffect } from "react";
import { gameService } from "~/api/axiosInstances";
import { GetUserProfile } from "~/api/userState";
import { onGameConcluded, onStancePlayed } from "~/signalRHandlers/gameServiceHubConnection";
import { useNavigate } from "react-router";

export default function GamePlay() {
    const [playerChoice, setPlayerChoice] = useState(null);
    const [opponentChoice, setOpponentChoice] = useState(null);
    const [result, setResult] = useState(null);
    const [waiting, setWaiting] = useState(false);
    const [otherPlayerMadeMove, setOtherPlayerMadeMove] = useState(false);
    const navigate = useNavigate();

    function getChoiceById(id : any) {
  return choices.find(choice => choice.id === id) || null;
}

    let handleStancePlayed = async (data: any) => {
        var issuer_player = data.player;
        console.log(data)
        var profile = await GetUserProfile();

        if (profile != null) {
            if (issuer_player != profile.userID) {
                setOtherPlayerMadeMove(true);
            }
        }

    }

    let handleConcludeMatch = async (data : any) =>
    {
        var profile = await GetUserProfile();

        if (data.winner == null)
        {
            setResult("It is a tie. 🤝")
        }
        else
        {
            if (profile.userID == data.winner)
            {
                setResult("You win! 🎉");
            }
            else
            {
                setResult("You lose! 😢");
            }
        }

        console.log(data);

        data.playerStates.map((playerState : any) => {
            if(playerState.playerID != profile.userID)
            {
                let opp_choice = getChoiceById(playerState.chosenStance);
                setOpponentChoice(opp_choice);
            }
        })

        setWaiting(false);
    }

    useEffect(() => {
        const unsubscribeStancePlayed = onStancePlayed((data: any) => {
            handleStancePlayed(data);
        });

        const unsubscribeMatchConcluded = onGameConcluded((data: any) => {
            handleConcludeMatch(data);
        });

        return () => {
            unsubscribeStancePlayed();
            unsubscribeMatchConcluded();
        }
    }, [])

    const choices = [
        { name: "Rock", icon: "✊", id: 1 },
        { name: "Paper", icon: "✋", id: 2 },
        { name: "Scissors", icon: "✌️", id: 3 },
    ]

    async function handleChoice(choice: any) {
        setPlayerChoice(choice);
        setWaiting(true);
        let resp = await gameService.post("/Game/SetStance", { stance: choice.id })
    }

    return (
        <div style={styles.container}>
            {!playerChoice && (
                <>
                    <h2>Choose your move:</h2>
                    <div style={styles.choiceContainer}>
                        {choices.map((choice) => (
                            <button
                                key={choice.name}
                                style={styles.choiceButton}
                                onClick={() => handleChoice(choice)}
                            >
                                <span style={styles.icon}>{choice.icon}</span>
                                <span>{choice.name}</span>
                            </button>
                        ))}

                    </div>
                    {otherPlayerMadeMove && (
                        <h2>The other player has made their move, Waiting on you to conclude the match</h2>
                    )}
                </>
            )}

            {playerChoice && waiting && (
                <div style={styles.waiting}>
                    <p>You chose: {playerChoice.icon} {playerChoice.name}</p>
                    <h3>Waiting for the other player to choose their move...</h3>
                </div>
            )}

            {playerChoice && !waiting && result && (
                <div style={styles.results}>
                    <p>
                        You chose: {playerChoice.icon} {playerChoice.name}
                    </p>
                    <p>
                        Opponent chose: {opponentChoice.icon} {opponentChoice.name}
                    </p>
                    <h3>{result}</h3>
                    <button
                        style={styles.playAgain}
                        onClick={() => {
                            navigate("/dashboard")
                        }}
                    >
                        Play Again
                    </button>
                </div>
            )}
        </div>
    );
}

const styles = {
    container: {
        fontFamily: "sans-serif",
        textAlign: "center",
        marginTop: "50px",
    },
    choiceContainer: {
        display: "flex",
        justifyContent: "center",
        gap: "20px",
        marginTop: "20px",
    },
    choiceButton: {
        padding: "15px",
        fontSize: "18px",
        cursor: "pointer",
        backgroundColor: "#f0f0f0",
        border: "2px solid #ccc",
        borderRadius: "10px",
        transition: "0.2s",
    },
    icon: {
        display: "block",
        fontSize: "40px",
    },
    waiting: {
        marginTop: "30px",
        fontSize: "18px",
    },
    results: {
        marginTop: "30px",
    },
    playAgain: {
        marginTop: "20px",
        padding: "10px 20px",
        fontSize: "16px",
        cursor: "pointer",
        backgroundColor: "#4CAF50",
        color: "#fff",
        border: "none",
        borderRadius: "5px",
    },
};