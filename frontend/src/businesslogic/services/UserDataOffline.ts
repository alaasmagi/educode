import Storage from "../data/LocalDataAccess";
import { LocalKeys } from "../helpers/HardcodedLocalDataKeys";
import LocalUserData from "../../models/LocalUserDataModel";

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

export async function GetJwtToken(): Promise<string | null> {
  const token: string | null = await Storage.getData(LocalKeys.jwtToken);
  return token;
}

export async function SaveJwtToken(token: string) {
  await Storage.saveData(LocalKeys.jwtToken, token);
}

export async function DeleteJwtToken() {
  await Storage.removeData(LocalKeys.jwtToken);
}

export async function GetRefreshToken(): Promise<string | null> {
  const token: string | null = await Storage.getData(LocalKeys.refreshToken);
  return token;
}

export async function SaveRefreshoken(token: string) {
  await Storage.saveData(LocalKeys.refreshToken, token);
}

export async function DeleteRefreshToken() {
  await Storage.removeData(LocalKeys.refreshToken);
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
  const token: string | null = await Storage.getData(LocalKeys.currentLanguage);
  return token;
}

export async function SaveCurrentLanguage(currentLanguage: string) {
  await Storage.saveData(LocalKeys.currentLanguage, currentLanguage);
}

export async function DeleteCurrentLanguage() {
  await Storage.removeData(LocalKeys.currentLanguage);
}
