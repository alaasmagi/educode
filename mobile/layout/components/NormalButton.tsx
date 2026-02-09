import React from "react";
import {Button} from "react-native-paper";
import {StyleSheet, TextStyle, ViewStyle} from 'react-native';

interface PrimaryButtonProperties {
  text: string;
  icon?: string;
  onPress: () => void;
  disabled?: boolean;
  // optional style overrides
  buttonStyle?: ViewStyle;
  contentStyle?: ViewStyle; // controls internal padding
  labelStyle?: TextStyle; // controls font size/family
  // convenience shorthands
  fontSize?: number;
  fontFamily?: string;
  paddingVertical?: number;
  paddingHorizontal?: number;
}

const PrimaryButton: React.FC<PrimaryButtonProperties> = ({ text, icon, onPress, disabled, buttonStyle, contentStyle, labelStyle, fontSize, fontFamily, paddingVertical, paddingHorizontal }) => {
  const mergedLabelStyle = [
    styles.label,
    labelStyle,
    fontSize ? { fontSize } : null,
    fontFamily ? { fontFamily } : null,
  ];

  const mergedContentStyle = [
    styles.content,
    contentStyle,
    paddingVertical ? { paddingVertical } : null,
    paddingHorizontal ? { paddingHorizontal } : null,
  ];

  return (
      <Button
        icon={icon}
        mode="elevated"
        onPress={onPress}
        disabled={disabled}
        style={buttonStyle}
        contentStyle={mergedContentStyle}
        labelStyle={mergedLabelStyle}
      >
          {text}
      </Button>
  );
};

const styles = StyleSheet.create({
  label: {
    fontSize: 16,
    // default font family from your assets (adjust name to match how you register fonts)
    fontFamily: 'Nunito-normal',
  } as TextStyle,
  content: {
    paddingVertical: 10,
    paddingHorizontal: 12,
  } as ViewStyle,
});

export default PrimaryButton;