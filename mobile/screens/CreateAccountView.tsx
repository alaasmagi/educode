import React, { useState, useEffect, useCallback } from "react";
import {
  SafeAreaView,
  StyleSheet,
  View,
  Keyboard,
  TouchableWithoutFeedback,
  KeyboardAvoidingView,
  Platform,
} from "react-native";
import { useTranslation } from "react-i18next";
import NavigationProps from "../types";
import { preventScreenCaptureAsync, allowScreenCaptureAsync } from "expo-screen-capture";
import KeyboardVisibilityHandler from "../businesslogic/hooks/KeyboardVisibilityHandler";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import TextBox from "../layout/components/TextBox";
import NormalButton from "../layout/components/NormalButton";
import Greeting from "../layout/components/Greeting";
import NormalLink from "../layout/components/NormalLink";
import NormalMessage from "../layout/components/NormalMessage";
import DataText from "../layout/components/DataText";
import UnderlineText from "../layout/components/UnderlineText";
import ErrorMessage from "../layout/components/ErrorMessage";
import FormHeader from "../layout/headers/FormHeader";
import { CreateUserAccount, RequestOTP, VerifyOTP } from "../businesslogic/services/UserDataOnline";
import CreateUserModel from "../models/CreateUserModel";
import VerifyOTPModel from "../models/VerifyOTPModel";
import { RegexFilters } from "../businesslogic/helpers/RegexFilters";
import Constants from "expo-constants";
import { ApplyStyles } from "../businesslogic/hooks/SelectAppTheme";
import { GetNativeSafeArea } from "../layout/styles/NativeStyles";

