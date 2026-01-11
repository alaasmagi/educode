import i18next from "i18next";
import { initReactI18next } from "react-i18next";
import en from "../src/locales/en.json";
import et from "../src/locales/et.json";


export const languageResources = {
    en: {translation: en},
    et: {translation: et}
}

i18next.use(initReactI18next).init({
    compatibilityJSON:"v4",
    lng:'et',
    fallbackLng:'en',
    resources: languageResources
});

export default i18next;