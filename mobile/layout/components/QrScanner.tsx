import React, { useEffect, useRef, useState } from "react";
import { StyleSheet, View, Text, TouchableOpacity, Animated } from "react-native";
import { CameraView } from "expo-camera";
import { IconContent } from "./Icons";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import { EQrStatus } from "../../models/EQrStatus";
import Icon from "./Icon";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme";
import { OverallUiStyles } from "../styles/Styles";

interface QrScannerProperties {
  onQrScanned: (event: { data: string }) => void;
  qrStatus: EQrStatus;
}

const QrScanner: React.FC<QrScannerProperties> = ({ onQrScanned, qrStatus }) => {
  const { styles } = ApplyStyles();
  const [zoom, setZoom] = useState<number>(0.25);
  const shakeAnim = useRef(new Animated.Value(0)).current;

  useEffect(() => {
    if (qrStatus === EQrStatus.Incorrect) {
      shakeAnim.setValue(0);
      Animated.sequence([
        Animated.timing(shakeAnim, { toValue: 10, duration: 40, useNativeDriver: true }),
        Animated.timing(shakeAnim, { toValue: -10, duration: 40, useNativeDriver: true }),
        Animated.timing(shakeAnim, { toValue: 6, duration: 40, useNativeDriver: true }),
        Animated.timing(shakeAnim, { toValue: -6, duration: 40, useNativeDriver: true }),
        Animated.timing(shakeAnim, { toValue: 0, duration: 40, useNativeDriver: true }),
      ]).start();
    }
  }, [qrStatus]);

  const sheet = StyleSheet.create({
    animatedViewContainer: {
      height: hp("32%"),
      width: hp("32%"),
    },
    camera: {
      aspectRatio: 1,
      borderRadius: styles["qr-scanner-camera-border-radius"],
      borderWidth: styles["qr-scanner-camera-border-width"],
    },
    text: {
      color: styles["qr-scanner-font-color"],
      fontSize: styles["qr-scanner-font-size"],
      fontFamily: OverallUiStyles["default-heading-font-family"],
      paddingHorizontal: wp("1%"),
      paddingVertical: hp("0.5%"),
    },
    container: {
      paddingLeft: wp("9%"),
      flexDirection: "row",
      gap: wp("1%"),
    },
    sideContainer: {
      justifyContent: "center",
      gap: hp("3%"),
    },
    buttonsContainer: {
      gap: hp("1%"),
    },
  });

  const borderColor =
    qrStatus === EQrStatus.Incorrect
      ? styles["qr-scanner-camera-border-alert-color"]
      : qrStatus === EQrStatus.Correct
      ? styles["qr-scanner-camera-border-success-color"]
      : styles["qr-scanner-camera-border-normal-color"];

  return (
    <View style={sheet.container}>
      <Animated.View style={[sheet.animatedViewContainer, { transform: [{ translateX: shakeAnim }] }]}>
        <CameraView
          zoom={zoom}
          style={[
            sheet.camera,
            { borderColor },
          ]}
          onBarcodeScanned={onQrScanned}
        />
      </Animated.View>
      <View style={sheet.sideContainer}>
        <Icon
          size={styles["qr-scanner-icon-size"]}
          color={styles["qr-scanner-icon-color"]}
          iconContent={IconContent["zoom-icon"]}
          strokeWidth={styles["qr-scanner-icon-thickness"]}
        />
        <View style={sheet.buttonsContainer}>
          <TouchableOpacity onPress={() => setZoom(0.25)}>
            <Text style={sheet.text}>1x</Text>
          </TouchableOpacity>
          <TouchableOpacity onPress={() => setZoom(0.5)}>
            <Text style={sheet.text}>2x</Text>
          </TouchableOpacity>
        </View>
      </View>
    </View>
  );
};

export default QrScanner;