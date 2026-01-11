import "../App.css";
import NormalButton from "../layout/components/NormalButton";
import { useNavigate } from "react-router-dom";
import TextBox from "../layout/components/TextBox";
import NormalLink from "../layout/components/Link";
import ErrorMessage from "../layout/components/ErrorMessage";
import { useCallback, useEffect, useState } from "react";
import { CreateUserAccount, RequestOTP, VerifyOTP } from "../businesslogic/services/UserDataFetch";
import { useTranslation } from "react-i18next";
import LanguageSwitch from "../layout/components/LanguageSwitch";
import { RegexFilters } from "../businesslogic/helpers/RegexFilters";
import NormalMessage from "../layout/components/NormalMessage";
import VerifyOTPModel from "../models/VerifyOTPModel";
import CreateUserModel from "../models/CreateUserModel";
import UnderlineText from "../layout/components/UnderlineText";
import DataField from "../layout/components/DataField";

function CreateAccountView() {
  const [stepNr, setStepNr] = useState(1);
  const [fullName, setFullName] = useState("");
  const [uniIdInput, setUniIdInput] = useState("");
  const [emailCode, setEmailCode] = useState("");
  const [password, setPassword] = useState("");
  const [passwordAgain, setPasswordAgain] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [normalMessage, setNormalMessage] = useState<string | null>("null");
  const [isButtonDisabled, setIsButtonDisabled] = useState<boolean>(false);

  const navigate = useNavigate();
  const { t } = useTranslation();

  const showTemporaryError = useCallback((message: string) => {
    setErrorMessage(message);
    const timeout = setTimeout(() => setErrorMessage(null), 3000);
    return () => clearTimeout(timeout);
  }, []);

  const isNameFormValid = () => fullName !== "" && !fullName.includes(";");
  const isPasswordFormValid = () => password.length >= 8 && password === passwordAgain;

  useEffect(() => {
    if (password.length < 8 && password !== "") {
      setNormalMessage(t("password-length-message"));
    } else if (!isPasswordFormValid() && password && passwordAgain) {
      setNormalMessage(t("password-match-message"));
    } else {
      setNormalMessage("");
    }
  }, [password, passwordAgain]);

  const handleOTPRequest = useCallback(async () => {
    setIsButtonDisabled(true);
    const status = await RequestOTP(uniIdInput, fullName);
    if (status === true) {
      setIsButtonDisabled(false);
      setStepNr(3);
    } else {
      setIsButtonDisabled(false);
      showTemporaryError(t(String(status)));
    }
  }, [uniIdInput, fullName, t, showTemporaryError]);

  const handleOTPVerification = useCallback(async () => {
    setIsButtonDisabled(true);
    const otpData: VerifyOTPModel = { uniId: uniIdInput, otp: emailCode };
    const status = await VerifyOTP(otpData);
    if (status === true) {
      setIsButtonDisabled(false);
      setStepNr(4);
    } else {
      setIsButtonDisabled(false);
      showTemporaryError(t(String(status)));
    }
  }, [uniIdInput, emailCode, t, showTemporaryError]);

  const handleRegister = useCallback(async () => {
    setIsButtonDisabled(true);
    const userData: CreateUserModel = {
      uniId: uniIdInput,
      fullName: fullName.trim(),
      password,
    };
    const status = await CreateUserAccount(userData);
    if (typeof status !== "string") {
      setIsButtonDisabled(false);
      navigate("/Login/create-account-success");
    } else {
      setIsButtonDisabled(false);
      showTemporaryError(t(String(status)));
    }
  }, [uniIdInput, fullName, password, navigate, t, showTemporaryError]);

  const renderStep = () => {
    const sharedMessage = normalMessage || errorMessage;
    const messageComponent = errorMessage ? (
      <ErrorMessage text={errorMessage} />
    ) : (
      <NormalMessage text={normalMessage ?? ""} />
    );

    switch (stepNr) {
      case 1:
        return (
          <div className="flex flex-col gap-6">
            <div className="flex flex-col gap-10">
              <div className="flex min-w-2xs flex-col gap-5">
                <TextBox
                  icon="person-icon"
                  label={t("name")}
                  value={fullName}
                  placeHolder={t("for-example-abbr") + "Andres Tamm"}
                  autofocus={true}
                  onChange={setFullName}
                />
              </div>
              {sharedMessage && <>{messageComponent}</>}
            </div>
            <div className="flex flex-col self-center justify-center ">
              <NormalButton
                text={t("continue")}
                onClick={() => setStepNr(2)}
                isDisabled={!isNameFormValid() || isButtonDisabled}
              />
              <div className="flex flex-col gap-4">
                <NormalLink text={t("already-registered")} onClick={() => navigate("/Login")} />
                <LanguageSwitch linkStyle={true} />
              </div>
            </div>
          </div>
        );
      case 2:
        return (
          <div className="flex flex-col gap-6">
            <div className="flex flex-col gap-10">
              <div className="flex min-w-2xs flex-col gap-5">
                <TextBox
                  icon="person-icon"
                  label="Uni-ID"
                  value={uniIdInput ?? ""}
                  autofocus={true}
                  onChange={(text) => setUniIdInput(text.trim())}
                />
              </div>
              {sharedMessage && <>{messageComponent}</>}
            </div>
            <div className="flex flex-col self-center justify-center ">
              <NormalButton text={t("continue")} onClick={handleOTPRequest} isDisabled={isButtonDisabled} />
              <div className="flex flex-col gap-4">
                <NormalLink text={t("something-wrong-back")} onClick={() => setStepNr(1)} />
                <LanguageSwitch linkStyle={true} />
              </div>
            </div>
          </div>
        );
      case 3:
        return (
          <div className="flex flex-col gap-6">
            <div className="flex flex-col gap-10">
              <UnderlineText text={`${t("one-time-key-prompt")} ${uniIdInput}${import.meta.env.VITE_EMAIL_DOMAIN}`} />
              <div className="flex min-w-2xs flex-col gap-5 items-center">
                <TextBox
                  icon="pincode-icon"
                  label={t("one-time-key")}
                  placeHolder={t("for-example-abbr") + "123456"}
                  value={emailCode}
                  autofocus={true}
                  onChange={(text) => setEmailCode(text.trim())}
                />
                {sharedMessage && <>{messageComponent}</>}
              </div>
            </div>
            <div className="flex flex-col self-center justify-center ">
              <NormalButton
                text={t("continue")}
                onClick={handleOTPVerification}
                isDisabled={!RegexFilters.defaultId.test(emailCode) || isButtonDisabled}
              />
              <div className="flex flex-col gap-4">
                <NormalLink text={t("something-wrong-back")} onClick={() => setStepNr(2)} />
                <LanguageSwitch linkStyle={true} />
              </div>
            </div>
          </div>
        );
      case 4:
        return (
          <div className="flex flex-col gap-6">
            <div className="flex flex-col gap-10">
              <div className="flex min-w-2xs flex-col gap-5">
                <TextBox
                  icon="lock-icon"
                  label={t("password")}
                  isPassword
                  value={password}
                  autofocus={true}
                  onChange={(text) => setPassword(text.trim())}
                />
                <TextBox
                  icon="lock-icon"
                  label={t("repeat-password")}
                  isPassword
                  value={passwordAgain}
                  onChange={(text) => setPasswordAgain(text.trim())}
                />
              </div>
              {sharedMessage && <>{messageComponent}</>}
            </div>
            <div className="flex flex-col self-center justify-center ">
              <NormalButton
                text={t("continue")}
                onClick={() => setStepNr(5)}
                isDisabled={!isPasswordFormValid() || isButtonDisabled}
              />
              <div className="flex flex-col gap-4">
                <NormalLink text={t("something-wrong-back")} onClick={() => setStepNr(3)} />
                <LanguageSwitch linkStyle={true} />
              </div>
            </div>
          </div>
        );
      case 5:
        return (
          <>
            <div className="flex flex-col gap-5">
              <UnderlineText text={t("verify-details")} />
              <div className="flex flex-col gap-3 border-2 p-2 border-main-text rounded-2xl">
                <DataField fieldName={t("name")} data={fullName} />
                <DataField fieldName={"Uni-ID"} data={uniIdInput} />
              </div>
              {sharedMessage && <>{messageComponent}</>}
            </div>
            <div className="flex flex-col self-center justify-center ">
              <NormalButton text={t("create-account")} onClick={handleRegister} isDisabled={isButtonDisabled} />
              <div className="flex flex-col gap-4">
                <NormalLink text={t("something-wrong-back")} onClick={() => setStepNr(4)} />
                <LanguageSwitch linkStyle={true} />
              </div>
            </div>
          </>
        );
    }
  };

  return (
    <>
      <div className="max-h-screen max-w-screen flex items-center justify-center gap-10">
        <div className="flex flex-col md:p-20 max-md:p-10 items-center gap-20 bg-main-dark rounded-3xl">
          <img src="../assets/logo/white-on-transparent.png" className="md:w-xl" />
          {renderStep()}
        </div>
      </div>
    </>
  );
}
export default CreateAccountView;
