import React, { useEffect, useState, useRef } from "react";

interface CircularCountdownProps {
    duration: number; // seconds
    size?: number;
    strokeWidth?: number;
    color?: string;
    onComplete?: () => void;
}

const CircularCountdown: React.FC<CircularCountdownProps> = ({
    duration,
    size = 100,
    strokeWidth = 8,
    color = "#4f46e5", // Indigo-600
    onComplete,
}) => {
    const [timeLeft, setTimeLeft] = useState(duration);
    const requestRef = useRef<number>();
    const startTimeRef = useRef<number>();

    useEffect(() => {
        const animate = (timestamp: number) => {
            if (!startTimeRef.current) startTimeRef.current = timestamp;

            const elapsed = (timestamp - startTimeRef.current) / 1000; // in seconds
            const remaining = Math.max(duration - elapsed, 0);

            setTimeLeft(remaining);

            if (remaining > 0) {
                requestRef.current = requestAnimationFrame(animate);
            } else {
                onComplete?.();
            }
        };

        requestRef.current = requestAnimationFrame(animate);

        return () => {
            if (requestRef.current) cancelAnimationFrame(requestRef.current);
        };
    }, [duration, onComplete]);

    // Circle math
    const radius = (size - strokeWidth) / 2;
    const circumference = 2 * Math.PI * radius;
    const progress = circumference - (timeLeft / duration) * circumference;

    return (
        <div style={{ width: size, height: size, position: "relative" }}>
            <svg width={size} height={size}>
                {/* Background circle */}
                <circle
                    stroke="#e5e7eb"
                    fill="transparent"
                    strokeWidth={strokeWidth}
                    r={radius}
                    cx={size / 2}
                    cy={size / 2}
                />
                {/* Progress circle */}
                <circle
                    stroke={color}
                    fill="transparent"
                    strokeWidth={strokeWidth}
                    strokeDasharray={circumference}
                    strokeDashoffset={progress}
                    strokeLinecap="round"
                    r={radius}
                    cx={size / 2}
                    cy={size / 2}
                    style={{ transition: "stroke-dashoffset 0.016s linear" }} // ~60fps
                />
            </svg>
            {/* Time label */}
            <div
                style={{
                    position: "absolute",
                    top: "50%",
                    left: "50%",
                    transform: "translate(-50%, -50%)",
                    fontWeight: "bold",
                    fontSize: size / 4,
                }}
            >
                {Math.ceil(timeLeft)}
            </div>
        </div>
    );
};

export default CircularCountdown;