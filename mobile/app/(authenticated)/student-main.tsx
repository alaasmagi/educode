import React from "react";
import StudentMainView from "../../screens/StudentMainView";
import { useNavigationAdapter } from "../../businesslogic/hooks/useNavigationAdapter";

export default function StudentMainScreen() {
  const { navigation, route } = useNavigationAdapter();
  return <StudentMainView navigation={navigation as any} route={route as any} />;
}


