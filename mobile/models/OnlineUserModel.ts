interface OnlineUserModel {
  id: number;
  userType: {
    userType: string;
  };
  uniId: string;
  studentCode?: string;
  fullName: string;
}

export default OnlineUserModel;
