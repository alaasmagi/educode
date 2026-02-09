import React from "react";
import ForgotPasswordView from "../screens/ForgotPasswordView";
import { useNavigationAdapter } from "../businesslogic/hooks/useNavigationAdapter";

export default function ForgotPasswordScreen() {
  const { navigation, route } = useNavigationAdapter();
  return <ForgotPasswordView navigation={navigation as any} route={route as any} />;
}


