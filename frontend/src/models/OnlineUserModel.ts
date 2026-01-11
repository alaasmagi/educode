import { EAccessLevel } from "./EAccessLevel";

interface OnlineUserModel {
  id: string;
  userType: string;
  accessLevel: EAccessLevel;
  studentCode?: string;
  uniId: string;
}

export default OnlineUserModel;
