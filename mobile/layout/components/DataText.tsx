import React from "react";
import { View, Text, StyleSheet } from "react-native";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme";
import { OverallUiStyles } from "../styles/Styles";

interface DataTextProperties {
  label: string;
  text: string;
}

const DataText: React.FC<DataTextProperties> = ({ label, text }) => {
  const { styles } = ApplyStyles();

  const sheet = StyleSheet.create({
    container: {
      flexDirection: "row",
      gap: hp("1%"),
      width: wp("90%"),
    },
    text: {
      color: styles["data-text-font-color"],
      fontSize: styles["data-text-font-size"],
      fontFamily: OverallUiStyles["default-font-family"]
    },
    label: {
      color: styles["data-text-font-color"],
      fontSize: styles["data-text-font-size"],
      fontFamily: OverallUiStyles["default-heading-font-family"],
    },
  });

  return (
    <View style={sheet.container}>
      <Text style={sheet.label}>{label + ": "}</Text>
      <Text style={sheet.text}>{text}</Text>
    </View>
  );
};

export default DataText;
