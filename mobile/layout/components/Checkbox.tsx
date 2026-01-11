import React, { useState } from "react";
import { TouchableOpacity, Text, View, StyleSheet } from "react-native";
import { widthPercentageToDP as wp } from "react-native-responsive-screen";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme";
import { OverallUiStyles } from "../styles/Styles";

interface CheckboxProps {
  label: string;
  checked?: boolean;
  onChange?: (checked: boolean) => void;
}

const Checkbox: React.FC<CheckboxProps> = ({ label, checked = false, onChange }) => {
  const { styles } = ApplyStyles();
  const [isChecked, setIsChecked] = useState(checked);

  const toggleCheckbox = () => {
    setIsChecked(!isChecked);
    if (onChange) onChange(!isChecked);
  };

  const sheet = StyleSheet.create({
    container: {
      flexDirection: "row",
      alignItems: "center",
      gap: wp("3%"),
    },
    checkbox: {
      width: styles["checkbox-size"],
      height: styles["checkbox-size"],
      borderRadius: styles["checkbox-border-radius"],
      borderWidth: styles["checkbox-border-thickness"],
      borderColor: styles["checkbox-border-color"],
      alignItems: "center",
      justifyContent: "center",
      backgroundColor: styles["checkbox-bg-color"],
    },
    innerCircle: {
      width: styles["checkbox-inner-size"],
      height: styles["checkbox-inner-size"],
      borderRadius: styles["checkbox-inner-radius"],
      backgroundColor: styles["checkbox-inner-bg-color"],
    },
    label: {
      fontSize: styles["checkbox-font-size"],
      color: styles["checkbox-font-color"],
      fontFamily: OverallUiStyles["default-font-family"]
    },
  });

  return (
    <TouchableOpacity style={sheet.container} onPress={toggleCheckbox}>
      <View style={sheet.checkbox}>
        {isChecked && <View style={sheet.innerCircle} />}
      </View>
      <Text style={sheet.label}>{label}</Text>
    </TouchableOpacity>
  );
};

export default Checkbox;