import { useColorScheme } from "react-native";
import { DarkTheme, LightTheme, AppTheme } from "../../layout/styles/ThemeStyles";
import { Styles } from "../../layout/styles/Styles";
import { useMemo, useEffect, useState } from "react";
import { EAppTheme } from "../../models/EAppTheme";
import { GetCurrentTheme } from "../services/UserDataOffline";

export function ApplyStyles() {
  const systemScheme = useColorScheme();
  const [appTheme, setAppTheme] = useState<EAppTheme>(EAppTheme.System);

  useEffect(() => {
    (async () => {
      const storedTheme = await GetCurrentTheme();
      setAppTheme(storedTheme ?? EAppTheme.System);
    })();
  }, []);

  let theme: AppTheme;
  switch (appTheme) {
    case EAppTheme.Dark:
      theme = DarkTheme;
      break;
    case EAppTheme.Light:
      theme = LightTheme;
      break;
    default:
      theme = systemScheme === "light" ? LightTheme : DarkTheme; 
      break;
  }
  
  const styles = useMemo(() => Styles(theme), [theme]);

  return { theme, styles, appTheme };
}