function CreateAccountView({ navigation }: NavigationProps) {
  const { t } = useTranslation();
  const [stepNr, setStepNr] = useState(1);
  const [fullName, setFullName] = useState("");
  const [uniId, setUniId] = useState("");
  const [studentCode, setStudentCode] = useState("");
  const [emailCode, setEmailCode] = useState("");
  const [password, setPassword] = useState("");
  const [passwordAgain, setPasswordAgain] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [normalMessage, setNormalMessage] = useState<string | null>(null);
  const [isButtonDisabled, setIsButtonDisabled] = useState<boolean>(false);
  const isKeyboardVisible = KeyboardVisibilityHandler();

  const { theme } = ApplyStyles();
  const safeAreaStyle = GetNativeSafeArea(theme);

  useEffect(() => {
    preventScreenCaptureAsync();
    return () => {
      allowScreenCaptureAsync();
    };
  }, []);
  const showTemporaryError = useCallback((message: string) => {
    setErrorMessage(message);
    const timeout = setTimeout(() => setErrorMessage(null), 2000);
    return () => clearTimeout(timeout);
  }, []);
  const isNameFormValid = () => fullName !== "" && !fullName.includes(";");
  const isStudentIDFormValid = () => uniId !== "" && RegexFilters.studentCode.test(studentCode);
  const isPasswordFormValid = () => password.length >= 8 && password === passwordAgain;
  useEffect(() => {
    setNormalMessage(!isNameFormValid() ? t("all-fields-required-message") : "");
  }, [fullName]);
  useEffect(() => {
    setNormalMessage(!isStudentIDFormValid() ? t("all-fields-required-message") : "");
  }, [uniId, studentCode]);
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
    Keyboard.dismiss();
    setIsButtonDisabled(true);
    const status = await RequestOTP(uniId, fullName);
    setIsButtonDisabled(false);
    if (status === true) {
      setStepNr(3);
    } else {
      showTemporaryError(t(String(status)));
    }
  }, [uniId, fullName, t, showTemporaryError]);
  const handleOTPVerification = useCallback(async () => {
    Keyboard.dismiss();
    setIsButtonDisabled(true);
    const otpData: VerifyOTPModel = { uniId, otp: emailCode };
    const status = await VerifyOTP(otpData);
    setIsButtonDisabled(false);
    if (status === true) {
      setStepNr(4);
    } else {
      showTemporaryError(t(String(status)));
    }
  }, [uniId, emailCode, t, showTemporaryError]);
  const handleRegister = useCallback(async () => {
    Keyboard.dismiss();
    setIsButtonDisabled(true);
    const userData: CreateUserModel = {
      uniId,
      studentCode: studentCode.toUpperCase(),
      fullName: fullName.trim(),
      password: password.trim(),
    };
    const status = await CreateUserAccount(userData);
    setIsButtonDisabled(false);
    if (status === true) {
      navigation.navigate("LoginView", { successMessage: t("create-account-success") });
    } else {
      showTemporaryError(t(String(status)));
    }
  }, [uniId, studentCode, fullName, password, navigation, t, showTemporaryError]);

  const styles = StyleSheet.create({
    headerContainer: {
      flex: 1.2,
      justifyContent: "flex-end",
      gap: hp("4%"),
    },
    textBoxContainer: {
      flex: 1.5,
      gap: hp("2%"),
      justifyContent: "center",
    },
    data: {
      alignSelf: "center",
      width: wp("90%"),
      borderWidth: 2,
      borderColor: theme["lightgray-gray"],
      borderRadius: 20,
      gap: hp("0.2%"),
      padding: 10,
    },
    textBoxes: {
      gap: hp("1%"),
      alignItems: "center",
    },
    messageContainer: {
      marginVertical: hp("2%"),
    },
    buttonContainer: {
      flex: 1.5,
      gap: hp("1%"),
      justifyContent: "center",
      alignItems: "center",
    },
  });

  const renderStep = () => {
    const sharedMessage = !isKeyboardVisible && (normalMessage || errorMessage);
    const messageComponent = errorMessage ? (
      <ErrorMessage text={errorMessage} />
    ) : (
      <NormalMessage text={normalMessage ?? ""} />
    );
    switch (stepNr) {
      case 1:
        return (
          <>
            <View style={styles.textBoxContainer}>
              <View style={styles.textBoxes}>
                <TextBox
                  iconName="person-icon"
                  label={`${t("name")}`}
                  value={fullName}
                  placeHolder={t("for-example-abbr") + " Andres Tamm"}
                  onChangeText={setFullName}
                  autoCapitalize="words"
                />
              </View>
              <View style={styles.messageContainer}>{sharedMessage && messageComponent}</View>
            </View>
            <View style={styles.buttonContainer}>
              <NormalButton
                text={t("continue")}
                onPress={() => setStepNr(2)}
                disabled={!isNameFormValid() || isButtonDisabled}
              />
              <NormalLink text={t("already-registered")} onPress={() => navigation.navigate("LoginView")} />
            </View>
          </>
        );
      case 2:
        return (
          <>
            <View style={styles.textBoxContainer}>
              <View style={styles.textBoxes}>
                <TextBox
                  iconName="person-icon"
                  label="Uni-ID"
                  autoCapitalize="none"
                  placeHolder={t("for-example-abbr") + " abcdef"}
                  value={uniId}
                  onChangeText={(text) => setUniId(text.trim())}
                />
                <TextBox
                  iconName="person-icon"
                  label={t("student-code")}
                  placeHolder={t("for-example-abbr") + " 123456ABCD"}
                  autoCapitalize="characters"
                  value={studentCode}
                  onChangeText={(text) => setStudentCode(text.trim())}
                />
              </View>
              <View style={styles.messageContainer}>{sharedMessage && messageComponent}</View>
            </View>
            <View style={styles.buttonContainer}>
              <NormalLink text={t("something-wrong-back")} onPress={() => setStepNr(1)} />
              <NormalButton
                text={t("continue")}
                onPress={handleOTPRequest}
                disabled={!isStudentIDFormValid() || isButtonDisabled}
              />
              <NormalLink text={t("already-registered")} onPress={() => navigation.navigate("LoginView")} />
            </View>
          </>
        );
      case 3:
        return (
          <>
            <View style={styles.textBoxContainer}>
              <UnderlineText
                text={`${t("one-time-key-prompt")} ${uniId + Constants.expoConfig?.extra?.EXPO_PUBLIC_EMAILDOMAIN}`}
              />
              <View style={styles.textBoxes}>
                <TextBox
                  iconName="passcode-icon"
                  label={t("one-time-key")}
                  value={emailCode}
                  placeHolder={t("for-example-abbr") + " 123456"}
                  onChangeText={(text) => setEmailCode(text.trim())}
                />
              </View>
              <View style={styles.messageContainer}>{sharedMessage && messageComponent}</View>
            </View>
            <View style={styles.buttonContainer}>
              {!isKeyboardVisible && <NormalLink text={t("something-wrong-back")} onPress={() => setStepNr(2)} />}
              <NormalButton
                text={t("continue")}
                onPress={handleOTPVerification}
                disabled={!RegexFilters.defaultId.test(emailCode) || isButtonDisabled}
              />
            </View>
          </>
        );
      case 4:
        return (
          <>
            <View style={styles.textBoxContainer}>
              <View style={styles.textBoxes}>
                <TextBox
                  iconName="lock-icon"
                  label={t("password")}
                  isPassword
                  value={password}
                  onChangeText={setPassword}
                />
                <TextBox
                  iconName="lock-icon"
                  label={t("repeat-password")}
                  isPassword
                  value={passwordAgain}
                  onChangeText={setPasswordAgain}
                />
              </View>
              <View style={styles.messageContainer}>{sharedMessage && messageComponent}</View>
            </View>
            <View style={styles.buttonContainer}>
              <NormalLink text={t("something-wrong-back")} onPress={() => setStepNr(3)} />
              <NormalButton
                text={t("continue")}
                onPress={() => setStepNr(5)}
                disabled={!isPasswordFormValid() || isButtonDisabled}
              />
              <NormalLink text={t("already-registered")} onPress={() => navigation.navigate("LoginView")} />
            </View>
          </>
        );
      case 5:
        return (
          <>
            <View style={styles.textBoxContainer}>
              <UnderlineText text={t("verify-details")} />
              <View style={styles.data}>
                <DataText label={t("name")} text={fullName} />
                <DataText label={"Uni-ID"} text={uniId} />
                <DataText label={t("student-code")} text={studentCode.toUpperCase()} />
              </View>
              <View style={styles.messageContainer}>{sharedMessage && messageComponent}</View>
            </View>
            <View style={styles.buttonContainer}>
              <NormalLink text={t("something-wrong-back")} onPress={() => setStepNr(4)} />
              <NormalButton text={t("create-account")} onPress={handleRegister} disabled={isButtonDisabled} />
              <NormalLink text={t("already-registered")} onPress={() => navigation.navigate("LoginView")} />
            </View>
          </>
        );
    }
  };
  return (
    <TouchableWithoutFeedback onPress={Keyboard.dismiss} accessible={false}>
      <KeyboardAvoidingView
        behavior={Platform.OS === "ios" ? "padding" : "height"}
        style={{ flex: 1 }}
        keyboardVerticalOffset={Platform.OS === "ios" ? 100 : -hp("9%")}
      >
        <SafeAreaView style={safeAreaStyle}>
          <View style={styles.headerContainer}>
            <FormHeader />
            {!isKeyboardVisible && <Greeting text={t("welcome")} />}
          </View>
          {renderStep()}
        </SafeAreaView>
      </KeyboardAvoidingView>
    </TouchableWithoutFeedback>
  );
}

export default CreateAccountView;
