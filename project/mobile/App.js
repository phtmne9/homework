import { DarkTheme, NavigationContainer } from "@react-navigation/native";
import { createBottomTabNavigator } from "@react-navigation/bottom-tabs";
import { useEffect, useState } from "react";
import { StatusBar } from "expo-status-bar";
import { SafeAreaProvider } from "react-native-safe-area-context";
import { Text } from "react-native";

import HistoryScreen from "./src/screens/HistoryScreen";
import InsightsScreen from "./src/screens/InsightsScreen";
import ModelScreen from "./src/screens/ModelScreen";
import TodayScreen from "./src/screens/TodayScreen";
import { loadHistory } from "./src/lib/storage";
import { theme } from "./src/lib/theme";

const Tab = createBottomTabNavigator();

const ICONS = { Сегодня: "◉", История: "▤", Инсайты: "◈", Модель: "⚙" };

const linking = {
  prefixes: [],
  config: {
    screens: {
      "Сегодня": "",
      "История": "history",
      "Инсайты": "insights",
      "Модель": "model",
    },
  },
};

const navTheme = {
  ...DarkTheme,
  colors: {
    ...DarkTheme.colors,
    primary: theme.accent,
    background: theme.bg,
    card: theme.card,
    text: theme.ink,
    border: theme.line,
    notification: theme.accent,
  },
};

export default function App() {
  const [history, setHistory] = useState([]);

  useEffect(() => { loadHistory().then(setHistory); }, []);

  return (
    <SafeAreaProvider>
      <StatusBar style="light" />
      <NavigationContainer theme={navTheme} linking={linking}>
        <Tab.Navigator
          screenOptions={({ route }) => ({
            headerShown: false,
            tabBarActiveTintColor: theme.accent,
            tabBarInactiveTintColor: theme.muted,
            tabBarStyle: {
              backgroundColor: theme.card,
              borderTopColor: theme.line,
              height: 74,
              paddingBottom: 14,
              paddingTop: 8,
            },
            tabBarLabelStyle: { fontSize: 11, marginTop: 2 },
            tabBarIcon: ({ color }) => (
              <Text style={{ color, fontSize: 18 }}>{ICONS[route.name]}</Text>
            ),
          })}
        >
          <Tab.Screen name="Сегодня">
            {() => <TodayScreen onSaved={setHistory} />}
          </Tab.Screen>
          <Tab.Screen name="История">
            {() => <HistoryScreen history={history} onChange={setHistory} />}
          </Tab.Screen>
          <Tab.Screen name="Инсайты">
            {() => <InsightsScreen history={history} />}
          </Tab.Screen>
          <Tab.Screen name="Модель" component={ModelScreen} />
        </Tab.Navigator>
      </NavigationContainer>
    </SafeAreaProvider>
  );
}
