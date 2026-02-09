import React from "react";
import CreateAccountView from "../screens/CreateAccountView";
import { useNavigationAdapter } from "../businesslogic/hooks/useNavigationAdapter";

export default function CreateAccountScreen() {
  const { navigation, route } = useNavigationAdapter();
  return <CreateAccountView navigation={navigation as any} route={route as any} />;
}


