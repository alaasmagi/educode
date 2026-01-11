import React from "react";
import { Image, StyleSheet, View } from "react-native";
import LanguageSwitch from "../components/LanguageSwitch";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme";
import { LightTheme } from "../styles/ThemeStyles";

const FormHeader = () => {
  const { theme, appTheme } = ApplyStyles();

  return (
    <View style={styles.structure}>
      <Image
        style={styles.logo}
        resizeMode="contain"
        source={
          theme["black-white"] === LightTheme["black-white"]
            ? require("../../assets/app-logos/logo-light.png")
            : require("../../assets/app-logos/logo-dark.png")
        }
      />
      <LanguageSwitch />
    </View>
  );
};

const styles = StyleSheet.create({
  logo: {
    width: wp("70%"),
    height: hp("7%"),
  },
  structure: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    width: wp("90%"),
  },
});

export default FormHeader;
