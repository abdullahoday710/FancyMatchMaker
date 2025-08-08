import React, { forwardRef, useImperativeHandle, useState } from "react";

export interface MatchFoundModalHandle {
  show: () => void;
  hide: () => void;
  toggle: () => void;
}

type MatchFoundModalProps = {
  children: React.ReactNode;
};

const MatchFoundModal = forwardRef<MatchFoundModalHandle, MatchFoundModalProps>((props, ref) => {
  const [visible, setVisible] = useState(false);

  useImperativeHandle(ref, () => ({
    show: () => setVisible(true),
    hide: () => setVisible(false),
    toggle: () => setVisible((v) => !v),
  }));

  if (!visible) return null;

  return (
    <div style={styles.overlay}>
      <div style={styles.modal}>
        {props.children}
      </div>
    </div>
  );
});

const styles: Record<string, React.CSSProperties> = {
  overlay: {
    position: "fixed",
    top: 0,
    left: 0,
    width: "100vw",
    height: "100vh",
    backgroundColor: "rgba(0,0,0,0.5)",
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    zIndex: 1000,
  },
  modal: {
    backgroundColor: "#fff",
    padding: 20,
    borderRadius: 8,
    minWidth: 300,
    position: "relative",
  },
  closeBtn: {
    position: "absolute",
    top: 10,
    right: 15,
    fontSize: 20,
    border: "none",
    background: "none",
    cursor: "pointer",
  },
};

export default MatchFoundModal;