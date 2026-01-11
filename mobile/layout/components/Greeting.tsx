import React from "react";
import { Text, StyleSheet } from "react-native";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme";
import { OverallUiStyles } from "../styles/Styles";

interface GreetingProperties {
  text: string;
}

const Greeting: React.FC<GreetingProperties> = ({ text }) => {
  const { styles } = ApplyStyles();

  const sheet = StyleSheet.create({
    content: {
      textAlign: "center",
      color: styles["greeting-font-color"],
      fontSize: styles["greeting-font-size"],
      alignSelf: "center",
      fontFamily: OverallUiStyles["default-heading-font-family"],
    },
  });

  return <Text style={sheet.content}>{text}</Text>;
};

export default Greeting;