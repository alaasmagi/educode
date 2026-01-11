import OnlineUserModel from "../../models/OnlineUserModel";
import CreateUserModel from "../../models/CreateUserModel";
import VerifyOTPModel from "../../models/VerifyOTPModel";
import ChangePasswordModel from "../../models/ChangePasswordModel";
import LocalUserData from "../../models/LocalUserDataModel";
import axios from "axios";

import {
  DeleteTempToken,
  GetTempToken,
  GetUserToken,
  SaveOfflineUserData,
  SaveTempToken,
  SaveUserToken,
} from "./UserDataOffline";
import Constants from "expo-constants";

export async function TestConnection(): Promise<boolean> {
  const response = await axios.get(`${Constants.expoConfig?.extra?.EXPO_PUBLIC_API_URL}/User/TestConnection`);
  return response.status === 200;
}

export async function UserLogin(uniId: string, password: string): Promise<boolean | string> {
  const response = await axios.post(
    `${Constants.expoConfig?.extra?.EXPO_PUBLIC_API_URL}/Auth/Login`,
    {
      uniId: uniId,
      password: password,
    },
    {
      headers: {
        "Content-Type": "application/json",
      },
      validateStatus: (status) => status < 500,
    }
  );
  if (response.status === 200 && !response.data.messageCode) {
    SaveUserToken(response.data.token);
    return true;
  }

  return response.data.messageCode ?? "internet-connection-error";
}

export async function CreateUserAccount(model: CreateUserModel): Promise<boolean | string> {
  const response = await axios.post(
    `${Constants.expoConfig?.extra?.EXPO_PUBLIC_API_URL}/Auth/Register`,
    {
      fullName: model.fullName,
      uniId: model.uniId,
      studentCode: model.studentCode,
      password: model.password,
      userRole: "Student",
      creator: "educode-mobile",
    },
    {
      headers: {
        "Content-Type": "application/json",
      },
      validateStatus: (status) => status < 500,
    }
  );
  if (response.status === 200 && !response.data.messageCode) {
    return true;
  }

  return response.data.messageCode ?? "internet-connection-error";
}

export async function FetchAndSaveUserDataByUniId(uniId: string): Promise<boolean | string> {
  const token = await GetUserToken();
  const response = await axios.get(`${Constants.expoConfig?.extra?.EXPO_PUBLIC_API_URL}/User/UniId/${uniId}`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
    validateStatus: (status) => status < 500,
  });

  if (response.status === 200 && !response.data.messageCode) {
    const data: OnlineUserModel = response.data;
    const localData: LocalUserData = {
      userType: data.userType.userType,
      uniId: data.uniId,
      studentCode: data.studentCode,
      offlineOnly: false,
      fullName: data.fullName,
    };

    SaveOfflineUserData(localData);
    return true;
  }

  return response.data.messageCode ?? "internet-connection-error";
}

export async function RequestOTP(uniId: string, fullName?: string): Promise<boolean | string> {
  const response = await axios.post(
    `${Constants.expoConfig?.extra?.EXPO_PUBLIC_API_URL}/Auth/RequestOTP`,
    {
      uniId: uniId,
      fullName: fullName ?? null,
    },
    {
      headers: {
        "Content-Type": "application/json",
      },
      validateStatus: (status) => status < 500,
    }
  );

  if (response.status === 200 && !response.data.messageCode) {
    return true;
  }

  return response.data.messageCode ?? "internet-connection-error";
}

export async function VerifyOTP(model: VerifyOTPModel): Promise<boolean | string> {
  const response = await axios.post(
    `${Constants.expoConfig?.extra?.EXPO_PUBLIC_API_URL}/Auth/VerifyOTP`,
    {
      uniId: model.uniId,
      otp: model.otp,
    },
    {
      headers: {
        "Content-Type": "application/json",
      },
      validateStatus: (status) => status < 500,
    }
  );

  if (response.status === 200 && !response.data.messageCode) {
    response.data.token && SaveTempToken(response.data.token);
    return true;
  }

  return response.data.messageCode ?? "internet-connection-error";
}

export async function ChangeUserPassword(model: ChangePasswordModel): Promise<boolean | string> {
  const token = await GetTempToken();
  const response = await axios.patch(
    `${Constants.expoConfig?.extra?.EXPO_PUBLIC_API_URL}/Auth/ChangePassword`,
    {
      uniId: model.uniId,
      newPassword: model.newPassword,
    },
    {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
      validateStatus: (status) => status < 500,
    }
  );

  if (response.status === 200 && !response.data.messageCode) {
    DeleteTempToken();
    return true;
  }

  return response.data.messageCode ?? "internet-connection-error";
}

export async function DeleteUser(uniId: string): Promise<boolean | string> {
  const token = await GetUserToken();
  const response = await axios.delete(
    `${Constants.expoConfig?.extra?.EXPO_PUBLIC_API_URL}/User/Delete/UniId/${uniId}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
      validateStatus: (status) => status < 500,
    }
  );

  if (response.status === 200 && !response.data.messageCode) {
    return true;
  }

  return response.data.messageCode ?? "internet-connection-error";
}
