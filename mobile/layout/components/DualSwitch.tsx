import React from "react";
import { View, Text, TouchableOpacity, StyleSheet } from "react-native";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme";
import { OverallUiStyles } from "../styles/Styles";
import Icon from "./Icon";

interface DualSwitchProperties {
  textLeft: string;
  iconLeft: string;
  textRight: string;
  iconRight: string;
  onPressLeft: () => void;
  onPressRight: () => void;
  isLeftSelected: boolean;
  isRightSelected: boolean;
  isDisabled?: boolean;
}

const DualSwitch: React.FC<DualSwitchProperties> = ({
  textLeft,
  iconLeft,
  textRight,
  iconRight,
  onPressLeft,
  onPressRight,
  isLeftSelected,
  isRightSelected,
  isDisabled = false,
}) => {
  const { styles } = ApplyStyles();

  const sheet = StyleSheet.create({
    container: {
      flexDirection: "row",
      backgroundColor: styles["dual-switch-bg-color"],
      borderRadius: styles["dual-switch-border-radius"],
    },
    containerDisabled: {
      flexDirection: "row",
      backgroundColor: styles["dual-switch-bg-color"],
      borderRadius: styles["dual-switch-border-radius"],
      padding: 2,
      opacity: 0.5,
    },
    option: {
      flex: 1,
      gap: 5,
      paddingVertical: hp("1%"),
      borderRadius: styles["dual-switch-option-border-radius"],
      alignItems: "center",
      justifyContent: "center",
    },
    selected: {
      backgroundColor: styles["dual-switch-selected-bg-color"],
      borderWidth: styles["dual-switch-selected-border-thickness"],
      borderColor: styles["dual-switch-selected-border-color"],
    },
    text: {
      textAlign: "center",
      color: styles["dual-switch-font-color"],
      fontSize: styles["dual-switch-font-size"],
      fontFamily: OverallUiStyles["default-heading-font-family"],
    },
  });

  return (
    <View style={isDisabled ? sheet.containerDisabled : sheet.container}>
      <TouchableOpacity
        style={[sheet.option, isLeftSelected && sheet.selected]}
        disabled={isDisabled}
        onPress={() => onPressLeft()}
      >
        <Icon
          size={styles["dual-switch-icon-size"]}
          color={styles["dual-switch-icon-color"]}
          iconContent={iconLeft}
          strokeWidth={styles["dual-switch-icon-thickness"]}
        />
        <Text style={sheet.text}>{textLeft}</Text>
      </TouchableOpacity>
      <TouchableOpacity
        style={[sheet.option, isRightSelected && sheet.selected]}
        disabled={isDisabled}
        onPress={() => onPressRight()}
      >
        <Icon
          size={styles["dual-switch-icon-size"]}
          color={styles["dual-switch-icon-color"]}
          iconContent={iconRight}
          strokeWidth={styles["dual-switch-icon-thickness"]}
        />
        <Text style={sheet.text}>{textRight}</Text>
      </TouchableOpacity>
    </View>
  );
};

export default DualSwitch;
