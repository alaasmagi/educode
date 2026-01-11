import React from "react";
import { View, Text, TouchableOpacity, StyleSheet } from "react-native";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme";
import { OverallUiStyles } from "../styles/Styles";
import Icon from "./Icon";

type TripleSwitchSelection = "left" | "middle" | "right";
interface TripleSwitchProperties {
  textLeft: string;
  textMiddle: string;
  textRight: string;
  iconLeft: string;
  iconMiddle: string;
  iconRight: string;
  onPressLeft: () => void;
  onPressMiddle: () => void;
  onPressRight: () => void;
  isLeftSelected: boolean;
  isMidSelected: boolean;
  isRightSelected: boolean;
  isDisabled?: boolean;
}

const TripleSwitch: React.FC<TripleSwitchProperties> = ({
  textLeft,
  textMiddle,
  textRight,
  iconLeft,
  iconMiddle,
  iconRight,
  onPressLeft,
  onPressMiddle,
  onPressRight,
  isLeftSelected,
  isMidSelected,
  isRightSelected,
  isDisabled = false,
}) => {
  const { styles } = ApplyStyles();

  const sheet = StyleSheet.create({
    container: {
      flexDirection: "row",
      backgroundColor: styles["triple-switch-bg-color"],
      borderRadius: styles["triple-switch-border-radius"],
    },
    containerDisabled: {
      flexDirection: "row",
      backgroundColor: styles["triple-switch-bg-color"],
      borderRadius: styles["triple-switch-border-radius"],
      padding: 2,
      opacity: 0.5,
    },
    option: {
      flex: 1,
      gap: 5,
      paddingVertical: hp("1%"),
      borderRadius: styles["triple-switch-option-border-radius"],
      alignItems: "center",
      justifyContent: "center",
    },
    selected: {
      backgroundColor: styles["triple-switch-selected-bg-color"],
      borderWidth: styles["triple-switch-selected-border-thickness"],
      borderColor: styles["triple-switch-selected-border-color"],
    },
    text: {
      textAlign: "center",
      color: styles["triple-switch-font-color"],
      fontSize: styles["triple-switch-font-size"],
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
          size={styles["triple-switch-icon-size"]}
          color={styles["triple-switch-icon-color"]}
          iconContent={iconLeft}
          strokeWidth={styles["triple-switch-icon-thickness"]}
        />
        <Text style={sheet.text}>{textLeft}</Text>
      </TouchableOpacity>
      <TouchableOpacity
        style={[sheet.option, isMidSelected && sheet.selected]}
        disabled={isDisabled}
        onPress={() => onPressMiddle()}
      >
        <Icon
          size={styles["triple-switch-icon-size"]}
          color={styles["triple-switch-icon-color"]}
          iconContent={iconMiddle}
          strokeWidth={styles["triple-switch-icon-thickness"]}
        />
        <Text style={sheet.text}>{textMiddle}</Text>
      </TouchableOpacity>
      <TouchableOpacity
        style={[sheet.option, isRightSelected && sheet.selected]}
        disabled={isDisabled}
        onPress={() => onPressRight()}
      >
        <Icon
          size={styles["triple-switch-icon-size"]}
          color={styles["triple-switch-icon-color"]}
          iconContent={iconRight}
          strokeWidth={styles["triple-switch-icon-thickness"]}
        />
        <Text style={sheet.text}>{textRight}</Text>
      </TouchableOpacity>
    </View>
  );
};

export default TripleSwitch;
