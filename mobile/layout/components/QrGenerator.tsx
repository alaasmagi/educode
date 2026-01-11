import React, { useEffect, useState } from "react";
import { StyleSheet, TouchableOpacity, View, Modal } from "react-native";
import QRCode from "react-native-qrcode-svg";
import { preventScreenCaptureAsync, allowScreenCaptureAsync } from "expo-screen-capture";
import { heightPercentageToDP as hp } from "react-native-responsive-screen";
import * as Brightness from "expo-brightness";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme"; // Või useAppStyles

interface QrGeneratorProperties {
  value: string;
}

const QrGenerator: React.FC<QrGeneratorProperties> = ({ value }) => {
  const { styles } = ApplyStyles();
  const [enlarged, setIsEnlarged] = useState(false);

  const restoreBrightness = async () => {
    await Brightness.restoreSystemBrightnessAsync();
  };

  useEffect(() => {
    preventScreenCaptureAsync();
    return () => {
      allowScreenCaptureAsync();
      restoreBrightness();
    };
  }, []);

  useEffect(() => {
    if (enlarged) {
      (async () => {
        await Brightness.setBrightnessAsync(1.0);
      })();
    } else {
      restoreBrightness();
    }
  }, [enlarged]);

  const sheet = StyleSheet.create({
    overlay: {
      flex: 1,
      backgroundColor: "rgba(0,0,0,0.96)",
      justifyContent: "center",
      alignItems: "center",
    },
    containerNormal: {
      height: styles["qr-generator-size"],
      width: styles["qr-generator-size"],
      aspectRatio: 1,
      borderRadius: styles["qr-generator-border-radius"],
      borderWidth: styles["qr-generator-border-thickness"],
      backgroundColor: styles["qr-generator-bg-color"],
      borderColor: styles["qr-generator-border-color"],
      alignItems: "center",
      justifyContent: "center",
    },
    containerEnlarged: {
      height: styles["qr-generator-enlarged-size"],
      width: styles["qr-generator-enlarged-size"],
      aspectRatio: 1,
      borderRadius: styles["qr-generator-border-radius"],
      borderWidth: styles["qr-generator-border-thickness"],
      backgroundColor: styles["qr-generator-bg-color"],
      borderColor: styles["qr-generator-border-color"],
      alignItems: "center",
      justifyContent: "center",
    },
  });

  return (
    <>
      {!enlarged && (
        <View style={sheet.containerNormal}>
          <TouchableOpacity onPress={() => setIsEnlarged(true)}>
            <QRCode
              color="#1E1E1E"
              size={hp("25%")}
              backgroundColor="#E8EEF1"
              value={value}
            />
          </TouchableOpacity>
        </View>
      )}
      <Modal
        visible={enlarged}
        statusBarTranslucent={true}
        navigationBarTranslucent={true}
        transparent
        animationType="fade"
        onRequestClose={() => setIsEnlarged(false)}
      >
        <TouchableOpacity style={sheet.overlay} onPress={() => setIsEnlarged(false)}>
          <View style={sheet.containerEnlarged}>
            <QRCode
              color="#1E1E1E"
              size={hp("34%")}
              backgroundColor="#E8EEF1"
              value={value}
            />
          </View>
        </TouchableOpacity>
      </Modal>
    </>
  );
};
export default QrGenerator;