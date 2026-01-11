import React from "react";
import { Text, StyleSheet, View } from "react-native";
import { IconContent } from "./Icons";
import Icon from "./Icon";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme"; 
import { OverallUiStyles } from "../styles/Styles";
import { widthPercentageToDP as wp } from "react-native-responsive-screen";


interface SuccessMessageProperties {
  text: string | null;
}

const SuccessMessage: React.FC<SuccessMessageProperties> = ({ text }) => {
  const { styles } = ApplyStyles();

  const sheet = StyleSheet.create({
    container: {
      width: wp("85%"),
      flexDirection: "row",
      alignSelf: "center",
      alignItems: "center",
      backgroundColor: styles["success-card-bg-color"],
      padding: 10,
      borderRadius: styles["message-card-border-radius"],
      borderColor: styles["success-card-border-color"],
      borderWidth: styles["message-card-border-thickness"],
    },
    icon: {
      width: "15%",
    },
    content: {
      width: "85%",
      textAlign: "center",
      color: styles["success-font-color"],
      fontSize: styles["message-card-font-size"],
      fontFamily: OverallUiStyles["default-font-family"]
    },
  });

  return (
    <View style={sheet.container}>
      <View style={sheet.icon}>
        <Icon
          size={styles["message-card-icon-size"]}
          iconContent={IconContent["check-icon"]}
          color={styles["success-font-color"]}
          strokeWidth={styles["message-card-icon-thickness"]}
        />
      </View>
      <Text style={sheet.content}>{text}</Text>
    </View>
  );
};

export default SuccessMessage;