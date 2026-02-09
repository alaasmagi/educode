import React from "react";
import TeacherMainView from "../../screens/TeacherMainView";
import { useNavigationAdapter } from "../../businesslogic/hooks/useNavigationAdapter";

export default function TeacherMainScreen() {
  const { navigation, route } = useNavigationAdapter();
  return <TeacherMainView navigation={navigation as any} route={route as any} />;
}


