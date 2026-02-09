import React from "react";
import SettingsView from "../../screens/SettingsView";
import { useNavigationAdapter } from "../../businesslogic/hooks/useNavigationAdapter";

export default function SettingsScreen() {
  const { navigation, route } = useNavigationAdapter();
  return <SettingsView navigation={navigation as any} route={route as any} />;
}


