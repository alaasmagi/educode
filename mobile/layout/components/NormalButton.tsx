import React from "react";
import { TouchableOpacity, Text, StyleSheet } from "react-native";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme";
import { OverallUiStyles } from "../styles/Styles";

interface NormalButtonProperties {
  text: string;
  onPress: () => void;
  disabled?: boolean;
}

const NormalButton: React.FC<NormalButtonProperties> = ({ text, onPress, disabled }) => {
  const { styles } = ApplyStyles();

  const baseStructure = {
    backgroundColor: styles["normal-button-bg-color"],
    borderRadius: styles["normal-button-border-radius"],
    borderWidth: styles["normal-button-border-thickness"] ?? styles["normal-border-thickness"],
    borderColor: styles["normal-button-border-color"],
    justifyContent: "center" as const,
    alignItems: "center" as const,
    width: wp("85%"),
    paddingHorizontal: wp("3.5%"),
    paddingVertical: hp("1.5%"),
  };

  const sheet = StyleSheet.create({
    structure: {
      ...baseStructure,
      opacity: 1,
    },
    structureDisabled: {
      ...baseStructure,
      opacity: 0.5,
    },
    content: {
      color: styles["normal-button-font-color"],
      textAlign: "center" as const,
      fontSize: styles["normal-button-font-size"],
      fontFamily: OverallUiStyles["default-heading-font-family"]
    },
  });

  return (
    <TouchableOpacity
      style={disabled ? sheet.structureDisabled : sheet.structure}
      onPress={onPress}
      disabled={disabled}
    >
      <Text style={sheet.content}>{text}</Text>
    </TouchableOpacity>
  );
};

export default NormalButton;