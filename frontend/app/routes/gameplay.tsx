import { useState, useEffect } from "react";
import { gameService } from "~/api/axiosInstances";
import { onGameConcluded, onStancePlayed } from "~/signalRHandlers/gameServiceHubConnection";

export default function GamePlay() {
    const [playerChoice, setPlayerChoice] = useState(null);
    const [computerChoice, setComputerChoice] = useState(null);
    const [result, setResult] = useState(null);
    const [waiting, setWaiting] = useState(false);

    useEffect(() => {
        const unsubscribeStancePlayed = onStancePlayed((data: any) => {
            console.log(data)
        });

        const unsubscribeMatchConcluded = onGameConcluded((data: any) => {
            console.log(data)
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

        // setTimeout(() => {
        //   const compChoice = choices[Math.floor(Math.random() * choices.length)];
        //   setComputerChoice(compChoice);

        //   if (choice.name === compChoice.name) {
        //     setResult("It's a draw!");
        //   } else if (
        //     (choice.name === "Rock" && compChoice.name === "Scissors") ||
        //     (choice.name === "Paper" && compChoice.name === "Rock") ||
        //     (choice.name === "Scissors" && compChoice.name === "Paper")
        //   ) {
        //     setResult("You win! 🎉");
        //   } else {
        //     setResult("You lose! 😢");
        //   }

        //   setWaiting(false);
        // }, 2000); // 2 second delay
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
                        Opponent chose: {computerChoice.icon} {computerChoice.name}
                    </p>
                    <h3>{result}</h3>
                    <button
                        style={styles.playAgain}
                        onClick={() => {
                            setPlayerChoice(null);
                            setResult(null);
                            setComputerChoice(null);
                            setWaiting(false);
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