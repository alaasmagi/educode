import React from "react";
import CompleteAttendanceView from "../../screens/CompleteAttendanceVIew";
import { useNavigationAdapter } from "../../businesslogic/hooks/useNavigationAdapter";

export default function CompleteAttendanceScreen() {
  const { navigation, route } = useNavigationAdapter();
  return <CompleteAttendanceView navigation={navigation as any} route={route as any} />;
}


