import i18next from "i18next";
import { initReactI18next, Translation } from "react-i18next";
import en from "../../locales/en.json";
import et from "../../locales/et.json";
import fi from "../../locales/fi.json";
import lv from "../../locales/lv.json";
import lt from "../../locales/lt.json";
import ge from "../../locales/ge.json";
import uk from "../../locales/uk.json";
import ru from "../../locales/ru.json";

export const languageResources = {
  en: { translation: en },
  et: { translation: et },
  fi: { translation: fi },
  lv: { translation: lv },
  lt: { translation: lt },
  ge: { translation: ge },
  uk: { translation: uk },
  ru: { translation: ru },
};

i18next.use(initReactI18next).init({
  compatibilityJSON: "v4",
  lng: "et",
  fallbackLng: "en",
  resources: languageResources,
});

export default i18next;
