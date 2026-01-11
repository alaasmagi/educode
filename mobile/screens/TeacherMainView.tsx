import React, { useEffect, useState } from "react";
import {
  SafeAreaView,
  StyleSheet,
  View,
  TouchableWithoutFeedback,
  Keyboard,
  KeyboardAvoidingView,
  Platform,
} from "react-native";
import { useTranslation } from "react-i18next";
import * as Haptics from "expo-haptics";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import NormalHeader from "../layout/headers/NormalHeader";
import NormalButton from "../layout/components/NormalButton";
import DualSwitch from "../layout/components/DualSwitch";
import QrScanner from "../layout/components/QrScanner";
import TextBox from "../layout/components/TextBox";
import KeyboardVisibilityHandler from "../businesslogic/hooks/KeyboardVisibilityHandler";
import { RegexFilters } from "../businesslogic/helpers/RegexFilters";
import SuccessMessage from "../layout/components/SuccessMessage";
import ErrorMessage from "../layout/components/ErrorMessage";
import NormalMessage from "../layout/components/NormalMessage";
import { AddAttendanceCheck, GetCurrentAttendance } from "../businesslogic/services/CourseAttendanceData";
import { CourseAttendance } from "../models/CourseAttendance";
import CreateAttendanceCheckModel from "../models/CreateAttendanceCheckModel";
import ToSixDigit from "../businesslogic/helpers/NumberConverter";
import BackButtonHandler from "../businesslogic/hooks/BackButtonHandler";
import DataText from "../layout/components/DataText";
import { ApplyStyles } from "../businesslogic/hooks/SelectAppTheme";
import { GetNativeSafeArea } from "../layout/styles/NativeStyles";
import { EQrStatus } from "../models/EQrStatus";
import { IconContent } from "../layout/components/Icons";

