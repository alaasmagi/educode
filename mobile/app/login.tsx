import React from "react";
import LoginView from "../screens/LoginView";
import { useNavigationAdapter } from "../businesslogic/hooks/useNavigationAdapter";

export default function LoginScreen() {
  const { navigation, route } = useNavigationAdapter();
  return <LoginView navigation={navigation as any} route={route as any} />;
}


