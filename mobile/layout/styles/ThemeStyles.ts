import { ColorValue } from "react-native";

export type AppTheme = {
  "darkgrey-white": ColorValue;
  "black-blue": ColorValue;
  "black-white": ColorValue;
  "white-black": ColorValue;
  "blue-black": ColorValue;
  "lightgrey-white": ColorValue;
  "lightgray-gray": ColorValue;
  "darkred-lightred": ColorValue;
  "darkgreen-lightgreen": ColorValue;
  "darkblue-lightblue": ColorValue;
  "default-blue": ColorValue;
  "default-white": ColorValue;
  "default-lightgray": ColorValue;
  "default-alert-red": ColorValue;
  "default-success-green": ColorValue;
  "default-normal-blue": ColorValue;
};

export const DarkTheme: AppTheme = {
  "darkgrey-white": "#343434",
  "black-blue": "#262626",
  "black-white": "#262626",
  "white-black": "#E8EEF1",
  "blue-black": "#1977E2",
  "lightgrey-white": "#515151",
  "lightgray-gray": "#BCBCBD",
  "darkred-lightred": "#3F1E20",
  "darkgreen-lightgreen": "#1E3F20",
  "darkblue-lightblue": "#16325B",
  "default-blue": "#1977E2",
  "default-white": "#E8EEF1",
  "default-lightgray": "#515151",
  "default-alert-red": "#DD2D4A",
  "default-success-green": "#2DD452",
  "default-normal-blue": "#4C97FF",
};

export const LightTheme: AppTheme = {
  "darkgrey-white": "#E8EEF1",
  "black-blue": "#1977E2",
  "black-white": "#E8EEF1",
  "white-black": "#262626",
  "blue-black": "#262626",
  "lightgrey-white": "#E0E0E0",
  "lightgray-gray": "#696969",
  "darkred-lightred": "#F8D6DB",
  "darkgreen-lightgreen": "#D0F5D8",
  "darkblue-lightblue": "#C2DBFF",
  "default-blue": "#1977E2",
  "default-white": "#E8EEF1",
  "default-lightgray": "#515151",
  "default-alert-red": "#DD2D4A",
  "default-success-green": "#2DD452",
  "default-normal-blue": "#4C97FF",
};
