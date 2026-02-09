import React from "react";
import { Stack } from "expo-router";
export default function AuthenticatedLayout() {
  return (
    <Stack screenOptions={{ headerShown: false }}>
      <Stack.Screen name="student-main" options={{ gestureEnabled: false }} />
      <Stack.Screen name="teacher-main" options={{ gestureEnabled: false }} />
      <Stack.Screen name="settings" options={{ gestureEnabled: false }} />
      <Stack.Screen name="complete-attendance" />
    </Stack>
  );
}
