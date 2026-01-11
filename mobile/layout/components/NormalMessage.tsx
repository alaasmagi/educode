import React from "react";
import { Text, StyleSheet, View } from "react-native";
import Icon from "./Icon";
import { IconContent } from "./Icons";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme"; 
import { OverallUiStyles } from "../styles/Styles";
import { widthPercentageToDP as wp } from "react-native-responsive-screen";

interface NormalMessageProperties {
  text: string | null;
}

const NormalMessage: React.FC<NormalMessageProperties> = ({ text }) => {
  const { styles } = ApplyStyles();

  const sheet = StyleSheet.create({
    container: {
      flexDirection: "row",
      alignSelf: "center",
      alignItems: "center",
      backgroundColor: styles["info-card-bg-color"],
      padding: 10,
      width: wp("85%"),
      borderRadius: styles["message-card-border-radius"],
      borderColor: styles["info-card-border-color"],
      borderWidth: styles["message-card-border-thickness"],
    },
    icon: {
      width: "15%",
    },
    content: {
      width: "85%",
      textAlign: "center",
      color: styles["info-font-color"],
      fontSize: styles["message-card-font-size"],
      fontFamily: OverallUiStyles["default-font-family"]
    },
  });

  return (
    <View style={sheet.container}>
      <View style={sheet.icon}>
        <Icon
          size={styles["message-card-icon-size"]}
          iconContent={IconContent["information-icon"]}
          color={styles["info-font-color"]}
          strokeWidth={styles["message-card-icon-thickness"]}
        />
      </View>
      <Text style={sheet.content}>{text}</Text>
    </View>
  );
};

export default NormalMessage;