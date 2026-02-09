import { useRouter, useLocalSearchParams } from "expo-router";
import { useMemo } from "react";

// Route mapping from old navigation to new Expo Router paths
const routeMap: Record<string, string> = {
  InitialSelectionView: "/initial-selection",
  LoginView: "/login",
  CreateAccountView: "/create-account",
  ForgotPasswordView: "/forgot-password",
  StudentMainView: "/(authenticated)/student-main",
  TeacherMainView: "/(authenticated)/teacher-main",
  SettingsView: "/(authenticated)/settings",
  CompleteAttendanceView: "/(authenticated)/complete-attendance",
};

/**
 * Hook to create a navigation adapter for screens migrating from React Navigation to Expo Router
 */
export function useNavigationAdapter() {
  const router = useRouter();
  const params = useLocalSearchParams();

  const navigation = useMemo(
    () => ({
      navigate: (routeName: string, routeParams?: any) => {
        const path = routeMap[routeName] || `/${routeName}`;
        if (routeParams) {
          router.push({
            pathname: path as any,
            params: routeParams,
          });
        } else {
          router.push(path as any);
        }
      },
      goBack: () => router.back(),
      replace: (routeName: string, routeParams?: any) => {
        const path = routeMap[routeName] || `/${routeName}`;
        if (routeParams) {
          router.replace({
            pathname: path as any,
            params: routeParams,
          });
        } else {
          router.replace(path as any);
        }
      },
      canGoBack: () => router.canGoBack(),
    }),
    [router]
  );

  const route = useMemo(
    () => ({
      params,
      key: "",
      name: "",
    }),
    [params]
  );

  return { navigation, route };
}

