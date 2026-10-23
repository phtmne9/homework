import { useCallback, useEffect, useState } from "react";
import { ActivityIndicator, ScrollView, StyleSheet, Text, View } from "react-native";

import { Banner, Bar, Card, Muted } from "../components/ui";
import { api } from "../lib/api";
import { theme } from "../lib/theme";

export default function InsightsScreen({ history }) {
  const [data, setData] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const load = useCallback(async () => {
    if (history.length < 3) {
      setData(null);
      setError("");
      return;
    }

    setLoading(true);
    setError("");

    try {
      setData(await api.insights(history.map((item) => item.values)));
    } catch (e) {
      setError(e.message);
      setData(null);
    } finally {
      setLoading(false);
    }
  }, [history]);

  useEffect(() => { load(); }, [load]);

  if (history.length < 3) {
    return (
      <ScrollView style={styles.screen} contentContainerStyle={styles.content}>
        <Text style={styles.h1}>Инсайты</Text>
        <Card>
          <Muted>
            Нужно минимум три записи. Сейчас их {history.length}. Инсайты строятся по
            вашей истории, а не по средним значениям.
          </Muted>
        </Card>
      </ScrollView>
    );
  }

  return (
    <ScrollView style={styles.screen} contentContainerStyle={styles.content}>
      <Text style={styles.h1}>Инсайты</Text>
      <Muted style={{ marginBottom: 16 }}>
        Персональная норма и связи между показателями
      </Muted>

      {error ? <Banner>{error}</Banner> : null}
      {loading ? <ActivityIndicator color={theme.accent} /> : null}

      {data ? (
        <>
          <Card title={`Сводка за ${data.days} дней`}>
            <View style={styles.summary}>
              {[
                ["Средняя", data.average_score, theme.accent],
                ["Лучшая", data.best_score, theme.high],
                ["Худшая", data.worst_score, theme.low],
              ].map(([title, value, color]) => (
                <View key={title} style={styles.summaryItem}>
                  <Text style={[styles.summaryValue, { color }]}>{value}</Text>
                  <Muted style={{ fontSize: 12 }}>{title}</Muted>
                </View>
              ))}
            </View>
          </Card>

          <Card title="Ваша норма против ориентира">
            {Object.entries(data.baseline).map(([name, item]) => {
              if (item.target == null) return null;
              const ratio = item.average / item.target;
              const good = ratio >= 0.85 && ratio <= 1.15;

              return (
                <View key={name} style={styles.baseRow}>
                  <Text style={styles.baseTitle}>{item.title}</Text>
                  <Text style={styles.baseValue}>
                    {item.average} <Muted style={{ fontSize: 11 }}>из {item.target} {item.unit}</Muted>
                  </Text>
                  <View style={styles.baseBar}>
                    <Bar value={ratio} color={good ? theme.high : theme.medium} height={6} />
                  </View>
                </View>
              );
            })}
          </Card>

          {data.correlations.length ? (
            <Card title="Связи с самочувствием">
              <Muted style={{ marginBottom: 10, fontSize: 12 }}>
                Корреляция по вашей истории: положительная значит «больше — лучше»
              </Muted>
              {data.correlations.map((item) => (
                <View key={item.feature} style={styles.corrRow}>
                  <Text style={styles.corrTitle}>{item.title}</Text>
                  <View style={{ flex: 1, marginHorizontal: 10 }}>
                    <Bar
                      value={Math.abs(item.correlation)}
                      color={item.correlation >= 0 ? theme.high : theme.low}
                      height={6}
                    />
                  </View>
                  <Text style={[styles.corrValue,
                    { color: item.correlation >= 0 ? theme.high : theme.low }]}>
                    {item.correlation > 0 ? "+" : ""}{item.correlation}
                  </Text>
                </View>
              ))}
            </Card>
          ) : (
            <Card title="Связи с самочувствием">
              <Muted>Нужно минимум пять записей, чтобы считать корреляции</Muted>
            </Card>
          )}

          <Card title="Что важно для модели">
            <Muted style={{ marginBottom: 10, fontSize: 12 }}>
              Вклад признаков, выученный на обучающих данных
            </Muted>
            {data.model_importance.map((item) => (
              <View key={item.feature} style={styles.corrRow}>
                <Text style={styles.corrTitle}>
                  {data.baseline[item.feature]?.title ?? item.feature}
                </Text>
                <View style={{ flex: 1, marginHorizontal: 10 }}>
                  <Bar value={item.importance} color={theme.accent} height={6} />
                </View>
                <Text style={styles.corrValue}>{item.importance}</Text>
              </View>
            ))}
          </Card>
        </>
      ) : null}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.bg },
  content: { padding: 16, paddingBottom: 40 },
  h1: { color: theme.ink, fontSize: 26, fontWeight: "800", marginBottom: 4 },
  summary: { flexDirection: "row", justifyContent: "space-between" },
  summaryItem: { alignItems: "center", flex: 1 },
  summaryValue: { fontSize: 26, fontWeight: "800" },
  baseRow: { marginBottom: 12 },
  baseTitle: { color: theme.ink, fontSize: 13, fontWeight: "600" },
  baseValue: { color: theme.ink, fontSize: 13, marginTop: 2 },
  baseBar: { marginTop: 6 },
  corrRow: { flexDirection: "row", alignItems: "center", marginBottom: 9 },
  corrTitle: { color: theme.ink, fontSize: 13, width: 104 },
  corrValue: { color: theme.muted, fontSize: 12, width: 48, textAlign: "right" },
});
