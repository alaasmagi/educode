export const RegexFilters = {
  studentCode: /^\d{6}[A-Za-z]{4}$/,
  defaultId: /^\d{6}$/,
  attendanceScanId: /^\d{6}-\d{6}$/,
  attendanceCheckData: /^\d{10};\d{6};\d{6};[\p{L}\s-]+;\d{6}[A-Za-z]{4}$/u,
};
