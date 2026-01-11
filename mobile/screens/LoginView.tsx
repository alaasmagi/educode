import React, { useState, useEffect } from "react";
import { Alert, View, StyleSheet, Keyboard } from "react-native";
import { useTranslation } from "react-i18next";
import { useCameraPermissions } from "expo-camera";
import { preventScreenCaptureAsync, allowScreenCaptureAsync } from "expo-screen-capture";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import NavigationProps from "../types";
import FormHeader from "../layout/headers/FormHeader";
import Greeting from "../layout/components/Greeting";
import TextBox from "../layout/components/TextBox";
import NormalButton from "../layout/components/NormalButton";
import NormalLink from "../layout/components/NormalLink";
import ErrorMessage from "../layout/components/ErrorMessage";
import SuccessMessage from "../layout/components/SuccessMessage";
import KeyboardVisibilityHandler from "../businesslogic/hooks/KeyboardVisibilityHandler";
import { UserLogin, FetchAndSaveUserDataByUniId } from "../businesslogic/services/UserDataOnline";
import { GetOfflineUserData } from "../businesslogic/services/UserDataOffline";
import { ScreenContainer } from "../layout/containers/ScreenContainer";
import { ApplyStyles } from "../businesslogic/hooks/SelectAppTheme";
import { GetNativeSafeArea } from "../layout/styles/NativeStyles";

function LoginView({ navigation, route }: NavigationProps) {
  const { t } = useTranslation();
  const [permission, requestPermission] = useCameraPermissions();
  const [uniId, setUniId] = useState("");
  const [password, setPassword] = useState("");
  const [successMessage, setSuccessMessage] = useState<string | null>(route?.params?.successMessage || null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const isKeyboardVisible = KeyboardVisibilityHandler();
  const [isButtonDisabled, setIsButtonDisabled] = useState<boolean>(false);
  const isFormValid = uniId !== "" && password !== "";

  const { theme } = ApplyStyles();
  const safeAreaStyle = GetNativeSafeArea(theme);

  useEffect(() => {
    preventScreenCaptureAsync();
    const timeout = setTimeout(() => setSuccessMessage(null), 2000);
    return () => {
      clearTimeout(timeout);
      allowScreenCaptureAsync();
    };
  }, []);

  const showTemporaryMessage = (type: "error" | "success", message: string) => {
    type === "error" ? setErrorMessage(message) : setSuccessMessage(message);
    setTimeout(() => {
      type === "error" ? setErrorMessage(null) : setSuccessMessage(null);
    }, 2000);
  };

  const checkPermissions = async (): Promise<boolean> => {
    if (permission?.granted) return true;
    const response = await requestPermission();
    if (!response.granted) {
      Alert.alert(t("camera-permission-denied"), t("camera-permission-denied-message"));
      return false;
    }
    return true;
  };

  const handleLogin = async () => {
    Keyboard.dismiss();
    setIsButtonDisabled(true);
    if (!(await checkPermissions())) {
      setIsButtonDisabled(false);
      return;
    }
    const loginStatus = await UserLogin(uniId.trim(), password.trim());
    setIsButtonDisabled(false);
    if (loginStatus === true) {
      const fetchDataStatus = await FetchAndSaveUserDataByUniId(uniId.trim());
      if (fetchDataStatus === true) {
        const localData = await GetOfflineUserData();
        if (localData) {
          const nextView = localData.userType === "Teacher" ? "TeacherMainView" : "StudentMainView";
          navigation.navigate(nextView, { localData });
        }
      } else {
        showTemporaryMessage("error", t(String(fetchDataStatus)));
      }
    } else {
      showTemporaryMessage("error", t(String(loginStatus)));
    }
  };

  const styles = StyleSheet.create({
    greetingContainer: {
      marginBottom: hp("4%"),
    },
    scrollViewContent: {
      justifyContent: "space-between",
    },
    textBoxContainer: {
      justifyContent: "center",
    },
    textBoxes: {
      alignItems: "center",
      gap: hp("2%"),
    },
    forgotPasswordContainer: {
      flexDirection: "row",
      justifyContent: "flex-end",
      width: wp("85%"),
    },
    messageContainer: {
      alignItems: "center",
      minHeight: hp("6%"),
      marginVertical: hp("0.5%"),
    },
    buttonContainer: {
      justifyContent: "center",
      alignItems: "center",
      gap: hp("1%"),
    },
  });

  return (
    <ScreenContainer
      header={<FormHeader />}
      scroll
      dismissKeyboardOnPress
      safeAreaStyle={safeAreaStyle}
      contentContainerStyle={styles.scrollViewContent}
    >
      {!isKeyboardVisible && (
        <View style={styles.greetingContainer}>
          <Greeting text={t("hello-again")} />
        </View>
      )}
      <View style={styles.textBoxContainer}>
        <View style={styles.textBoxes}>
          <TextBox iconName="person-icon" label="Uni-ID" onChangeText={setUniId} value={uniId} autoCapitalize="none" />
          <TextBox iconName="lock-icon" label={t("password")} isPassword onChangeText={setPassword} value={password} />
        </View>
        <View style={styles.forgotPasswordContainer}>
          <NormalLink text={t("forgot-password")} onPress={() => navigation.navigate("ForgotPasswordView")} />
        </View>
        {!isKeyboardVisible && (<View style={styles.messageContainer}>
          {!isKeyboardVisible && errorMessage && <ErrorMessage text={errorMessage} />}
          {!isKeyboardVisible && successMessage && <SuccessMessage text={successMessage} />}
        </View>)}
      </View>
      <View style={styles.buttonContainer}>
        <NormalButton text={t("log-in")} onPress={handleLogin} disabled={!isFormValid || isButtonDisabled} />
        <NormalLink text={t("register-now")} onPress={() => navigation.navigate("CreateAccountView")} />
      </View>
    </ScreenContainer>
  );
}

export default LoginView;
