import { Platform, StatusBar } from "react-native";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import { AppTheme } from "./ThemeStyles";

export const GetNativeSafeArea = (theme: AppTheme) => ({
  flex: 1,
  backgroundColor: theme["darkgrey-white"],
  paddingTop: Platform.OS === "android" ? StatusBar.currentHeight : 0,
  paddingBottom: hp("2.5%"),
  paddingHorizontal: wp("5%"),
});
