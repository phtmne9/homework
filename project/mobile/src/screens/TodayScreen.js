import { useEffect, useState } from "react";
import { ActivityIndicator, Pressable, ScrollView, StyleSheet, Text,
         TextInput, View } from "react-native";

import { Banner, Bar, Card, Muted } from "../components/ui";
import { api } from "../lib/api";
import { saveEntry } from "../lib/storage";
import { classColor, classTitle, theme } from "../lib/theme";

const DEFAULTS = {
  sleep_hours: "7.5",
  water_ml: "2000",
  steps: "8000",
  calories: "2100",
  protein_g: "110",
  caffeine_mg: "120",
  screen_hours: "5",
  stress_level: "4",
};

export default function TodayScreen({ onSaved }) {
  const [fields, setFields] = useState([]);
  const [values, setValues] = useState(DEFAULTS);
  const [invalid, setInvalid] = useState({});
  const [result, setResult] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    api.features()
      .then((data) => setFields(data.features))
      .catch((e) => setError(e.message));
  }, []);

  const change = (name, text) => {
    setValues((prev) => ({ ...prev, [name]: text.replace(",", ".") }));
    setInvalid((prev) => ({ ...prev, [name]: false }));
  };

  const validate = () => {
    const payload = {};
    const bad = {};

    for (const field of fields) {
      const raw = (values[field.name] ?? "").trim();
      const number = Number(raw);

      if (raw === "" || Number.isNaN(number) || number < field.min || number > field.max) {
        bad[field.name] = true;
        continue;
      }

      payload[field.name] = number;
    }

    setInvalid(bad);

    return Object.keys(bad).length ? null : payload;
  };

  const analyze = async () => {
    setError("");
    const payload = validate();

    if (!payload) {
      setError("Проверьте подсвеченные поля: значение пустое или вне допустимого диапазона");
      setResult(null);
      return;
    }

    setLoading(true);

    try {
      const data = await api.predict(payload);
      setResult(data);

      const history = await saveEntry({
        id: Date.now(),
        date: new Date().toISOString(),
        values: payload,
        energy_score: data.energy_score,
        wellbeing_class: data.wellbeing_class,
      });

      onSaved?.(history);
    } catch (e) {
      setError(e.message);
      setResult(null);
    } finally {
      setLoading(false);
    }
  };

  return (
    <ScrollView style={styles.screen} contentContainerStyle={styles.content}>
      <Text style={styles.h1}>Сегодня</Text>
      <Muted style={{ marginBottom: 16 }}>
        Внесите показатели дня — модель оценит самочувствие и подскажет, что тянет его вниз
      </Muted>

      {error ? <Banner>{error}</Banner> : null}

      <Card title="Показатели">
        {fields.length === 0 ? (
          <Muted>Загрузка полей…</Muted>
        ) : (
          fields.map((field) => (
            <View key={field.name} style={styles.row}>
              <View style={styles.label}>
                <Text style={styles.labelText}>{field.title}</Text>
                {field.target != null ? (
                  <Muted style={{ fontSize: 11 }}>ориентир {field.target} {field.unit}</Muted>
                ) : null}
              </View>
              <TextInput
                style={[styles.input, invalid[field.name] && styles.inputBad]}
                value={values[field.name]}
                onChangeText={(text) => change(field.name, text)}
                keyboardType="numeric"
                placeholder={field.unit}
                placeholderTextColor={theme.muted}
              />
            </View>
          ))
        )}

        <Pressable
          style={({ pressed }) => [styles.button, pressed && styles.buttonPressed,
                                   loading && styles.buttonDisabled]}
          onPress={analyze}
          disabled={loading || fields.length === 0}
        >
          {loading
            ? <ActivityIndicator color="#fff" />
            : <Text style={styles.buttonText}>Оценить день</Text>}
        </Pressable>
      </Card>

      {result ? (
        <>
          <Card>
            <View style={styles.scoreRow}>
              <View>
                <Muted>Оценка энергии</Muted>
                <Text style={[styles.score, { color: classColor(result.wellbeing_class) }]}>
                  {result.energy_score}
                  <Text style={styles.scoreMax}> / 10</Text>
                </Text>
              </View>
              <View style={[styles.badge,
                            { backgroundColor: classColor(result.wellbeing_class) }]}>
                <Text style={styles.badgeText}>{classTitle(result.wellbeing_class)}</Text>
              </View>
            </View>

            <Muted style={{ marginTop: 10 }}>
              Средняя ошибка модели — {result.model_mae} балла
            </Muted>
          </Card>

          <Card title="Уверенность модели">
            {Object.entries(result.probabilities)
              .sort((a, b) => b[1] - a[1])
              .map(([label, value]) => (
                <View key={label} style={styles.probRow}>
                  <Text style={styles.probName}>{classTitle(label)}</Text>
                  <View style={{ flex: 1, marginHorizontal: 10 }}>
                    <Bar value={value} color={classColor(label)} />
                  </View>
                  <Text style={styles.probValue}>{(value * 100).toFixed(1)}%</Text>
                </View>
              ))}
          </Card>

          <Card title="Что тянет вниз">
            {result.advice.length === 0 ? (
              <Muted>Все показатели близки к ориентирам — заметных провалов нет</Muted>
            ) : (
              result.advice.map((item) => (
                <View key={item.feature} style={styles.advice}>
                  <View style={[styles.dot, { backgroundColor: theme.low }]} />
                  <Text style={styles.adviceText}>{item.message}</Text>
                </View>
              ))
            )}
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
  row: { flexDirection: "row", alignItems: "center", marginBottom: 10 },
  label: { flex: 1 },
  labelText: { color: theme.ink, fontSize: 14, fontWeight: "600" },
  input: {
    width: 108,
    backgroundColor: theme.cardSoft,
    borderWidth: 1,
    borderColor: theme.line,
    borderRadius: 10,
    paddingHorizontal: 12,
    paddingVertical: 9,
    color: theme.ink,
    fontSize: 15,
    textAlign: "right",
  },
  inputBad: { borderColor: theme.low, backgroundColor: "#2a1d1d" },
  button: {
    marginTop: 8,
    backgroundColor: theme.accent,
    borderRadius: 12,
    paddingVertical: 13,
    alignItems: "center",
  },
  buttonPressed: { opacity: 0.85 },
  buttonDisabled: { backgroundColor: "#3c4a6e" },
  buttonText: { color: "#fff", fontSize: 16, fontWeight: "700" },
  scoreRow: { flexDirection: "row", alignItems: "center", justifyContent: "space-between" },
  score: { fontSize: 42, fontWeight: "800" },
  scoreMax: { fontSize: 18, color: theme.muted, fontWeight: "600" },
  badge: { paddingHorizontal: 14, paddingVertical: 7, borderRadius: 999 },
  badgeText: { color: "#0f1420", fontWeight: "800", fontSize: 13 },
  probRow: { flexDirection: "row", alignItems: "center", marginBottom: 9 },
  probName: { color: theme.ink, fontSize: 13, width: 72 },
  probValue: { color: theme.muted, fontSize: 12, width: 52, textAlign: "right" },
  advice: { flexDirection: "row", alignItems: "center", marginBottom: 9 },
  dot: { width: 7, height: 7, borderRadius: 999, marginRight: 10 },
  adviceText: { color: theme.ink, fontSize: 13, flex: 1 },
});
