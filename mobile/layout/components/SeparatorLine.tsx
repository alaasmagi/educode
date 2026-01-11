import React from "react";
import { View, StyleSheet, Text } from "react-native";
import { widthPercentageToDP as wp } from "react-native-responsive-screen";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme";
import { OverallUiStyles } from "../styles/Styles";
interface SeparatorLineProperties {
  text: string;
}

const SeparatorLine: React.FC<SeparatorLineProperties> = ({ text }) => {
  const { styles } = ApplyStyles();

  const sheet = StyleSheet.create({
    lineContainer: {
      flexDirection: "row",
      alignItems: "center",
      width: wp("90%"),
    },
    line: {
      flex: 1,
      height: styles["separator-line-thickness"],
      backgroundColor: styles["separator-line-color"],
    },
    lineText: {
      color: styles["separator-line-font-color"],
      fontSize: styles["separator-line-font-size"],
      fontFamily: OverallUiStyles["default-font-family"],
      marginHorizontal: wp("2%"),
    },
  });

  return (
    <View style={sheet.lineContainer}>
      <View style={sheet.line} />
      <Text style={sheet.lineText}>{text}</Text>
      <View style={sheet.line} />
    </View>
  );
};

export default SeparatorLine;