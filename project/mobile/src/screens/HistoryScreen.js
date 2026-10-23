import { useState } from "react";
import { Pressable, ScrollView, StyleSheet, Text, View } from "react-native";

import { Bar, Card, Muted } from "../components/ui";
import { clearHistory, removeEntry } from "../lib/storage";
import { classColor, classTitle, theme } from "../lib/theme";

const formatDate = (iso) =>
  new Date(iso).toLocaleString("ru-RU", {
    day: "2-digit", month: "2-digit", hour: "2-digit", minute: "2-digit",
  });

export default function HistoryScreen({ history, onChange }) {
  const [busy, setBusy] = useState(false);

  const remove = async (id) => {
    setBusy(true);
    onChange(await removeEntry(id));
    setBusy(false);
  };

  const clear = async () => {
    setBusy(true);
    onChange(await clearHistory());
    setBusy(false);
  };

  const chart = [...history].slice(0, 14).reverse();
  const average = history.length
    ? history.reduce((sum, item) => sum + item.energy_score, 0) / history.length
    : 0;

  return (
    <ScrollView style={styles.screen} contentContainerStyle={styles.content}>
      <Text style={styles.h1}>История</Text>
      <Muted style={{ marginBottom: 16 }}>
        Записи хранятся на устройстве, последние 60 дней
      </Muted>

      {history.length === 0 ? (
        <Card><Muted>Пока пусто. Оцените день на вкладке «Сегодня».</Muted></Card>
      ) : (
        <>
          <Card title={`Динамика энергии, последние ${chart.length}`}>
            <View style={styles.chart}>
              {chart.map((item) => (
                <View key={item.id} style={styles.column}>
                  <View
                    style={{
                      width: "70%",
                      height: `${(item.energy_score / 10) * 100}%`,
                      backgroundColor: classColor(item.wellbeing_class),
                      borderRadius: 4,
                    }}
                  />
                </View>
              ))}
            </View>
            <View style={styles.chartAxis}>
              <Muted style={{ fontSize: 11 }}>раньше</Muted>
              <Muted style={{ fontSize: 11 }}>сейчас</Muted>
            </View>
            <Muted style={{ marginTop: 8 }}>
              Средняя оценка за всё время: {average.toFixed(2)}
            </Muted>
          </Card>

          {history.map((item) => (
            <Card key={item.id}>
              <View style={styles.entryHead}>
                <View>
                  <Text style={styles.entryDate}>{formatDate(item.date)}</Text>
                  <Muted style={{ fontSize: 12 }}>
                    сон {item.values.sleep_hours} ч · вода {item.values.water_ml} мл ·
                    шаги {item.values.steps}
                  </Muted>
                </View>
                <Pressable onPress={() => remove(item.id)} disabled={busy} hitSlop={10}>
                  <Text style={styles.remove}>✕</Text>
                </Pressable>
              </View>

              <View style={styles.entryScore}>
                <Text style={[styles.scoreValue, { color: classColor(item.wellbeing_class) }]}>
                  {item.energy_score}
                </Text>
                <View style={{ flex: 1, marginHorizontal: 12 }}>
                  <Bar value={item.energy_score / 10} color={classColor(item.wellbeing_class)} />
                </View>
                <Text style={[styles.entryClass, { color: classColor(item.wellbeing_class) }]}>
                  {classTitle(item.wellbeing_class)}
                </Text>
              </View>
            </Card>
          ))}

          <Pressable style={styles.clear} onPress={clear} disabled={busy}>
            <Text style={styles.clearText}>Очистить историю</Text>
          </Pressable>
        </>
      )}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.bg },
  content: { padding: 16, paddingBottom: 40 },
  h1: { color: theme.ink, fontSize: 26, fontWeight: "800", marginBottom: 4 },
  chart: { flexDirection: "row", alignItems: "flex-end", height: 120, gap: 5 },
  column: { flex: 1, height: "100%", justifyContent: "flex-end", alignItems: "center" },
  chartAxis: { flexDirection: "row", justifyContent: "space-between", marginTop: 6 },
  entryHead: { flexDirection: "row", justifyContent: "space-between", marginBottom: 12 },
  entryDate: { color: theme.ink, fontSize: 14, fontWeight: "600", marginBottom: 3 },
  remove: { color: theme.muted, fontSize: 16, paddingHorizontal: 4 },
  entryScore: { flexDirection: "row", alignItems: "center" },
  scoreValue: { fontSize: 20, fontWeight: "800", width: 46 },
  entryClass: { fontSize: 12, fontWeight: "700", width: 66, textAlign: "right" },
  clear: {
    borderWidth: 1,
    borderColor: theme.line,
    borderRadius: 12,
    paddingVertical: 12,
    alignItems: "center",
  },
  clearText: { color: theme.low, fontSize: 14, fontWeight: "600" },
});