function TeacherMainView({ navigation, route }) {
  const { localData } = route.params;
  const { t } = useTranslation();
  const isKeyboardVisible = KeyboardVisibilityHandler();
  const [qrScanView, setQrScanView] = useState(true);
  const [scanned, setScanned] = useState(false);
  const [currentAttendanceData, setCurrentAttendanceData] = useState<CourseAttendance | null>(null);
  const [currentAttendancePlaceHolder, setCurrentAttendancePlaceHolder] = useState<string | null>(null);
  const [studentCode, setStudentCode] = useState("");
  const [fullName, setFullName] = useState("");
  const [workplaceId, setWorkplaceId] = useState("");
  const [lastAddedStudentCode, setLastAddedStudentCode] = useState("");
  const [lastAddedStudentWorkplaceId, setLastAddedStudentWorkplaceId] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [normalMessage, setNormalMessage] = useState<string | null>(null);
  const [isModeToggleInLeftPos, setIsModeToggleInLeftPos] = useState<boolean>(true);
  const [isButtonDisabled, setIsButtonDisabled] = useState<boolean>(false);
  const [scanStatus, setScanStatus] = useState<EQrStatus>(EQrStatus.Undefined);

  const { theme } = ApplyStyles();
  const safeAreaStyle = GetNativeSafeArea(theme);

  BackButtonHandler(navigation);

  const handleBarcodeScanned = async ({ data }: { data: string }) => {
    if (!scanned) {
      setScanned(true);
      await Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success);
      if (RegexFilters.attendanceCheckData.test(data)) {
        const attendanceCheckData = data.split(";");
        const timestamp = Math.floor(Date.now() / 1000);
        if (timestamp - parseInt(attendanceCheckData[0]) > 120) {
          setErrorMessage(t("timestamp-error"));
          setScanStatus(EQrStatus.Incorrect);
        } else {
          const model: CreateAttendanceCheckModel = {
            courseAttendanceId: parseInt(attendanceCheckData[1]),
            workplaceId: attendanceCheckData[2] === "000000" ? undefined : parseInt(attendanceCheckData[2]),
            fullName: attendanceCheckData[3],
            studentCode: attendanceCheckData[4],
          };
          const response = await AddAttendanceCheck(model);
          if (typeof response === "string") {
            setErrorMessage(t("student-already-registered"));
            setTimeout(() => setErrorMessage(null), 2000);
            setScanStatus(EQrStatus.Incorrect);
          } else {
            setSuccessMessage(`${t("attendance-check-added-for")}: ${attendanceCheckData[3]}`);
            setScanStatus(EQrStatus.Correct);
            setLastAddedStudentCode(attendanceCheckData[4]);
            setLastAddedStudentWorkplaceId(attendanceCheckData[2]);
            setTimeout(() => setSuccessMessage(null), 2000);
          }
        }
      } else {
        setErrorMessage(t("attendance-check-add-fail"));
        setScanStatus(EQrStatus.Incorrect);
      }
      setTimeout(() => setScanned(false), 2000);
      setTimeout(() => setErrorMessage(null), 2000);
      setTimeout(() => setSuccessMessage(null), 2000);
      clearScanStatus();
    }
  };

  const clearScanStatus = () => setTimeout(() => setScanStatus(EQrStatus.Undefined), 2000);

  const fetchCurrentAttendance = async () => {
    const status = await GetCurrentAttendance(localData.uniId);
    if (typeof status === "string") {
      setNormalMessage(status);
    } else {
      setCurrentAttendanceData(status);
    }
  };

  const isStudentCodeValid = () => RegexFilters.studentCode.test(studentCode);
  const isFullNameValid = () => fullName !== "" && !fullName.includes(";");
  const handleAddStudentManually = async () => {
    Keyboard.dismiss();
    setIsButtonDisabled(true);
    if (workplaceId !== "" && !RegexFilters.defaultId.test(workplaceId)) {
      setErrorMessage(t("wrong-studentcode"));
      setTimeout(() => setErrorMessage(null), 2000);
    }
    const model: CreateAttendanceCheckModel = {
      studentCode: studentCode.toUpperCase(),
      fullName: fullName.trim(),
      courseAttendanceId: Number(currentAttendanceData!.attendanceId),
      workplaceId: parseInt(workplaceId) ?? null,
    };
    const response = await AddAttendanceCheck(model);
    setIsButtonDisabled(false);
    if (typeof response === "string") {
      setErrorMessage(t("student-already-registered"));
      setTimeout(() => setErrorMessage(null), 2000);
    } else {
      setSuccessMessage(`${t("attendance-check-added-for")}:  ${fullName}`);
      setLastAddedStudentCode(studentCode);
      setLastAddedStudentWorkplaceId(workplaceId);
      setFullName("");
      setStudentCode("");
      setWorkplaceId("");
      setTimeout(() => setSuccessMessage(null), 2000);
    }
  };

  useEffect(() => {
    fetchCurrentAttendance();
    const interval = setInterval(() => fetchCurrentAttendance(), 120000);
    return () => clearInterval(interval);
  }, []);

  const styles = StyleSheet.create({
    headerContainer: {
      flex: 0.8,
      justifyContent: "center",
    },
    onlineToggleContainer: {
      flex: 0.8,
      justifyContent: "center",
    },
    currentAttendanceContainer: {
      flex: 0.5,
      justifyContent: "center",
      marginBottom: 10,
    },
    qrScannerContainer: {
      flex: 2,
      justifyContent: "center",
      alignItems: "center",
    },
    textBoxes: {
      width: wp("90%"),
      justifyContent: "center",
      alignItems: "center",
      gap: hp("2%"),
    },
    manualInputContainer: {
      flex: 2,
      gap: hp("3%"),
      justifyContent: "flex-end",
      alignItems: "center",
    },
    messageContainer: {
      flex: 1,
      justifyContent: "flex-start",
      marginTop: hp("2%"),
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
            <NormalHeader navigation={navigation} route={route} />
          </View>
          {!isKeyboardVisible && (
            <View style={styles.onlineToggleContainer}>
              <DualSwitch
                textLeft={t("scan-student")}
                iconLeft={IconContent["qr-scan-icon"]}
                textRight={t("add-manually")}
                iconRight={IconContent["passcode-icon"]}
                isLeftSelected={isModeToggleInLeftPos}
                isRightSelected={!isModeToggleInLeftPos}
                onPressLeft={() => {
                  setQrScanView(true);
                  setIsModeToggleInLeftPos(true);
                }}
                onPressRight={() => {
                  setQrScanView(false);
                  setIsModeToggleInLeftPos(false);
                }}
                isDisabled={!currentAttendanceData}
              />
            </View>
          )}
          {!isKeyboardVisible && (
            <View style={styles.currentAttendanceContainer}>
              {currentAttendanceData ? (
                <View style={styles.data}>
                  <DataText label={t("course-name")} text={currentAttendanceData.courseName} />
                  <DataText label={t("course-code")} text={currentAttendanceData.courseCode} />
                  <DataText label={t("attendance-id")} text={ToSixDigit(Number(currentAttendanceData.attendanceId))} />
                </View>
              ) : (
                <>
                  <NormalMessage text={t(String(normalMessage))} />
                </>
              )}
            </View>
          )}
          {qrScanView ? (
            <>
              <View style={styles.qrScannerContainer}>
                {currentAttendanceData && <QrScanner onQrScanned={handleBarcodeScanned} qrStatus={scanStatus} />}
              </View>
              <View style={styles.messageContainer}>
                {successMessage && !isKeyboardVisible && <SuccessMessage text={successMessage} />}
                {errorMessage && !isKeyboardVisible && <ErrorMessage text={errorMessage} />}
              </View>
            </>
          ) : (
            <>
              <View style={styles.messageContainer}>
                {successMessage && !isKeyboardVisible && <SuccessMessage text={successMessage} />}
                {errorMessage && !isKeyboardVisible && <ErrorMessage text={errorMessage} />}
              </View>
              <View style={styles.manualInputContainer}>
                <View style={styles.textBoxes}>
                  <TextBox
                    iconName="person-icon"
                    label={`${t("name")}`}
                    value={fullName}
                    placeHolder={t("for-example-abbr") + " Andres Tamm"}
                    onChangeText={setFullName}
                    autoCapitalize="words"
                  />
                  <TextBox
                    iconName="person-icon"
                    label={`${t("student-code")}`}
                    value={studentCode}
                    placeHolder={t("for-example-abbr") + " 123456ABCD"}
                    onChangeText={(text) => setStudentCode(text.trim())}
                    autoCapitalize="characters"
                  />
                  <TextBox
                    iconName="work-icon"
                    label={t("workplace-id")}
                    placeHolder={t("for-example-abbr") + " 123456"}
                    value={workplaceId}
                    onChangeText={(text) => setWorkplaceId(text.trim())}
                  />
                </View>
                <NormalButton
                  text={t("add-manually")}
                  onPress={handleAddStudentManually}
                  disabled={!isStudentCodeValid() || !isFullNameValid() || isButtonDisabled}
                />
              </View>
            </>
          )}
        </SafeAreaView>
      </KeyboardAvoidingView>
    </TouchableWithoutFeedback>
  );
}

export default TeacherMainView;
