import React, { useState, useEffect } from "react";
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
import TextBox from "../layout/components/TextBox";
import NormalButton from "../layout/components/NormalButton";
import FormHeader from "../layout/headers/FormHeader";
import Greeting from "../layout/components/Greeting";
import NormalLink from "../layout/components/NormalLink";
import ErrorMessage from "../layout/components/ErrorMessage";
import NormalMessage from "../layout/components/NormalMessage";
import UnderlineText from "../layout/components/UnderlineText";
import NavigationProps from "../types";
import { RequestOTP, VerifyOTP, ChangeUserPassword } from "../businesslogic/services/UserDataOnline";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import KeyboardVisibilityHandler from "../businesslogic/hooks/KeyboardVisibilityHandler";
import Constants from "expo-constants";
import { preventScreenCaptureAsync, allowScreenCaptureAsync } from "expo-screen-capture";
import { RegexFilters } from "../businesslogic/helpers/RegexFilters";
import { ApplyStyles } from "../businesslogic/hooks/SelectAppTheme";
import { GetNativeSafeArea } from "../layout/styles/NativeStyles";

function ForgotPasswordView({ navigation, route }: NavigationProps) {
  const { localData } = route.params ?? {};
  const isNormalPassChange: boolean = route?.params?.isNormalPassChange ?? false;
  const [stepNr, setStepNr] = useState(1);
  const [uniId, setUniId] = useState("");
  const [emailCode, setEmailCode] = useState("");
  const [password, setPassword] = useState("");
  const [passwordAgain, setPasswordAgain] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [normalMessage, setNormalMessage] = useState<string | null>(null);
  const [isButtonDisabled, setIsButtonDisabled] = useState<boolean>(false);
  const { t } = useTranslation();
  const isKeyboardVisible = KeyboardVisibilityHandler();

  const { theme } = ApplyStyles();
  const safeAreaStyle = GetNativeSafeArea(theme);

  useEffect(() => {
    preventScreenCaptureAsync();
    return () => {
      allowScreenCaptureAsync();
    };
  }, []);
  const isStudentIDFormValid = () => uniId != "";
  const isPasswordFormValid = () => password.length >= 8 && password === passwordAgain;
  useEffect(() => {
    setNormalMessage(!isStudentIDFormValid() ? t("all-fields-required-message") : "");
  }, [uniId]);
  useEffect(() => {
    if (password && password.length < 8) {
      setNormalMessage(t("password-length-message"));
    } else if (password && passwordAgain && password !== passwordAgain) {
      setNormalMessage(t("password-match-message"));
    } else {
      setNormalMessage("");
    }
  }, [password, passwordAgain]);
  const handleOTPRequest = async () => {
    Keyboard.dismiss();
    setIsButtonDisabled(true);
    const status = await RequestOTP(uniId);
    setIsButtonDisabled(false);
    if (status === true) {
      setStepNr(2);
    } else {
      showError(String(status));
    }
  };
  const handleOTPVerification = async () => {
    Keyboard.dismiss();
    setIsButtonDisabled(true);
    const status = await VerifyOTP({ uniId, otp: emailCode });
    setIsButtonDisabled(false);
    if (status === true) {
      setStepNr(3);
    } else {
      showError(String(status));
    }
  };
  const handlePasswordChange = async () => {
    Keyboard.dismiss();
    setIsButtonDisabled(true);
    const status = await ChangeUserPassword({ uniId, newPassword: password });
    setIsButtonDisabled(false);
    if (status === true) {
      const successMessage = t("password-changed");
      const targetScreen = isNormalPassChange ? "SettingsView" : "LoginView";
      navigation.navigate(targetScreen, { localData, successMessage });
    } else {
      showError(String(status));
    }
  };
  const showError = (message: string) => {
    setErrorMessage(t(message));
    setTimeout(() => setErrorMessage(null), 2000);
  };

  const styles = StyleSheet.create({
    headerContainer: {
      flex: 1,
      justifyContent: "flex-end",
      gap: hp("4%"),
    },
    textBoxContainer: {
      flex: 1.5,
      gap: hp("2%"),
      justifyContent: "center",
    },
    textBoxes: {
      gap: hp("1%"),
      alignItems: "center",
    },
    errorContainer: {
      marginTop: hp("2%"),
    },
    buttonContainer: {
      flex: 2,
      gap: hp("1%"),
      justifyContent: "center",
      alignItems: "center",
    },
  });

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
            {!isKeyboardVisible && <Greeting text={isNormalPassChange ? t("change-password") : t("forgot-password")} />}
          </View>
          {stepNr === 1 && (
            <>
              <View style={styles.textBoxContainer}>
                <UnderlineText text={t("verify-account") + ": "} />
                <View style={styles.textBoxes}>
                  <TextBox
                    iconName="person-icon"
                    label="Uni-ID"
                    onChangeText={(text) => setUniId(text.trim())}
                    value={uniId}
                    autoCapitalize="none"
                  />
                </View>
                {!isKeyboardVisible && errorMessage && (
                  <View style={styles.errorContainer}>
                    <ErrorMessage text={errorMessage} />
                  </View>
                )}
              </View>
              <View style={styles.buttonContainer}>
                <NormalLink
                  text={isNormalPassChange ? t("dont-change-password") : t("still-remember-password")}
                  onPress={() =>
                    isNormalPassChange
                      ? navigation.navigate("SettingsView", { localData })
                      : navigation.navigate("LoginView")
                  }
                />
                <NormalButton
                  text={t("continue")}
                  onPress={handleOTPRequest}
                  disabled={!isStudentIDFormValid() || isButtonDisabled}
                />
              </View>
            </>
          )}
          {stepNr === 2 && (
            <>
              <View style={styles.textBoxContainer}>
                <UnderlineText
                  text={`${t("one-time-key-prompt")} ${uniId + Constants.expoConfig?.extra?.EXPO_PUBLIC_EMAILDOMAIN}`}
                />
                <View style={styles.textBoxes}>
                  <TextBox
                    iconName="pincode-icon"
                    label={`${t("one-time-key")}*`}
                    onChangeText={(text) => setEmailCode(text.trim())}
                    value={emailCode}
                  />
                </View>
                {!isKeyboardVisible && errorMessage && (
                  <View style={styles.errorContainer}>
                    <ErrorMessage text={errorMessage} />
                  </View>
                )}
              </View>
              <View style={styles.buttonContainer}>
                <NormalButton
                  text={t("continue")}
                  onPress={handleOTPVerification}
                  disabled={!RegexFilters.defaultId.test(emailCode) || isButtonDisabled}
                />
                {!isKeyboardVisible && <NormalLink text={t("something-wrong-back")} onPress={() => setStepNr(1)} />}
              </View>
            </>
          )}
          {stepNr === 3 && (
            <>
              <View style={styles.textBoxContainer}>
                {!isKeyboardVisible && <UnderlineText text={t("set-new-password")} />}
                <View style={styles.textBoxes}>
                  <TextBox
                    iconName="lock-icon"
                    label={t("password")}
                    isPassword
                    onChangeText={setPassword}
                    value={password}
                  />
                  <TextBox
                    iconName="lock-icon"
                    label={t("repeat-password")}
                    isPassword
                    onChangeText={setPasswordAgain}
                    value={passwordAgain}
                  />
                </View>
                {!isKeyboardVisible && normalMessage && (
                  <View style={styles.errorContainer}>
                    <NormalMessage text={normalMessage} />
                  </View>
                )}
                {!isKeyboardVisible && errorMessage && (
                  <View style={styles.errorContainer}>
                    <NormalMessage text={errorMessage} />
                  </View>
                )}
              </View>
              <View style={styles.buttonContainer}>
                <NormalButton
                  text={t("continue")}
                  onPress={handlePasswordChange}
                  disabled={!isPasswordFormValid() || isButtonDisabled}
                />
                {!isKeyboardVisible && <NormalLink text={t("something-wrong-back")} onPress={() => setStepNr(2)} />}
              </View>
            </>
          )}
        </SafeAreaView>
      </KeyboardAvoidingView>
    </TouchableWithoutFeedback>
  );
}

const styles = StyleSheet.create({
  headerContainer: {
    flex: 1,
    justifyContent: "flex-end",
    gap: hp("4%"),
  },
  textBoxContainer: {
    flex: 1.5,
    gap: hp("2%"),
    justifyContent: "center",
  },
  textBoxes: {
    gap: hp("1%"),
    alignItems: "center",
  },
  errorContainer: {
    marginTop: hp("2%"),
  },
  buttonContainer: {
    flex: 2,
    gap: hp("1%"),
    justifyContent: "center",
    alignItems: "center",
  },
});

export default ForgotPasswordView;
