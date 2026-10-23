import { useEffect, useState } from "react";

import Analyze from "./components/Analyze.jsx";
import History from "./components/History.jsx";
import Stats from "./components/Stats.jsx";

const TABS = [
  { id: "analyze", title: "Анализ текста" },
  { id: "history", title: "История" },
  { id: "stats", title: "Статистика" },
];

const fromHash = () => {
  const id = window.location.hash.replace("#", "");
  return TABS.some((item) => item.id === id) ? id : "analyze";
};

export default function App() {
  const [tab, setTab] = useState(fromHash);
  const [version, setVersion] = useState(0);

  useEffect(() => {
    const sync = () => setTab(fromHash());
    window.addEventListener("hashchange", sync);
    return () => window.removeEventListener("hashchange", sync);
  }, []);

  const open = (id) => {
    window.location.hash = id;
    setTab(id);
  };

  const refresh = () => setVersion((v) => v + 1);

  return (
    <div className="page">
      <header className="head">
        <h1>AI Sentiment Analyzer</h1>
        <p>Определение тональности текста готовой моделью rubert-tiny</p>
      </header>

      <nav className="tabs">
        {TABS.map((item) => (
          <button
            key={item.id}
            className={tab === item.id ? "tab active" : "tab"}
            onClick={() => open(item.id)}
          >
            {item.title}
          </button>
        ))}
      </nav>

      <main>
        {tab === "analyze" && <Analyze onAnalyzed={refresh} />}
        {tab === "history" && <History version={version} onChange={refresh} />}
        {tab === "stats" && <Stats version={version} />}
      </main>
    </div>
  );
}
