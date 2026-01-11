import { useFocusEffect } from "@react-navigation/native";
import { useCallback } from "react";
import { BackHandler, Alert } from "react-native";
import { useTranslation } from "react-i18next";

const BackButtonHandler = (navigation: any) => {
  const { t } = useTranslation();

  useFocusEffect(
    useCallback(() => {
      const backAction = () => {
        Alert.alert(t("exit-app"), t("exit-app-prompt"), [
          { text: t("cancel"), onPress: () => null, style: "cancel" },
          { text: t("yes"), onPress: () => BackHandler.exitApp() },
        ]);
        return true;
      };

      const backHandler = BackHandler.addEventListener(
        "hardwareBackPress",
        backAction
      );

      return () => backHandler.remove();
    }, [navigation, t])
  );
};

export default BackButtonHandler;
