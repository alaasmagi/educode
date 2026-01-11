import { EAccessLevel } from "./EAccessLevel";

interface LocalUserData {
  uniId?: string;
  userType: string;
  accessLevel: EAccessLevel;
  studentCode?: string;
  offlineOnly: boolean;
}

export default LocalUserData;
