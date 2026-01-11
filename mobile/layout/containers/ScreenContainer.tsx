import React from "react";
import {
  KeyboardAvoidingView,
  Platform,
  SafeAreaView,
  ScrollView,
  TouchableWithoutFeedback,
  StyleSheet,
  Keyboard,
  View,
  StyleProp,
  ViewStyle,
} from "react-native";
import { GetNativeSafeArea } from "../styles/NativeStyles";
import { heightPercentageToDP as hp } from "react-native-responsive-screen";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme";

type ScreenContainerProps = {
  children: React.ReactNode;
  header?: React.ReactNode;
  scroll?: boolean;
  dismissKeyboardOnPress?: boolean;
  keyboardVerticalOffsetIOS?: number;
  keyboardVerticalOffsetAndroid?: number;
  safeAreaStyle?: StyleProp<ViewStyle>;
  contentContainerStyle?: StyleProp<ViewStyle>;
};

export const ScreenContainer: React.FC<ScreenContainerProps> = ({
  children,
  header,
  dismissKeyboardOnPress = false,
  scroll = false,
  keyboardVerticalOffsetIOS = 100,
  keyboardVerticalOffsetAndroid = -hp("9%"),
  safeAreaStyle,
}) => {
  const { theme } = ApplyStyles();

  const defaultSafeAreaStyle = GetNativeSafeArea(theme);
  const safeAreaViewStyle = safeAreaStyle ?? defaultSafeAreaStyle;

  const ContainerView = scroll ? ScrollView : View;
  const containerProps = scroll ? { keyboardShouldPersistTaps: "handled" as const } : {};

  const sheet = StyleSheet.create({
    header: {
      marginVertical: hp("5%"),
    },
    structure: {
      flex: 1,
    }
  });

  const Inner = (
    <KeyboardAvoidingView
      style={sheet.structure}
      behavior={Platform.OS === "ios" ? "padding" : "height"}
      keyboardVerticalOffset={Platform.OS === "ios" ? keyboardVerticalOffsetIOS : keyboardVerticalOffsetAndroid}
    >
      <SafeAreaView style={safeAreaViewStyle}>
        {header ? <View style={sheet.header}>{header}</View> : null}
        <ContainerView {...containerProps}>{children}</ContainerView>
      </SafeAreaView>
    </KeyboardAvoidingView>
  );
  if (dismissKeyboardOnPress) {
    return (
      <TouchableWithoutFeedback onPress={Keyboard.dismiss} accessible={false}>
        {Inner}
      </TouchableWithoutFeedback>
    );
  }
  return Inner;
};