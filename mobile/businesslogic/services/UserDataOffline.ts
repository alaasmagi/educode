import Storage from "../data/LocalDataAccess";
import { LocalKeys } from "../helpers/HardcodedLocalDataKeys";
import LocalUserData from "../../models/LocalUserDataModel";
import { EAppTheme } from "../../models/EAppTheme";

export async function GetOfflineUserData(): Promise<LocalUserData | null> {
  const userData: LocalUserData | null = await Storage.getData(LocalKeys.userProfile);
  return userData;
}

export async function SaveOfflineUserData(userData: LocalUserData) {
  await Storage.saveData(LocalKeys.userProfile, userData);
}

export async function DeleteOfflineUserData() {
  await Storage.removeData(LocalKeys.userProfile);
}

export async function GetUserToken(): Promise<string | null> {
  const token: string | null = await Storage.getData(LocalKeys.localToken);
  return token;
}

export async function SaveUserToken(token: string) {
  await Storage.saveData(LocalKeys.localToken, token);
}

export async function DeleteUserToken() {
  await Storage.removeData(LocalKeys.localToken);
}

export async function GetTempToken(): Promise<string | null> {
  const token: string | null = await Storage.getData(LocalKeys.tempToken);
  return token;
}

export async function SaveTempToken(token: string) {
  await Storage.saveData(LocalKeys.tempToken, token);
}

export async function DeleteTempToken() {
  await Storage.removeData(LocalKeys.tempToken);
}

export async function GetCurrentLanguage(): Promise<string | null> {
  const language: string | null = await Storage.getData(LocalKeys.currentLanguage);
  return language;
}

export async function SaveCurrentLanguage(currentLanguage: string) {
  await Storage.saveData(LocalKeys.currentLanguage, currentLanguage);
}

export async function DeleteCurrentLanguage() {
  await Storage.removeData(LocalKeys.currentLanguage);
}

export async function GetCurrentTheme(): Promise<EAppTheme | null> {
  const theme: EAppTheme | null = await Storage.getData(LocalKeys.appTheme);
  return theme;
}

export async function SaveCurrentTheme(currentTheme: EAppTheme) {
  await Storage.saveData(LocalKeys.appTheme, currentTheme);
}

export async function DeleteCurrentTheme() {
  await Storage.removeData(LocalKeys.appTheme);
}
