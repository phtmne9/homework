import { StyleSheet, Text, View } from "react-native";

import { theme } from "../lib/theme";

export function Card({ title, children, style }) {
  return (
    <View style={[styles.card, style]}>
      {title ? <Text style={styles.cardTitle}>{title}</Text> : null}
      {children}
    </View>
  );
}

export function Muted({ children, style }) {
  return <Text style={[styles.muted, style]}>{children}</Text>;
}

export function Banner({ kind = "error", children }) {
  const color = kind === "error" ? theme.low : theme.accent;

  return (
    <View style={[styles.banner, { borderColor: color }]}>
      <Text style={{ color, fontSize: 13 }}>{children}</Text>
    </View>
  );
}

export function Bar({ value, color, height = 8 }) {
  return (
    <View style={[styles.barTrack, { height }]}>
      <View
        style={{
          width: `${Math.max(0, Math.min(value, 1)) * 100}%`,
          height: "100%",
          borderRadius: height,
          backgroundColor: color,
        }}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: theme.card,
    borderRadius: 16,
    borderWidth: 1,
    borderColor: theme.line,
    padding: 16,
    marginBottom: 14,
  },
  cardTitle: {
    color: theme.ink,
    fontSize: 15,
    fontWeight: "700",
    marginBottom: 12,
  },
  muted: { color: theme.muted, fontSize: 13 },
  banner: {
    borderWidth: 1,
    borderRadius: 12,
    padding: 12,
    marginBottom: 14,
    backgroundColor: theme.cardSoft,
  },
  barTrack: {
    backgroundColor: theme.cardSoft,
    borderRadius: 999,
    overflow: "hidden",
  },
});
