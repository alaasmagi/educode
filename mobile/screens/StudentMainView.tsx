import React, { useEffect, useState } from "react";
import { Keyboard, View } from "react-native";
import * as Haptics from "expo-haptics";
import { useTranslation } from "react-i18next";
import NavigationProps from "../types";
import SeparatorLine from "../layout/components/SeparatorLine";
import TextBox from "../layout/components/TextBox";
import QrScanner from "../layout/components/QrScanner";
import NormalHeader from "../layout/headers/NormalHeader";
import NormalButton from "../layout/components/NormalButton";
import StepDivider from "../layout/components/StepDivider";
import Checkbox from "../layout/components/Checkbox";
import NormalLink from "../layout/components/NormalLink";
import ErrorMessage from "../layout/components/ErrorMessage";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import { RegexFilters } from "../businesslogic/helpers/RegexFilters";
import KeyboardVisibilityHandler from "../businesslogic/hooks/KeyboardVisibilityHandler";
import BackButtonHandler from "../businesslogic/hooks/BackButtonHandler";
import ToSixDigit from "../businesslogic/helpers/NumberConverter";
import GetSixDigitTimeStamp from "../businesslogic/helpers/TimeStamp";
import { EQrStatus } from "../models/EQrStatus";
import { ScreenContainer } from "../layout/containers/ScreenContainer";
import { ApplyStyles } from "../businesslogic/hooks/SelectAppTheme";
import { GetNativeSafeArea } from "../layout/styles/NativeStyles";

import { StyleSheet } from "react-native";

