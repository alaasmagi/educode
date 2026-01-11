import { useState, useEffect } from "react";
import { Flags } from "./Icons";
import i18next from "../../../services/i18next";
import NormalLink from "./Link";

interface LanguageSwitchProperties {
  linkStyle?: boolean;
}

const LanguageSwitch: React.FC<LanguageSwitchProperties> = ({ linkStyle = false }) => {
  const [currentLang, setCurrentLang] = useState(i18next.language);

  useEffect(() => {
    const handleLanguageChange = (lng: string) => {
      setCurrentLang(lng);
    };

    i18next.on("languageChanged", handleLanguageChange);

    return () => {
      i18next.off("languageChanged", handleLanguageChange);
    };
  }, []);

  const toggleLanguage = () => {
    const newLang = currentLang === "et" ? "en" : "et";
    i18next.changeLanguage(newLang);
  };

  return (
    <>
      {linkStyle == false ? (
        <button
          className="hover:bg-button-hover hover:cursor-pointer px-3 py-2 bg-button-dark border-main-blue rounded-2xl border-2"
          onClick={toggleLanguage}
        >
          <img src={currentLang === "en" ? Flags["eng-flag"] : Flags["est-flag"]} className="w-10" />
        </button>
      ) : (
        <NormalLink text={currentLang === "en" ? "Eesti keeles" : "In english"} onClick={toggleLanguage} />
      )}
    </>
  );
};

export default LanguageSwitch;
