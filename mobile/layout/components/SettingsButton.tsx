import React from "react";
import { TouchableOpacity, StyleSheet } from "react-native";
import { IconContent } from "./Icons";
import Icon from "./Icon";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme";
interface SettingsButtonProperties {
  onPress: () => void;
}

const SettingsButton: React.FC<SettingsButtonProperties> = ({ onPress }) => {
  const { styles } = ApplyStyles();

  const sheet = StyleSheet.create({
    structure: {
      backgroundColor: styles["settings-button-bg-color"],
      borderRadius: styles["settings-button-border-radius"],
      borderWidth: styles["settings-button-border-thickness"],
      borderColor: styles["settings-button-border-color"],
      paddingHorizontal: 10,
      paddingVertical: 10,
    },
  });

  return (
    <TouchableOpacity style={sheet.structure} onPress={onPress}>
      <Icon
        size={styles["settings-button-icon-size"]}
        color={styles["settings-button-icon-color"]}
        iconContent={IconContent["account-settings-icon"]}
        strokeWidth={styles["settings-button-icon-thickness"]}
      />
    </TouchableOpacity>
  );
};

export default SettingsButton;