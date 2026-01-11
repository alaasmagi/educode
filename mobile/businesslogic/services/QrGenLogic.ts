export function GenerateQrString(studentCode: string, fullName: string, attendanceId: number, workplaceId: number) {
  const timestamp = Math.floor(Date.now() / 1000);
  const result =
    timestamp + ";" + attendanceId + ";" + workplaceId + ";" + fullName + ";" + studentCode;
  return result;
}
