import { useTranslation } from "react-i18next";
import "../App.css";
import LanguageSwitch from "../layout/components/LanguageSwitch";
import NormalButton from "../layout/components/NormalButton";
import { useNavigate, useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { GetCurrentLanguage, GetOfflineUserData } from "../businesslogic/services/UserDataOffline";
import { FetchAndSaveUserDataByUniId } from "../businesslogic/services/UserDataFetch";
import i18next from "i18next";
import NormalMessage from "../layout/components/NormalMessage";

function InitialSelectionView() {
  const { message } = useParams() ?? null;
  const [normalMessage, setNormalMessage] = useState<string | null>(null);
  const navigate = useNavigate();
  const { t } = useTranslation();

  useEffect(() => {
    init();
    message && setNormalMessage(t(String(message)));
    setTimeout(() => setNormalMessage(null), 3000);
  }, []);

  const init = async () => {
    const lang = await GetCurrentLanguage();
    if (lang) i18next.changeLanguage(lang);
    let localData = await GetOfflineUserData();
    const loginSuccess = await FetchAndSaveUserDataByUniId(String(localData?.uniId));
    if (typeof loginSuccess !== "string") {
      navigate("/Home");
    } else {
      return;
    }
  };

  return (
    <>
      <div className="max-h-screen max-w-screen flex items-center justify-center gap-10">
        <div className="flex flex-col md:p-20 max-md:p-10 items-center gap-20 bg-main-dark rounded-3xl">
          <img src="../assets/logo/white-on-transparent.png" className="w-xl" />
          <div className="flex flex-col gap-3.5">
            {normalMessage && <NormalMessage text={t(normalMessage)} />}
            <NormalButton text={t("log-in")} onClick={() => navigate("/Login")} />
            <NormalButton text={t("register-as-teacher")} onClick={() => navigate("/CreateAccount")} />
            <LanguageSwitch linkStyle={true} />
          </div>
        </div>
      </div>
    </>
  );
}

export default InitialSelectionView;
