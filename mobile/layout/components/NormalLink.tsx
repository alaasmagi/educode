import React from "react";
import { TouchableOpacity, Text, StyleSheet } from "react-native";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme";
import { OverallUiStyles } from "../styles/Styles";

interface NormalLinkProperties {
  text: string;
  onPress: () => void;
}

const NormalLink: React.FC<NormalLinkProperties> = ({ text, onPress }) => {
  const { styles } = ApplyStyles();

  const sheet = StyleSheet.create({
    content: {
      color: styles["normal-link-font-color"],
      fontSize: styles["normal-link-font-size"],
      textDecorationLine: "underline",
      fontFamily: OverallUiStyles["default-font-family"]
    },
  });

  return (
    <TouchableOpacity onPress={onPress}>
      <Text style={sheet.content}>{text}</Text>
    </TouchableOpacity>
  );
};

export default NormalLink;