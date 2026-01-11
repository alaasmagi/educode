import React, { useState } from "react";
import { View, TextInput, StyleSheet, Text, TouchableOpacity } from "react-native";
import { IconContent } from "./Icons";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import Icon from "./Icon";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme";
import { OverallUiStyles } from "../styles/Styles";

interface TextBoxProperties {
  iconName: string;
  label: string;
  placeHolder?: string;
  value?: string;
  onChangeText?: (text: string) => void;
  isPassword?: boolean;
  autoCapitalize?: "none" | "sentences" | "words" | "characters";
  editable?: boolean;
}

const TextBox: React.FC<TextBoxProperties> = ({
  iconName,
  label,
  placeHolder,
  value,
  onChangeText,
  isPassword,
  autoCapitalize = "sentences",
  editable = true,
}) => {
  const { styles } = ApplyStyles();
  const [isPasswordVisible, setIsPasswordVisible] = useState(false);
  const [isFocused, setIsFocused] = useState<boolean>(false);

  const sheet = StyleSheet.create({
    textBoxContainer: {
      alignSelf: "center",
      width: wp("85%"),
    },
    inputUnFocused: {
      flexDirection: "row",
      alignItems: "center",
      paddingVertical: hp("0.1%"),
      paddingHorizontal: wp("2%"),
      gap: wp("1%"),
      borderWidth: styles["textbox-border-thickness"],
      borderColor: styles["textbox-border-color"],
      borderRadius: styles["textbox-border-radius"],
    },
    label: {
      color: styles["textbox-font-color"],
      fontSize: styles["textbox-font-size"],
      fontFamily: OverallUiStyles["default-font-family"],
    },
    input: {
      color: styles["textbox-font-color"],
      fontSize: styles["textbox-font-size"],
      fontFamily: OverallUiStyles["default-font-family"],
      flex: 1,
    },
    inputDisabled: {
      color: styles["textbox-font-color"],
      fontSize: styles["textbox-font-size"],
      fontFamily: OverallUiStyles["default-font-family"],
      flex: 1,
      opacity: 0.5,
    },
    inputFocused: {
      flexDirection: "row",
      alignItems: "center",
      gap: wp("1%"),
      paddingVertical: hp("0.1%"),
      paddingHorizontal: wp("2%"),
      borderWidth: styles["textbox-border-thickness"],
      borderColor: styles["textbox-active-border-color"],
      borderRadius: styles["textbox-border-radius"],
    },
  });

  return (
    <View style={sheet.textBoxContainer}>
      <Text style={sheet.label}>{label}</Text>
      <View style={isFocused ? sheet.inputFocused : sheet.inputUnFocused}>
        <Icon
          size={styles["textbox-icon-size"]}
          color={styles["textbox-icon-color"]}
          iconContent={IconContent[iconName]}
          strokeWidth={styles["textbox-icon-thickness"]}
        />
        <TextInput
          placeholder={placeHolder}
          placeholderTextColor={styles["textbox-placeholder-color"]}
          style={editable == false ? sheet.inputDisabled : sheet.input}
          scrollEnabled={true}
          numberOfLines={1}
          secureTextEntry={isPassword && !isPasswordVisible}
          value={value}
          onChangeText={onChangeText}
          autoCapitalize={autoCapitalize}
          editable={editable}
          onFocus={() => setIsFocused(true)}
          onBlur={() => setIsFocused(false)}
        />
        {isPassword && (
          <TouchableOpacity onPress={() => setIsPasswordVisible((prev) => !prev)}>
            <Icon
              size={styles["textbox-icon-size"]}
              color={styles["textbox-icon-color"]}
              iconContent={isPasswordVisible ? IconContent["visibility-on"] : IconContent["visibility-off"]}
              strokeWidth={styles["textbox-icon-thickness"]}
            />
          </TouchableOpacity>
        )}
      </View>
    </View>
  );
};

export default TextBox;