function StudentMainView({ navigation, route }: NavigationProps) {
  const { t } = useTranslation();
  const isKeyboardVisible = KeyboardVisibilityHandler();
  BackButtonHandler(navigation);
  const {
    localData,
    attendanceId: routeAttendanceId,
    workplaceId: routeWorkplaceId,
    stepNr: initialStepNr = 1,
  } = route.params || {};

  const { theme } = ApplyStyles();
  const safeAreaStyle = GetNativeSafeArea(theme);

  const [stepNr, setStepNr] = useState(initialStepNr);
  const [scanned, setScanned] = useState(false);
  const [attendanceId, setAttendanceId] = useState(routeAttendanceId || "");
  const [scannedAttendanceData, setScannedAttendanceData] = useState("");
  const [workplaceId, setWorkplaceId] = useState(routeWorkplaceId || "");
  const [scanForWorkplace, setScanForWorkplace] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [scanStatus, setScanStatus] = useState<EQrStatus>(EQrStatus.Undefined);

  const clearErrorMessage = () => setTimeout(() => setErrorMessage(null), 2000);
  const clearScanStatus = () => setTimeout(() => setScanStatus(EQrStatus.Undefined), 2000);

  const isTimestampValid = (timestampStr: string): boolean => {
    const scannedTime = parseInt(timestampStr);
    const currentTime = GetSixDigitTimeStamp();
    return currentTime - scannedTime <= 20;
  };

  const handleQrScanned = async ({ data }: { data: string }) => {
    if (scanned) return;
    setScanned(true);
    await Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success);
    if (stepNr === 1 && RegexFilters.attendanceScanId.test(data)) {
      const [idPart, timestampPart] = data.split("-");
      if (!isTimestampValid(timestampPart)) {
        setErrorMessage(t("timestamp-error"));
        setScanStatus(EQrStatus.Incorrect);
        clearErrorMessage();
        clearScanStatus();
      } else {
        setScanStatus(EQrStatus.Correct);
        clearScanStatus();
        setScannedAttendanceData(data);
        setAttendanceId(ToSixDigit(Number(idPart)));
      }
    } else if (stepNr === 2 && RegexFilters.defaultId.test(data)) {
      setWorkplaceId(ToSixDigit(Number(data)));
      setScanStatus(EQrStatus.Correct);
      clearScanStatus();
    } else {
      setErrorMessage(t("invalid-qr"));
      setScanStatus(EQrStatus.Incorrect);
      clearErrorMessage();
      clearScanStatus();
    }
    setTimeout(() => setScanned(false), 2000);
  };

  const handleNextStep = () => {
    const [, timestampPart] = scannedAttendanceData.split("-");
    if (!isTimestampValid(timestampPart)) {
      setErrorMessage(t("timestamp-error"));
      clearErrorMessage();
      return;
    }
    if (scanForWorkplace) {
      setStepNr(2);
      setScanForWorkplace(false);
      Keyboard.dismiss();
    } else {
      navigation.navigate("CompleteAttendanceView", {
        localData,
        attendanceId,
        stepNr,
      });
    }
  };

  const handleDataSubmit = () => {
    navigation.navigate("CompleteAttendanceView", {
      localData,
      attendanceId,
      workplaceId,
      stepNr,
    });
  };

  const stylesLocal = StyleSheet.create({
    scrollViewContent: {
      justifyContent: "space-between",
    },
    stepDividerContainer: {
      marginVertical: hp("1%"),
    },
    qrContainer: {
      marginVertical: hp("1%"),
    },
    manualInputContainer: {
      marginVertical: hp("1%"),
      gap: hp("1%"),
    },
    additionalFieldContainer: {
      alignItems: "center",
      justifyContent: "center",
    },
    lowNavButtonContainer: {
      alignItems: "center",
    },
    linkContainer: {
      alignItems: "center",
      justifyContent: "flex-end",
    },
  });

  return (
    <ScreenContainer
      header={<NormalHeader navigation={navigation} route={route} />}
      scroll
      dismissKeyboardOnPress
      safeAreaStyle={safeAreaStyle}
      contentContainerStyle={stylesLocal.scrollViewContent}
    >
      {!isKeyboardVisible && (
        <View style={stylesLocal.stepDividerContainer}>
          <StepDivider stepNumber={stepNr} label={stepNr === 1 ? t("step-scan-board") : t("step-scan-workplace")} />
        </View>
      )}
      {!isKeyboardVisible && (
        <View style={stylesLocal.qrContainer}>
          <QrScanner onQrScanned={handleQrScanned} qrStatus={scanStatus} />
        </View>
      )}
      {stepNr === 1 ? (
        <View style={stylesLocal.manualInputContainer}>
          <SeparatorLine text={t("or-enter-id-manually")} />
          <TextBox
            iconName="key-icon"
            label={t("attendance-id")}
            placeHolder={t("for-example-abbr") + " 123456-123456"}
            value={scannedAttendanceData}
            onChangeText={(text) => setScannedAttendanceData(text.trim())}
          />
          <View style={stylesLocal.additionalFieldContainer}>
            {errorMessage ? (
              <ErrorMessage text={errorMessage} />
            ) : (
              <Checkbox
                label={t("add-workplace")}
                checked={scanForWorkplace}
                onChange={() => setScanForWorkplace((prev) => !prev)}
              />
            )}
          </View>
          <View style={stylesLocal.lowNavButtonContainer}>
            <NormalButton
              text={t("continue")}
              onPress={handleNextStep}
              disabled={!RegexFilters.attendanceScanId.test(scannedAttendanceData)}
            />
          </View>
        </View>
      ) : (
        <View style={stylesLocal.manualInputContainer}>
          <SeparatorLine text={t("or-enter-id-manually")} />
          <TextBox
            iconName="work-icon"
            label={t("workplace-id")}
            placeHolder={t("for-example-abbr") + " 123456"}
            value={workplaceId}
            onChangeText={(text) => setWorkplaceId(text.trim())}
          />
          <View style={stylesLocal.additionalFieldContainer}>
            {errorMessage && <ErrorMessage text={errorMessage} />}
          </View>
          <View style={stylesLocal.lowNavButtonContainer}>
            <NormalLink text={t("something-wrong-back")} onPress={() => setStepNr(1)} />
            <NormalButton
              text={t("continue")}
              disabled={!RegexFilters.defaultId.test(workplaceId)}
              onPress={handleDataSubmit}
            />
          </View>
        </View>
      )}
    </ScreenContainer>
  );
}

export default StudentMainView;
