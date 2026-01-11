import React from "react";
import { View, Text, StyleSheet } from "react-native";
import { widthPercentageToDP as wp } from "react-native-responsive-screen";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme";
import { OverallUiStyles } from "../styles/Styles";

interface StepDividerProps {
  stepNumber: number;
  label: string;
}

const StepDivider: React.FC<StepDividerProps> = ({ stepNumber, label }) => {
  const { styles } = ApplyStyles();

  const sheet = StyleSheet.create({
    container: {
      flexDirection: "row",
      alignItems: "center",
      justifyContent: "center",
      width: wp("90%"),
      gap: wp("3%"),
    },
    circle: {
      width: styles["step-divider-circle-width"],
      height: styles["step-divider-circle-height"],
      borderRadius: styles["step-divider-border-radius"],
      backgroundColor: styles["step-divider-bg-color"],
      justifyContent: "center",
      alignItems: "center",
      borderWidth: styles["step-divider-border-thickness"],
      borderColor: styles["step-divider-border-color"],
    },
    number: {
      color: styles["step-divider-number-color"],
      fontSize: styles["step-divider-font-size"],
    },
    text: {
      fontSize: styles["step-divider-font-size"],
      color: styles["step-divider-font-color"],
      fontFamily: OverallUiStyles["default-font-family"]
    },
  });

  return (
    <View style={sheet.container}>
      <View style={sheet.circle}>
        <Text style={sheet.number}>{stepNumber}</Text>
      </View>
      <Text style={sheet.text}>{label}</Text>
    </View>
  );
};

export default StepDivider;