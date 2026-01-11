import React, { useState, useEffect, useCallback } from "react";
import { SafeAreaView, StyleSheet, View, TouchableWithoutFeedback, Keyboard } from "react-native";
import { useTranslation } from "react-i18next";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import NavigationProps from "../types";
import NormalHeader from "../layout/headers/NormalHeader";
import NormalButton from "../layout/components/NormalButton";
import NormalLink from "../layout/components/NormalLink";
import DualSwitch from "../layout/components/DualSwitch";
import StepDivider from "../layout/components/StepDivider";
import QrGenerator from "../layout/components/QrGenerator";
import SuccessMessage from "../layout/components/SuccessMessage";
import ErrorMessage from "../layout/components/ErrorMessage";
import UnderlineText from "../layout/components/UnderlineText";
import { GenerateQrString } from "../businesslogic/services/QrGenLogic";
import { AddAttendanceCheck } from "../businesslogic/services/CourseAttendanceData";
import KeyboardVisibilityHandler from "../businesslogic/hooks/KeyboardVisibilityHandler";
import CreateAttendanceCheckModel from "../models/CreateAttendanceCheckModel";
import IconDataText from "../layout/components/DataText";
import { ApplyStyles } from "../businesslogic/hooks/SelectAppTheme";
import { GetNativeSafeArea } from "../layout/styles/NativeStyles";
import { IconContent } from "../layout/components/Icons";
import Icon from "../layout/components/Icon";

function CompleteAttendanceView({ navigation, route }: NavigationProps) {
  const { localData, attendanceId, workplaceId = null, stepNr: initialStep } = route.params;
  const { t } = useTranslation();
  const isKeyboardVisible = KeyboardVisibilityHandler();

  const { theme } = ApplyStyles();
  const safeAreaStyle = GetNativeSafeArea(theme);

  const [isOnline, setIsOnline] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isModeToggleInLeftPos, setIsModeToggleInLeftPos] = useState<boolean>(true);
  const [isButtonDisabled, setIsButtonDisabled] = useState<boolean>(false);
  const stepNr = initialStep + 1;
  const [qrValue, setQrValue] = useState(() =>
    GenerateQrString(localData.studentCode, localData.fullName, attendanceId, workplaceId ?? "000000")
  );
  useEffect(() => {
    const intervalId = setInterval(() => {
      setQrValue(GenerateQrString(localData.studentCode, localData.fullName, attendanceId, workplaceId ?? "000000"));
    }, 60000);
    return () => clearInterval(intervalId);
  }, [localData.studentCode, attendanceId, workplaceId]);

  const refreshQrCode = () => {
    setQrValue(GenerateQrString(localData.studentCode, localData.fullName, attendanceId, workplaceId ?? "000000"));
  };
  const navigateBack = useCallback(() => {
    navigation.navigate("StudentMainView", {
      localData,
      attendanceId,
      workplaceId,
      stepNr: stepNr - 1,
    });
  }, [navigation, localData, attendanceId, workplaceId, stepNr]);

  const handleAttendanceCheckAdd = async () => {
    setIsButtonDisabled(true);
    const attendanceCheck: CreateAttendanceCheckModel = {
      studentCode: localData.studentCode,
      fullName: localData.fullName,
      courseAttendanceId: attendanceId,
      workplaceId: parseInt(workplaceId) ?? null,
    };
    const status = await AddAttendanceCheck(attendanceCheck);
    setIsButtonDisabled(false);
    if (status === true) {
      setSuccessMessage(t("attendance-check-added"));
      setTimeout(() => {
        setSuccessMessage(null);
        navigation.navigate("StudentMainView", { localData });
      }, 2000);
    } else {
      setErrorMessage(t(String(status)));
      setTimeout(() => setErrorMessage(null), 2000);
    }
  };

  const stylesLocal = StyleSheet.create({
    headerContainer: {
      flex: 1,
      justifyContent: "center",
    },
    onlineToggleContainer: {
      flex: 1,
      justifyContent: "center",
    },
    stepDividerContainer: {
      flex: 0.5,
      alignItems: "center",
      justifyContent: "center",
    },
    qrContainer: {
      flex: 3,
      justifyContent: "flex-start",
      alignItems: "center",
    },
    dataContainer: {
      flex: 1,
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
    messageContainer: {
      flex: 1,
      alignItems: "center",
      justifyContent: "flex-end",
    },
    lowNavButtonContainer: {
      flex: 1,
      gap: 4,
      justifyContent: "flex-end",
      alignItems: "center",
    },
    underlineText: {
      marginTop: hp("5%"),
      marginBottom: hp("0.5%"),
    },
  });

  const renderSharedData = () => (
    <View style={stylesLocal.data}>
      <IconDataText label={t("name")} text={localData.fullName} />
      <IconDataText label={t("attendance-id")} text={attendanceId} />
      <IconDataText label={t("workplace-id")} text={workplaceId || t("not-available")} />
    </View>
  );

  return (
    <TouchableWithoutFeedback onPress={Keyboard.dismiss} accessible={false}>
      <SafeAreaView style={safeAreaStyle}>
        <View style={stylesLocal.headerContainer}>
          <NormalHeader navigation={navigation} route={route} />
        </View>
        <View style={stylesLocal.onlineToggleContainer}>
          <DualSwitch
            textLeft={t("offline-mode")}
            iconLeft={IconContent["offline-icon"]}
            textRight={t("online-mode")}
            iconRight={IconContent["online-icon"]}
            isLeftSelected={isModeToggleInLeftPos}
            isRightSelected={!isModeToggleInLeftPos}
            onPressLeft={() => {
              setIsOnline(false);
              setIsModeToggleInLeftPos(true);
            }}
            onPressRight={() => {
              setIsOnline(true);
              setIsModeToggleInLeftPos(false);
            }}
            isDisabled={localData.offlineOnly}
          />
        </View>
        <View style={stylesLocal.stepDividerContainer}>
          <StepDivider label={t(isOnline ? "step-end-attendance" : "step-show-qr")} stepNumber={stepNr} />
        </View>
        {isOnline ? (
          <>
            <View style={stylesLocal.qrContainer}>
              <View style={stylesLocal.underlineText}>
                <UnderlineText text={t("verify-details")} />
              </View>
              <View style={stylesLocal.dataContainer}>{renderSharedData()}</View>
              <View style={stylesLocal.messageContainer}>
                {successMessage && <SuccessMessage text={successMessage} />}
                {errorMessage && <ErrorMessage text={errorMessage} />}
              </View>
            </View>
            <View style={stylesLocal.lowNavButtonContainer}>
              <NormalLink text={t("something-wrong-back")} onPress={navigateBack} />
              <NormalButton text={t("check-in")} onPress={handleAttendanceCheckAdd} disabled={isButtonDisabled} />
            </View>
          </>
        ) : (
          <>
            {!isKeyboardVisible && (
              <View style={stylesLocal.qrContainer}>
                <QrGenerator value={qrValue} />
              </View>
            )}
            {renderSharedData()}
            <View style={stylesLocal.lowNavButtonContainer}>
              <NormalLink text={t("something-wrong-back")} onPress={navigateBack} />
              <NormalButton text={t("refresh-qr")} onPress={refreshQrCode} disabled={isButtonDisabled} />
            </View>
          </>
        )}
      </SafeAreaView>
    </TouchableWithoutFeedback>
  );
}

export default CompleteAttendanceView;
