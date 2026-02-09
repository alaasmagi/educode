import React from "react";
import { Stack } from "expo-router";
import { StatusBar } from "expo-status-bar";
import { SafeAreaProvider } from "react-native-safe-area-context";
import { useFonts } from "expo-font";
import { PaperProvider } from "react-native-paper";
import { ApplyStyles } from "../businesslogic/hooks/SelectAppTheme";
import { EAppTheme } from "../models/EAppTheme";

export default function RootLayout() {
  const { appTheme } = ApplyStyles();
  const [fontsLoaded] = useFonts({
    "Nunito-normal": require("../assets/fonts/Nunito-normal.ttf"),
    "Roboto-normal": require("../assets/fonts/Roboto-normal.ttf"),
    "Nunito-bold": require("../assets/fonts/Nunito-bold.ttf"),
  });

  if (!fontsLoaded) {
    return null;
  }

  return (
    <SafeAreaProvider>
      <PaperProvider>
        <StatusBar style={appTheme === EAppTheme.Light ? "dark" : "light"} />
        <Stack screenOptions={{ headerShown: false }}>
          <Stack.Screen name="index" />
          <Stack.Screen name="initial-selection" />
          <Stack.Screen name="login" options={{ gestureEnabled: false }} />
          <Stack.Screen name="create-account" />
          <Stack.Screen name="forgot-password" />
          <Stack.Screen name="(authenticated)" options={{ headerShown: false }} />
        </Stack>
      </PaperProvider>
    </SafeAreaProvider>
  );
}

