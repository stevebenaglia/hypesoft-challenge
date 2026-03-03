"use client";

import { createContext, useContext, useState, useEffect } from "react";
import { NextIntlClientProvider } from "next-intl";
import ptBR from "@/locales/pt-BR.json";
import en from "@/locales/en.json";
import es from "@/locales/es.json";

export type Locale = "pt-BR" | "en" | "es";

const allMessages = { "pt-BR": ptBR, en, es } as const;

export const LOCALES: { value: Locale; label: string }[] = [
  { value: "pt-BR", label: "PT" },
  { value: "en", label: "EN" },
  { value: "es", label: "ES" },
];

interface LocaleContextValue {
  locale: Locale;
  setLocale: (locale: Locale) => void;
}

const LocaleContext = createContext<LocaleContextValue>({
  locale: "pt-BR",
  setLocale: () => {},
});

export function useLocale() {
  return useContext(LocaleContext);
}

export function LocaleProvider({ children }: { children: React.ReactNode }) {
  const [locale, setLocaleState] = useState<Locale>("pt-BR");

  useEffect(() => {
    const stored = localStorage.getItem("locale") as Locale | null;
    if (stored && stored in allMessages) {
      setLocaleState(stored);
    }
  }, []);

  function setLocale(newLocale: Locale) {
    localStorage.setItem("locale", newLocale);
    setLocaleState(newLocale);
  }

  return (
    <LocaleContext.Provider value={{ locale, setLocale }}>
      <NextIntlClientProvider locale={locale} messages={allMessages[locale]}>
        {children}
      </NextIntlClientProvider>
    </LocaleContext.Provider>
  );
}
