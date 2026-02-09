import React from "react";
import InitialSelectionView from "../screens/InitialSelectionView";
import { useNavigationAdapter } from "../businesslogic/hooks/useNavigationAdapter";

export default function InitialSelectionScreen() {
  const { navigation, route } = useNavigationAdapter();
  return <InitialSelectionView navigation={navigation as any} route={route as any} />;
}



