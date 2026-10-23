import AsyncStorage from "@react-native-async-storage/async-storage";

const KEY = "health-tracker:history";

export async function loadHistory() {
  try {
    const raw = await AsyncStorage.getItem(KEY);
    return raw ? JSON.parse(raw) : [];
  } catch {
    return [];
  }
}

export async function saveEntry(entry) {
  const history = await loadHistory();
  const next = [entry, ...history].slice(0, 60);

  try {
    await AsyncStorage.setItem(KEY, JSON.stringify(next));
  } catch {
    // переполнение хранилища не должно ронять приложение
  }

  return next;
}

export async function removeEntry(id) {
  const history = await loadHistory();
  const next = history.filter((item) => item.id !== id);
  await AsyncStorage.setItem(KEY, JSON.stringify(next));
  return next;
}

export async function clearHistory() {
  await AsyncStorage.removeItem(KEY);
  return [];
}
