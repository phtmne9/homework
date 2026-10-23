import { useEffect, useState } from "react";
import { ActivityIndicator, ScrollView, StyleSheet, Text, View } from "react-native";

import { Banner, Card, Muted } from "../components/ui";
import { API_URL, api } from "../lib/api";
import { theme } from "../lib/theme";

export default function ModelScreen() {
  const [meta, setMeta] = useState(null);
  const [health, setHealth] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    Promise.all([api.metrics(), api.health()])
      .then(([m, h]) => { setMeta(m); setHealth(h); })
      .catch((e) => setError(e.message));
  }, []);

  return (
    <ScrollView style={styles.screen} contentContainerStyle={styles.content}>
      <Text style={styles.h1}>Модель</Text>
      <Muted style={{ marginBottom: 16 }}>Качество и состав моделей на сервере</Muted>

      {error ? <Banner>{error}</Banner> : null}
      {!meta && !error ? <ActivityIndicator color={theme.accent} /> : null}

      {health ? (
        <Card title="Сервер">
          <Row label="Адрес" value={API_URL} />
          <Row label="Статус" value={health.status.toUpperCase()} color={theme.high} />
          <Row label="Признаков" value={String(health.features)} />
        </Card>
      ) : null}

      {meta ? (
        <>
          <Card title="Регрессия — оценка энергии">
            <Row label="Модель" value={meta.regressor.name} />
            <Row label="MAE" value={String(meta.regressor.mae)} />
            <Row label="RMSE" value={String(meta.regressor.rmse)} />
            <Row label="R²" value={String(meta.regressor.r2)} color={theme.high} />
          </Card>

          <Card title="Классификация — уровень самочувствия">
            <Row label="Модель" value={meta.classifier.name} />
            <Row label="Accuracy" value={String(meta.classifier.accuracy)} />
            <Row label="F1-macro" value={String(meta.classifier.f1_macro)} color={theme.high} />
          </Card>

          <Card title="Сравнение моделей">
            <Muted style={{ marginBottom: 8, fontSize: 12 }}>Регрессия</Muted>
            {Object.entries(meta.all_regressors).map(([name, m]) => (
              <Row key={name} label={name} value={`R² ${m.r2} · MAE ${m.mae}`} />
            ))}
            <Muted style={{ marginTop: 10, marginBottom: 8, fontSize: 12 }}>Классификация</Muted>
            {Object.entries(meta.all_classifiers).map(([name, m]) => (
              <Row key={name} label={name} value={`F1 ${m.f1_macro} · acc ${m.accuracy}`} />
            ))}
          </Card>

          <Card title="Данные">
            <Row label="Обучающая выборка" value={`${meta.train_size} дней`} />
            <Row label="Тестовая выборка" value={`${meta.test_size} дней`} />
            <Muted style={{ marginTop: 10, fontSize: 12 }}>
              Данные синтетические: сгенерированы скриптом с заложенными нелинейными
              связями. Реальные показатели пользователей не использовались.
            </Muted>
          </Card>
        </>
      ) : null}
    </ScrollView>
  );
}

function Row({ label, value, color }) {
  return (
    <View style={styles.row}>
      <Text style={styles.rowLabel}>{label}</Text>
      <Text style={[styles.rowValue, color && { color }]}>{value}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  screen: { flex: 1, backgroundColor: theme.bg },
  content: { padding: 16, paddingBottom: 40 },
  h1: { color: theme.ink, fontSize: 26, fontWeight: "800", marginBottom: 4 },
  row: {
    flexDirection: "row", justifyContent: "space-between",
    alignItems: "center", paddingVertical: 5,
  },
  rowLabel: { color: theme.muted, fontSize: 13, flexShrink: 1 },
  rowValue: { color: theme.ink, fontSize: 13, fontWeight: "600", marginLeft: 10 },
});
