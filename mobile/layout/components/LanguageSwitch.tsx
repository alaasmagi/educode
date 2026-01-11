import React, { useState, useRef } from "react";
import { TouchableOpacity, StyleSheet, Image, View, TouchableWithoutFeedback, LayoutChangeEvent } from "react-native";
import { Flags, IconContent } from "./Icons";
import i18next from "../../businesslogic/services/i18next";
import { SaveCurrentLanguage } from "../../businesslogic/services/UserDataOffline";
import { widthPercentageToDP as wp, heightPercentageToDP as hp } from "react-native-responsive-screen";
import Icon from "./Icon";
import { Styles } from "../styles/Styles";
import { ApplyStyles } from "../../businesslogic/hooks/SelectAppTheme";

const languages = [
  { code: "et", icon: Flags["est-flag"] },
  { code: "en", icon: Flags["eng-flag"] },
  { code: "fi", icon: Flags["fin-flag"] },
  { code: "lv", icon: Flags["lat-flag"] },
  { code: "lt", icon: Flags["lit-flag"] },
  { code: "ge", icon: Flags["ger-flag"] },
  { code: "uk", icon: Flags["ukr-flag"] },
  { code: "ru", icon: Flags["rus-flag"] },
];

const LanguageSwitch = () => {
  
  const { styles } = ApplyStyles();
  const [showDropdown, setShowDropdown] = useState(false);
  const [buttonWidth, setButtonWidth] = useState<number>(100);
  const currentLang = i18next.language;

  const buttonRef = useRef<View>(null);

  const dropdownLanguages = languages.filter((l) => l.code !== currentLang);

  const onSelectLanguage = async (langCode: string) => {
    setShowDropdown(false);
    if (langCode !== currentLang) {
      i18next.changeLanguage(langCode);
      await SaveCurrentLanguage(langCode);
    }
  };

  const localStyles = StyleSheet.create({
  structure: {
    backgroundColor: styles["normal-button-bg-color"],
    borderRadius: styles["normal-button-border-radius"],
    borderWidth: styles["normal-button-border-thickness"],
    borderColor: styles["normal-button-border-color"],
    paddingVertical: hp("0.5%"),
    paddingHorizontal: wp("3%"),
    justifyContent: "center",
    alignItems: "center",
  },
  image: {
    height: hp("4.5%"),
    width: wp("9%"),
    resizeMode: "contain",
  },
  dropdown: {
    backgroundColor: styles["normal-button-bg-color"],
    borderRadius: styles["normal-button-border-radius"],
    borderWidth: styles["normal-button-border-thickness"],
    borderColor: styles["normal-button-border-color"],
    alignSelf: "center",
    overflow: "hidden",
    position: "absolute",
    zIndex: 1,
  },
  dropdownItem: {
    paddingVertical: hp("0.5%"),
    alignItems: "center",
    flexDirection: "row",
    justifyContent: "center",
    backgroundColor: "transparent",
  },
  imageDropdown: {
    height: hp("2.7%"),
    width: wp("8%"),
    resizeMode: "contain",
  },
  backDrop: {
    width: wp("100%"),
    height: hp("100%"),
  },
});

  return (
    <View>
      <TouchableOpacity
        style={localStyles.structure}
        ref={buttonRef}
        activeOpacity={0.8}
        onPress={() => setShowDropdown(!showDropdown)}
        onLayout={(event: LayoutChangeEvent) => {
          setButtonWidth(event.nativeEvent.layout.width);
        }}
      >
        <Image source={languages.find((l) => l.code === currentLang)?.icon} style={localStyles.image} />
      </TouchableOpacity>
      {showDropdown && (
        <TouchableWithoutFeedback onPress={() => setShowDropdown(false)} style={localStyles.backDrop}>
          <View style={[localStyles.dropdown, { width: buttonWidth }]}>
            <TouchableOpacity
              style={[localStyles.dropdownItem]}
              onPress={() => {
                setShowDropdown(false);
              }}
            >
              <Icon
                size={styles["normal-icon-size"]}
                color={styles["normal-icon-color"]}
                iconContent={IconContent["arrow-up-icon"]}
                strokeWidth={styles["normal-icon-thickness"]}
              />
            </TouchableOpacity>
            {dropdownLanguages.map((item) => (
              <TouchableOpacity
                key={item.code}
                style={localStyles.dropdownItem}
                onPress={() => onSelectLanguage(item.code)}
              >
                <Image source={item.icon} style={localStyles.image} />
              </TouchableOpacity>
            ))}
          </View>
        </TouchableWithoutFeedback>
      )}
    </View>
  );
};

export default LanguageSwitch;
