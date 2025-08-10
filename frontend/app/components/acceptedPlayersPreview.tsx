import React from "react";

interface AcceptedPlayersPreviewProps {
  totalPlayers: number; // total number of icons
  acceptedCount: number; // how many players have accepted
  iconSize?: number; // px
  icon?: React.ReactNode; // custom icon
}

const DefaultIcon = ({ size }: { size: number }) => (
  <svg
    width={size}
    height={size}
    viewBox="0 0 24 24"
    fill="currentColor"
    style={{ display: "block" }}
  >
    <circle cx="12" cy="8" r="4" />
    <path d="M4 20c0-4 4-6 8-6s8 2 8 6" />
  </svg>
);

const AcceptedPlayersPreview: React.FC<AcceptedPlayersPreviewProps> = ({
  totalPlayers,
  acceptedCount,
  iconSize = 40,
  icon,
}) => {
  const icons = Array.from({ length: totalPlayers }, (_, i) => {
    const isAccepted = i < acceptedCount;
    return (
      <div
        key={i}
        style={{
          opacity: isAccepted ? 1 : 0.4,
          transition: "opacity 0.3s ease",
          color: "#4f46e5", // Indigo-600
        }}
      >
        {icon || <DefaultIcon size={iconSize} />}
      </div>
    );
  });

  return (
    <div
      style={{
        display: "flex",
        gap: "10px",
        alignItems: "center",
        justifyContent: "center",
      }}
    >
      {icons}
    </div>
  );
};

export default AcceptedPlayersPreview;