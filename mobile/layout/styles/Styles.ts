import { ColorValue } from "react-native";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import { AppTheme } from "./ThemeStyles";

export function Styles(theme: AppTheme) {
  return {
    "normal-font-color": theme["white-black"],
    "normal-icon-color": theme["default-white"],

    "normal-icon-size": 32,
    "normal-button-font-size": 26,

    "normal-button-bg-color": theme["black-blue"],
    "normal-button-border-radius": 20,
    "normal-button-border-thickness": 2,
    "normal-button-border-color": theme["blue-black"],
    "normal-button-font-color": theme["default-white"],

    "textbox-border-thickness": 1,
    "textbox-border-color": theme["white-black"],
    "textbox-font-color": theme["white-black"],
    "textbox-font-size": 20,
    "textbox-active-border-color": theme["default-blue"],
    "textbox-border-radius": 15,
    "textbox-icon-color": theme["white-black"],
    "textbox-icon-size": 28,
    "textbox-icon-thickness": 2,
    "textbox-placeholder-color": theme["lightgray-gray"],

    // Message card styles
    "message-card-icon-thickness": 2,
    "message-card-icon-size": 36,
    "message-card-border-radius": 20,
    "message-card-border-thickness": 2,
    "message-card-font-size": 20,

    "alert-font-color": theme["default-alert-red"],
    "info-font-color": theme["default-normal-blue"],
    "success-font-color": theme["default-success-green"],

    "alert-icon-color": theme["default-alert-red"],
    "info-icon-color": theme["default-normal-blue"],
    "success-icon-color": theme["default-success-green"],

    "alert-card-bg-color": theme["darkred-lightred"],
    "info-card-bg-color": theme["darkblue-lightblue"],
    "success-card-bg-color": theme["darkgreen-lightgreen"],
    "alert-card-border-color": theme["default-alert-red"],
    "info-card-border-color": theme["default-normal-blue"],
    "success-card-border-color": theme["default-success-green"],

    "normal-border-thickness": 2,
    "wide-border-thickness": "",
    "narrow-border-thickness": "",

    "normal-icon-thickness": 2,
    "wide-icon-thickness": "",

    "data-text-font-size": wp("5%"),
    "data-text-font-color": theme["white-black"],

    "normal-link-font-size": wp("4.5%"),
    "normal-link-font-color": theme["white-black"],

    "checkbox-size": 30,
    "checkbox-bg-color": theme["black-white"],
    "checkbox-border-radius": 8,
    "checkbox-border-thickness": 2,
    "checkbox-border-color": theme["blue-black"],
    "checkbox-inner-size": 17,
    "checkbox-inner-radius": 3,
    "checkbox-inner-bg-color": theme["default-blue"],
    "checkbox-font-color": theme["white-black"],
    "checkbox-font-size": wp("4.5%"),

    "greeting-font-color": theme["white-black"],
    "greeting-font-size": wp("7.8%"),

    "dual-switch-border-radius": 20,
    "dual-switch-bg-color": theme["default-lightgray"],
    "dual-switch-option-border-radius": 20,
    "dual-switch-selected-bg-color": theme["black-blue"],
    "dual-switch-selected-border-color": theme["default-blue"],
    "dual-switch-selected-border-thickness": 2,
    "dual-switch-font-color": theme["default-white"],
    "dual-switch-font-size": wp("4.5%"),
    "dual-switch-icon-color": theme["default-white"],
    "dual-switch-icon-size": 34,
    "dual-switch-icon-thickness": 2,

    "qr-generator-size": hp("32%"),
    "qr-generator-border-radius": wp("8%"),
    "qr-generator-border-thickness": wp("4%"),
    "qr-generator-border-color": theme["default-lightgray"],
    "qr-generator-bg-color": theme["default-white"],
    "qr-generator-enlarged-size": hp("42%"),

    "qr-scanner-camera-border-radius": wp("8%"),
    "qr-scanner-camera-border-normal-color": theme["default-lightgray"],
    "qr-scanner-camera-border-success-color": theme["default-success-green"],
    "qr-scanner-camera-border-alert-color": theme["default-alert-red"],
    "qr-scanner-camera-border-width": wp("4%"),
    "qr-scanner-font-color": theme["white-black"],
    "qr-scanner-font-size": wp("6%"),
    "qr-scanner-icon-size": 36,
    "qr-scanner-icon-color": theme["white-black"],
    "qr-scanner-icon-thickness": 2,

    "separator-line-thickness": 1,
    "separator-line-color": theme["white-black"],
    "separator-line-font-color": theme["white-black"],
    "separator-line-font-size": wp("4%"),

    "settings-button-bg-color": theme["black-blue"],
    "settings-button-border-radius": 20,
    "settings-button-border-thickness": 2,
    "settings-button-border-color": theme["blue-black"],

    "settings-button-icon-size": 32,
    "settings-button-icon-color": theme["default-white"],
    "settings-button-icon-thickness": 2,

    "step-divider-circle-width": wp("10%"),
    "step-divider-circle-height": hp("3.5%"),
    "step-divider-border-radius": wp("3%"),
    "step-divider-border-color": theme["blue-black"],
    "step-divider-border-thickness": 2,
    "step-divider-bg-color": theme["black-blue"],
    "step-divider-font-size": wp("4.5%"),
    "step-divider-font-color": theme["white-black"],
    "step-divider-number-color": theme["default-white"],

    "underline-text-font-color": theme["white-black"],
    "underline-text-font-size": 25,

    "triple-switch-border-radius": 20,
    "triple-switch-bg-color": theme["default-lightgray"],
    "triple-switch-option-border-radius": 20,
    "triple-switch-selected-bg-color": theme["black-blue"],
    "triple-switch-selected-border-color": theme["default-blue"],
    "triple-switch-selected-border-thickness": 2,
    "triple-switch-font-color": theme["default-white"],
    "triple-switch-font-size": wp("4.5%"),
    "triple-switch-icon-color": theme["default-white"],
    "triple-switch-icon-size": 34,
    "triple-switch-icon-thickness": 2,
  };
}

export const OverallUiStyles = {
  "default-heading-font-family": "Nunito-bold",
  "default-font-family": "Nunito-normal",
};
